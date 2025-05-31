using System;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    public abstract class SubstringComparer : IComparer<Substring>, IEqualityComparer<Substring>
    {
        private static readonly SubstringComparer _ordinal = new OrdinalComparer();

        public static SubstringComparer Ordinal => _ordinal;

        public abstract int Compare(Substring x, Substring y);

        public abstract bool Equals(Substring x, Substring y);
        public abstract int GetHashCode(Substring obj);

        internal sealed class OrdinalComparer : SubstringComparer
        {
            public override int Compare(Substring x, Substring y)
            {
                return x.CompareTo(y, StringComparison.Ordinal);
            }

            public override bool Equals(Substring x, Substring y)
            {
                return x.Equals(y, StringComparison.Ordinal);
            }

            public override int GetHashCode(Substring obj)
            {
                Enforce.Argument(obj.SourceString, nameof(obj)).IsNotNull();

                return obj.GetHashCode();
            }
        }
    }
}
