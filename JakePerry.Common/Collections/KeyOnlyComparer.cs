using System;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    /// <summary>
    /// Compares the keys of two <see cref="KeyValuePair{TKey, TValue}"/>
    /// instances and ignores their values.
    /// </summary>
    public sealed class KeyOnlyComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>, IEqualityComparer<KeyValuePair<TKey, TValue>>
    {
        private static KeyOnlyComparer<TKey, TValue> _default;

        private readonly Comparers<TKey> m_cmp;

        /// <summary>
        /// Get a comparer instance which uses <see cref="EqualityComparer{T}.Default"/> to compare keys.
        /// </summary>
        public static KeyOnlyComparer<TKey, TValue> Default => _default ??= new();

        private KeyOnlyComparer()
        {
            m_cmp = Comparers<TKey>.GetDefault(throwIfNotComparable: true);
        }

        public KeyOnlyComparer(Comparers<TKey> comparers)
        {
            if (comparers.IsNull) throw new ArgumentNullException(nameof(comparers));
            m_cmp = comparers;
        }

        public KeyOnlyComparer(IComparer<TKey> order, IEqualityComparer<TKey> equality)
        {
            m_cmp = new Comparers<TKey>(order, equality);
        }

        public int Compare(KeyValuePair<TKey, TValue> arg0, KeyValuePair<TKey, TValue> arg1)
        {
            return m_cmp.Compare(arg0.Key, arg1.Key);
        }

        public bool Equals(KeyValuePair<TKey, TValue> arg0, KeyValuePair<TKey, TValue> arg1)
        {
            return m_cmp.Equals(arg0.Key, arg1.Key);
        }

        public int GetHashCode(KeyValuePair<TKey, TValue> arg)
        {
            return m_cmp.GetHashCode(arg.Key);
        }
    }
}
