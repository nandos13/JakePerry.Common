using System;

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

            var sb = StringBuilderCache.Acquire(capacity: message.Length + kJoin.Length + trace.Length);
            sb.Append(message);
            sb.Append(kJoin);
            sb.Append(trace);

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        internal void Assert(bool condition, string message)
        {
            System.Diagnostics.Debug.Assert(condition, message);
        }

        internal void LogError(bool trace, string message)
        {
            if (trace)
            {
                var stackTrace = Environment.StackTrace;
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
            LogError(trace, message);
        }

        void IDebugImpl.LogInfo(bool trace, string message)
        {
            LogInfo(trace, message);
        }
    }
}
