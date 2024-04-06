using System;
using System.Reflection;

namespace JakePerry
{
    /// <summary>
    /// Exposes some internal methods declared by the <see cref="Environment"/> class.
    /// </summary>
    internal static class EnvironmentEx
    {
        private static MethodInfo Method_GetResourceString
        {
            get
            {
                const BindingFlags kFlags = BindingFlags.Static | BindingFlags.NonPublic;

                var parameters = new ParamsArray<Type>(typeof(string), typeof(object[]));
                return ReflectionEx.GetMethod(typeof(Environment), "GetResourceString", kFlags, parameters);
            }
        }

        internal static string GetResourceString(string key, params object[] args)
        {
            var invocationArgs = ReflectionEx.RentArrayWithArguments(key, args);

            var result = Method_GetResourceString.Invoke(null, invocationArgs);

            ReflectionEx.ReturnArray(invocationArgs);

            return (string)result;
        }

        internal static string GetResourceString(string key)
        {
            return GetResourceString(key, Array.Empty<object>());
        }
    }
}
