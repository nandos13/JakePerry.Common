#if !JP_HANDLE_DISABLE_DEBUG
#define DEBUG_HANDLE_TRACES
#endif

using JakePerry.Collections;
using JakePerry.Threading;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using static JakePerry.PySharp.BuiltIns;

namespace JakePerry
{
    // TODO: Move doc, rename?
    // TODO: Consider a version that just works on disposable,
    // then a value-holding struct that wraps the typeless one
    public interface IHandle<out T> : IDisposable
    {
        T Value { get; }
    }

    // TODO: Revise documentation

    public readonly struct Handle<T> : IDisposable, IStructWithDefaultCheck
    {
        private sealed class Inner
        {
            /// <summary>
            /// Encapsulates the current state of a single handle.
            /// </summary>
            private struct State
            {
                public bool released;

#if DEBUG_HANDLE_TRACES
                // When a handle is acquired/released, these fields will contain a reference
                // to the StackTrace object for debugging purposes. Only when an issue arises
                // will these objects be converted into a string representation.
                public object acquireTrace;
                public object releaseTrace;
#endif

#if DEBUG_HANDLE_TRACES
                [MethodImpl(MethodImplOptions.NoInlining)]
#else
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
                public static State Acquire(int skipFrames)
                {
                    State state = default;

#if DEBUG_HANDLE_TRACES
                    StackTrace trace = new(skipFrames + 1, fNeedFileInfo: true);
                    state.acquireTrace = trace;
#endif

                    return state;
                }
            }

            private readonly IHandle<T> m_handle;
            private readonly ChunkList<State> m_stateList;

            private int m_liveCount;

            private SpinLockSlim m_lock = SpinLockSlim.Create();

            public ref SpinLockSlim Lock => ref m_lock;

            public IHandle<T> Implementation => m_handle;

#if DEBUG_HANDLE_TRACES
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            public Inner(IHandle<T> h)
            {
                m_handle = h;
                m_stateList = new(chunkSize: 16)
                {
                    State.Acquire(skipFrames: 2)
                };
            }

#if DEBUG_HANDLE_TRACES
            ~Inner()
            {
                foreach (int i in range(m_stateList.Count))
                {
                    ref State state = ref m_stateList.UnsafeGetByRef(i);

                    if (!state.released)
                    {
                        string traceStr = GetTraceString(ref state.acquireTrace);

                        Exception ex = new(
                            "Detected potential memory leak. A handle's underlying object was collected by the GC, but the handle was never released." +
                            $"\nHandle creation trace:\n{traceStr}\n--- End of stack trace from handle's creation location ---");

                        JPDebug.LogException(ex);
                    }
                }
            }
#endif

            private static string GetTraceString(ref object trace)
            {
                if (trace is not string traceString)
                {
                    StackTrace trace2 = trace as StackTrace;
                    Debug.Assert(trace2 is not null);

                    StringBuilder sb = StringBuilderCache.Acquire();

                    foreach (int i in range(trace2.FrameCount))
                    {
                        StackFrame frame = trace2.GetFrame(i);

                        sb.AppendLine(frame.ToString());
                    }

                    traceString = StringBuilderCache.GetStringAndRelease(sb);
                    trace = traceString;
                }

                return traceString;
            }

            public bool CheckReleased(int version, bool lockAndCheck)
            {
                JPDebug.Assert(version >= 0);
                JPDebug.Assert(m_stateList.Count > version);

                // Check once without locking
                if (m_stateList[version].released) return true;

                // If we don't need to be 100% accurate, just assume it's not disposed or disposing yet.
                if (!lockAndCheck) return false;

                // Acquire the spin lock
                bool acquiredLock = false;
                try
                {
                    m_lock.AcquireLock(ref acquiredLock);

                    return m_stateList[version].released;
                }
                finally { m_lock.ReleaseLock(acquiredLock); }
            }

            public Exception CreateReleasedExceptionNoLock(int version, string message)
            {
                JPDebug.Assert(version >= 0);
                JPDebug.Assert(m_stateList.Count > version);

                ref State state = ref m_stateList.UnsafeGetByRef(version);
                JPDebug.Assert(state.released);

#if DEBUG
                string traceStr = GetTraceString(ref state.releaseTrace);
                message += $"\nHandle dispose trace:\n{traceStr}\n--- End of stack trace from handle's release location ---";
#endif

                return new ObjectDisposedException("Handle", message);
            }

#if DEBUG_HANDLE_TRACES
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            public int AcquireNewHandle(int fromVersion)
            {
                JPDebug.Assert(fromVersion >= 0);
                JPDebug.Assert(m_stateList.Count > fromVersion);

                bool handleWasAlreadyReleased = false;
                int newVersion = -1;

                // Acquire the spin lock. Minimal work is performed within the lock scope.
                bool acquiredLock = false;
                try
                {
                    m_lock.AcquireLock(ref acquiredLock);

                    if (m_stateList[fromVersion].released)
                    {
                        handleWasAlreadyReleased = true;
                    }
                    else
                    {
                        newVersion = m_stateList.Count;
                        m_stateList.Add(default);
                    }
                }
                finally { m_lock.ReleaseLock(acquiredLock); }

                if (handleWasAlreadyReleased)
                {
                    throw CreateReleasedExceptionNoLock(fromVersion,
                        "Cannot acquire a new handle; The current handle has been disposed. Additional hadles should be acquired sooner.");
                }

                State newState = State.Acquire(skipFrames: 2);
                m_stateList[newVersion] = newState;

                return newVersion;
            }

#if DEBUG_HANDLE_TRACES
            [MethodImpl(MethodImplOptions.NoInlining)]
#endif
            public void Release(int version)
            {
                JPDebug.Assert(version >= 0);
                JPDebug.Assert(m_stateList.Count > version);

                bool handleWasAlreadyReleased = false;
                bool countReachedZero = false;

                // Acquire the spin lock. Minimal work is performed within the lock scope.
                bool acquiredLock = false;
                try
                {
                    m_lock.AcquireLock(ref acquiredLock);

                    ref State state = ref m_stateList.UnsafeGetByRef(version);

                    if (state.released)
                    {
                        handleWasAlreadyReleased = true;
                    }
                    else
                    {
                        // Mark the versioned handle as released &amp; check if this was the last
                        // live handle to be released.
                        state.released = true;
                        countReachedZero = --m_liveCount == 0;

#if DEBUG
                        // Debug only: Record the stacktrace for the location of the Handle<T>.Dispose call.
                        state.releaseTrace = new StackTrace(skipFrames: 2, fNeedFileInfo: true);
#endif
                    }
                }
                finally { m_lock.ReleaseLock(acquiredLock); }

                if (handleWasAlreadyReleased)
                {
                    throw CreateReleasedExceptionNoLock(version,
                        "Cannot dispose handle; The current handle has already been disposed.");
                }

                // If the count reached zero, the implementation can be disposed.
                if (countReachedZero)
                {
                    m_handle.Dispose();
                    GC.SuppressFinalize(this);
                }
            }
        }

        private readonly Inner m_inner;
        private readonly int m_version;

        public bool IsDefaultValue => m_inner is null;

        /// <summary>
        /// The resource held by this handle.
        /// </summary>
        public readonly T Value
        {
            get
            {
                if (m_inner is null) return default;

                if (m_inner.CheckReleased(m_version, false))
                {
                    throw m_inner.CreateReleasedExceptionNoLock(m_version,
                        "Cannot get value from handle; The current handle has been disposed.");
                }

                return m_inner.Implementation.Value;
            }
        }

#if DEBUG_HANDLE_TRACES
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif
        public Handle(IHandle<T> implementation)
        {
            Enforce.Argument(implementation, nameof(implementation)).IsNotNull();

            m_inner = new Inner(implementation);
            m_version = 0;
        }

        private Handle(Inner inner, int version)
        {
            m_inner = inner;
            m_version = version;
        }

        /// <summary>
        /// Acquire a copy of this handle. The underlying resource will not be disposed until both the current
        /// handle and the returned handle are disposed.
        /// </summary>
#if DEBUG_HANDLE_TRACES
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif
        public Handle<T> Acquire()
        {
            if (m_inner is null) return default;

            int v = m_inner.AcquireNewHandle(m_version);

            return new(m_inner, v);
        }

#if DEBUG_HANDLE_TRACES
        [MethodImpl(MethodImplOptions.NoInlining)]
#endif
        public void Dispose()
        {
            m_inner?.Release(m_version);
        }
    }
}
