using JakePerry.Reflection;
using System;
using System.Reflection;

namespace JakePerry
{
    public static class ExceptionUtility
    {
        public static void SetStackTrace(Exception exception, string stackTrace)
        {
            const BindingFlags kFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            if (exception is null) return;

            FieldInfo field = ReflectionEx.GetField(typeof(Exception), "_remoteStackTraceString", kFlags);

            string nl = Environment.NewLine;
            stackTrace += string.Concat(nl, SR.Exception_EndStackTraceFromPreviousThrow, nl);

            field.SetValue(exception, stackTrace);
        }
    }
}
