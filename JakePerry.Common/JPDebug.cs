using JakePerry.Debugging;
using JakePerry.Threading;
using System;

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

        private static IDebugImpl GetImpl()
        {
            var impl = _implThread;
            if (impl == null)
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

        private static string Stringify(object message)
        {
            if (message is string s) return s;
            if (message is null) return string.Empty;

            return message.ToString();
        }

        /// <inheritdoc cref="IDebugImpl.Assert(bool, string)"/>
        internal static void Assert(bool condition, string message)
        {
            GetImpl()?.Assert(condition, message);
        }

        /// <inheritdoc cref="IDebugImpl.Assert(bool, string)"/>
        internal static void Assert(bool condition)
        {
            Assert(condition, "Assertion failure.");
        }

        /// <inheritdoc cref="IDebugImpl.LogError(bool, string)"/>
        internal static void LogError(bool trace, object message)
        {
            GetImpl()?.LogError(trace, Stringify(message));
        }

        /// <inheritdoc cref="IDebugImpl.LogError(bool, string)"/>
        internal static void LogError(object message)
        {
            LogError(true, message);
        }

        /// <inheritdoc cref="IDebugImpl.LogInfo(bool, string)"/>
        internal static void LogInfo(bool trace, object message)
        {
            GetImpl()?.LogInfo(trace, Stringify(message));
        }

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

        internal static void SetImplementationForThread(IDebugImpl impl)
        {
            _implThread = impl;
        }

        internal static void ResetImplementation()
        {
            SetImplementation(DefaultDebugImpl.Instance);
        }

        internal static void ResetImplementationForThread()
        {
            SetImplementationForThread(DefaultDebugImpl.Instance);
        }
    }
}
