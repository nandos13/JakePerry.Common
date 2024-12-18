using JakePerry.Debugging;
using JakePerry.Threading;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace JakePerry
{
    /// <summary>
    /// Provides API for common debugging methods.
    /// </summary>
    internal static class JPDebug
    {
        private static SpinLockSlim _lock = SpinLockSlim.Create();

        private static IDebugImpl _impl = DefaultDebugImpl.Instance;

        [ThreadStatic]
        private static IDebugImpl _implThread;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IDebugImpl GetImpl()
        {
            var impl = _implThread;
            if (impl is null)
            {
                bool acquiredLock = false;
                try
                {
                    _lock.AcquireLock(ref acquiredLock);

                    impl = _impl;
                }
                finally { _lock.ReleaseLock(acquiredLock); }
            }

            return impl;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string Stringify(object message)
        {
            if (message is string s) return s;
            if (message is null) return string.Empty;

            return message.ToString();
        }

        /// <inheritdoc cref="IDebugImpl.Assert(bool, string)"/>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Assert(bool condition, string message)
        {
            GetImpl()?.Assert(condition, message);
        }

        /// <inheritdoc cref="IDebugImpl.Assert(bool, string)"/>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Assert(bool condition)
        {
            Assert(condition, "Assertion failure.");
        }

        /// <inheritdoc cref="IDebugImpl.LogError(bool, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError(bool trace, object message)
        {
            GetImpl()?.LogError(trace, Stringify(message));
        }

        /// <inheritdoc cref="IDebugImpl.LogError(StackTrace, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError(StackTrace trace, object message)
        {
            GetImpl()?.LogError(trace, Stringify(message));
        }

        /// <inheritdoc cref="IDebugImpl.LogError(bool, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError(object message)
        {
            LogError(true, message);
        }

        /// <inheritdoc cref="IDebugImpl.LogInfo(bool, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogInfo(bool trace, object message)
        {
            GetImpl()?.LogInfo(trace, Stringify(message));
        }

        /// <inheritdoc cref="IDebugImpl.LogException(Exception)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogException(Exception exception)
        {
            GetImpl()?.LogException(exception);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void SetImplementation(IDebugImpl impl)
        {
            bool acquiredLock = false;
            try
            {
                _lock.AcquireLock(ref acquiredLock);

                _impl = impl;
            }
            finally { _lock.ReleaseLock(acquiredLock); }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetImplementationForThread(IDebugImpl impl)
        {
            _implThread = impl;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ResetImplementation()
        {
            SetImplementation(DefaultDebugImpl.Instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ResetImplementationForThread()
        {
            SetImplementationForThread(DefaultDebugImpl.Instance);
        }
    }
}
