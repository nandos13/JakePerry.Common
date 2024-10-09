using JakePerry.Reflection;
using System;
using System.Reflection;

namespace JakePerry
{
    /// <summary>
    /// Partially exposes the internal System.SR class.
    /// </summary>
    internal static partial class SR
    {
        private const BindingFlags kFlags = BindingFlags.Static | BindingFlags.NonPublic;

        private static readonly Type[] _format3Args = new Type[4] { typeof(string), typeof(object), typeof(object), typeof(object) };

        /// <summary>
        /// Get the internal System.SR class via reflection
        /// </summary>
        internal static Type SRClass => ReflectionEx.GetType(typeof(object).Assembly, "System.SR");

        /// <summary>
        /// Get the single argument format method: SR.Format(string, object)
        /// </summary>
        internal static MethodInfo Format1Method
            => ReflectionEx.GetMethod(SRClass, "Format", kFlags, new ParamsArray<Type>(typeof(string), typeof(object)));

        /// <summary>
        /// Get the two argument format method: SR.Format(string, object, object)
        /// </summary>
        internal static MethodInfo Format2Method
            => ReflectionEx.GetMethod(SRClass, "Format", kFlags, new ParamsArray<Type>(typeof(string), typeof(object), typeof(object)));

        /// <summary>
        /// Get the three argument format method: SR.Format(string, object, object, object)
        /// </summary>
        internal static MethodInfo Format3Method
            => ReflectionEx.GetMethod(SRClass, "Format", kFlags, new ParamsArray<Type>(_format3Args));

        /// <summary>
        /// Get the GetResourceString(string) method
        /// </summary>
        internal static MethodInfo GetResourceStringMethod
            => ReflectionEx.GetMethod(SRClass, "GetResourceString", kFlags, new ParamsArray<Type>(typeof(string)));

        internal static string Format(string resourceFormat, object p1)
        {
            using var scope = ReflectionEx.RentArrayWithArgsInScope(out var array, resourceFormat, p1);
            return (string)Format1Method.Invoke(null, array);
        }

        internal static string Format(string resourceFormat, object p1, object p2)
        {
            using var scope = ReflectionEx.RentArrayWithArgsInScope(out var array, resourceFormat, p1, p2);
            return (string)Format2Method.Invoke(null, array);
        }

        internal static string Format(string resourceFormat, object p1, object p2, object p3)
        {
            using var scope = ReflectionEx.RentArrayWithArgsInScope(out var array, resourceFormat, p1, p2, p3);
            return (string)Format3Method.Invoke(null, array);
        }

        internal static string GetResourceString(string resourceKey)
        {
            using var scope = ReflectionEx.RentArrayWithArgsInScope(out var array, resourceKey);
            return (string)GetResourceStringMethod.Invoke(null, array);
        }

        internal static string GetResourceString(string resourceKey, string defaultString)
        {
            string resource = GetResourceString(resourceKey);

            return resource is null || resource == resourceKey ? defaultString : resource;
        }
    }
}
