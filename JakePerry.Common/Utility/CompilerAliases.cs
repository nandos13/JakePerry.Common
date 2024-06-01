using JakePerry.Collections;
using System;

namespace JakePerry
{
    /// <summary>
    /// Provides a means to access the compiler alias for certain types,
    /// ie. Int32 has alias <see langword="int"/>.
    /// </summary>
    internal static class CompilerAliases
    {
        private static readonly ContiguousDictionary<Type, string> _lookup = new(Comparers<Type>.Create(TypeComparer.Instance))
        {
            [typeof(void)] = "void",
            [typeof(bool)] = "bool",
            [typeof(bool?)] = "bool?",
            [typeof(byte)] = "byte",
            [typeof(byte?)] = "byte?",
            [typeof(sbyte)] = "sbyte",
            [typeof(sbyte?)] = "sbyte?",
            [typeof(char)] = "char",
            [typeof(char?)] = "char?",
            [typeof(float)] = "float",
            [typeof(float?)] = "float?",
            [typeof(double)] = "double",
            [typeof(double?)] = "double?",
            [typeof(decimal)] = "decimal",
            [typeof(decimal?)] = "decimal?",
            [typeof(short)] = "short",
            [typeof(short?)] = "short?",
            [typeof(ushort)] = "ushort",
            [typeof(ushort?)] = "ushort?",
            [typeof(int)] = "int",
            [typeof(int?)] = "int?",
            [typeof(uint)] = "uint",
            [typeof(uint?)] = "uint?",
            [typeof(long)] = "long",
            [typeof(long?)] = "long?",
            [typeof(ulong)] = "ulong",
            [typeof(ulong?)] = "ulong?",
            [typeof(nint)] = "nint",
            [typeof(nint?)] = "nint?",
            [typeof(nuint)] = "nuint",
            [typeof(nuint?)] = "nuint?",
            [typeof(string)] = "string",
            [typeof(object)] = "object"
        };

        /// <summary>
        /// Get the compiler alias for a specified type.
        /// </summary>
        /// <returns>
        /// Returns the compiler alias that represents type <paramref name="t"/> if it has one;
        /// Otherwise, returns <see langword="null"/>.
        /// </returns>
        internal static string GetAlias(Type t)
        {
            return _lookup.TryGetValue(t, out string alias) ? alias : null;
        }
    }
}
