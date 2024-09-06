using JakePerry.Reflection;
using System;
using System.Reflection;

namespace JakePerry
{
    public static class ExceptionUtility
    {
        private static string Exception_EndStackTraceFromPreviousThrow
            => EnvironmentEx.GetResourceString("Exception_EndStackTraceFromPreviousThrow");

        public static void SetStackTrace(Exception exception, string stackTrace)
        {
            const BindingFlags kFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            if (exception is null) return;

            var field = ReflectionEx.GetField(typeof(Exception), "_remoteStackTraceString", kFlags);

            var nl = Environment.NewLine;
            stackTrace += string.Concat(nl, Exception_EndStackTraceFromPreviousThrow, nl);

            field.SetValue(exception, stackTrace);
        }
    }
}
