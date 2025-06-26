using JakePerry.Collections;
using JakePerry.Threading;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using static JakePerry.PySharp.BuiltIns;

namespace JakePerry
{
    internal sealed class MultiVersionHandle
    {
        /// <summary>
        /// Encapsulates the current state of a single handle.
        /// </summary>
        private struct State
        {
            public bool released;

            // When a handle is acquired/released, these fields will contain a reference
            // to the StackTrace object for debugging purposes. Only when an issue arises
            // will these objects be converted into a string representation.
            public object acquireTrace;
            public object releaseTrace;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static State Acquire(int skipFrames)
            {
                State state = default;

                StackTrace trace = new(skipFrames + 1, fNeedFileInfo: true);
                state.acquireTrace = trace;

                return state;
            }
        }

        private readonly IDisposable m_obj;
        private readonly ChunkList<State> m_stateList;

        private int m_liveCount;

        private SpinLockSlim m_lock = SpinLockSlim.Create();

        public ref SpinLockSlim Lock => ref m_lock;

        public IDisposable Obj => m_obj;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public MultiVersionHandle(IDisposable h)
        {
            Enforce.Argument(h, nameof(h)).IsNotNull();

            m_obj = h;
            m_stateList = new(chunkSize: 16)
            {
                State.Acquire(skipFrames: 2)
            };
        }

        ~MultiVersionHandle()
        {
            string traceToFinalizer = null;

            foreach (int i in range(m_stateList.Count))
            {
                ref State state = ref m_stateList.UnsafeGetByRef(i);

                if (!state.released)
                {
                    string traceStr = GetTraceString(ref state.acquireTrace);

                    Exception ex = new(
                        "Detected potential memory leak. A handle's underlying object was collected by the GC, but the handle was never released." +
                        $"\nHandle creation trace:\n{traceStr}\n--- End of stack trace from handle's creation location ---");

                    traceToFinalizer ??= new StackTrace().ToString();
                    ExceptionUtility.SetStackTrace(ex, traceToFinalizer);

                    JPDebug.LogException(ex);
                }
            }
        }

        private static string GetTraceString(ref object trace)
        {
            if (trace is not string traceString)
            {
                StackTrace trace2 = trace as StackTrace;
                Debug.Assert(trace2 is not null);

                traceString = trace2.ToString();
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

            string traceStr = GetTraceString(ref state.releaseTrace);
            message += $"\nHandle dispose trace:\n{traceStr}\n--- End of stack trace from handle's release location ---";

            return new ObjectDisposedException("Handle", message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
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

        [MethodImpl(MethodImplOptions.NoInlining)]
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

                    // Debug only: Record the stacktrace for the location of the Handle<T>.Dispose call.
                    state.releaseTrace = new StackTrace(skipFrames: 2, fNeedFileInfo: true);
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
                m_obj.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
