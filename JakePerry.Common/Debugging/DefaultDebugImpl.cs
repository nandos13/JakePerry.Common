using System;
using System.Diagnostics;

namespace JakePerry.Debugging
{
    /// <summary>
    /// Default implementation for the <see cref="IDebugImpl"/> interface.
    /// </summary>
    internal sealed class DefaultDebugImpl : IDebugImpl
    {
        private static readonly DefaultDebugImpl _inst = new();

        internal static DefaultDebugImpl Instance => _inst;

        private static string HandleTrace(string message, string trace)
        {
            const string kJoin = ",\n";

            if (string.IsNullOrEmpty(trace)) return message;
            if (string.IsNullOrEmpty(message)) return trace;

            return string.Concat(message, kJoin, trace);
        }

        internal void Assert(bool condition, string message)
        {
            System.Diagnostics.Debug.Assert(condition, message);
        }

        internal void LogError(string stackTrace, string message)
        {
            if (stackTrace is not null)
            {
                message = HandleTrace(message, stackTrace);
            }
            Console.Error.WriteLine(message);
        }

        internal void LogInfo(bool trace, string message)
        {
            if (trace)
            {
                var stackTrace = Environment.StackTrace;
                message = HandleTrace(message, stackTrace);
            }
            Console.Out.WriteLine(message);
        }

        void IDebugImpl.Assert(bool condition, string message)
        {
            Assert(condition, message);
        }

        void IDebugImpl.LogError(bool trace, string message)
        {
            var stackTrace = trace ? Environment.StackTrace : null;
            LogError(stackTrace, message);
        }

        void IDebugImpl.LogError(StackTrace trace, string message)
        {
            var stackTrace = trace?.ToString();
            LogError(stackTrace, message);
        }

        void IDebugImpl.LogInfo(bool trace, string message)
        {
            LogInfo(trace, message);
        }

        void IDebugImpl.LogException(Exception exception)
        {
            var message = exception?.ToString();
            if (message is not null)
            {
                LogError(null, message);
            }
        }
    }
}
