using System;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    public sealed class TypeComparer : IComparer<Type>, IEqualityComparer<Type>
    {
        private static readonly TypeComparer _inst = new();

        public static TypeComparer Instance => _inst;

        public int Compare(Type x, Type y)
        {
            if (x is null) return y is null ? 0 : -1;
            if (y is null) return 1;

            return StringComparer.Ordinal.Compare(x.FullName, y.FullName);
        }

        public bool Equals(Type x, Type y)
        {
            return EqualityComparer<Type>.Default.Equals(x, y);
        }

        public int GetHashCode(Type obj)
        {
            return EqualityComparer<Type>.Default.GetHashCode(obj);
        }
    }
}
