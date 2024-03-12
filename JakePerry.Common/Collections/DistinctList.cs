using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    /// <summary>
    /// A list of distinct items. Maintains a List&lt;T&gt; for enumeration
    /// performance and a HashSet&lt;T&gt; for lookup performance.
    /// <para>
    /// This collection does not provide methods to insert an item at a
    /// particular index and as such is intended for use in situations where
    /// the order of the collection is unimportant.
    /// </para>
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0502:Explicit new reference type allocation")]
    public sealed class DistinctList<T> : IEnumerable, IEnumerable<T>, IReadOnlyCollection<T>, ICollection<T>, IReadOnlyList<T>
    {
        // Default capacity of List<T>
        private const int _defaultCapacity = 4;

        /// <summary>
        /// Avoids a null reference when GetEnumerator is invoked but the
        /// collections have not been allocated.
        /// </summary>
        private static readonly List<T> _emptyList = new List<T>();

        private List<T> m_list;
        private HashSet<T> m_set;

        public int Count => m_list?.Count ?? 0;

        bool ICollection<T>.IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                if (m_list is null)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return m_list[index];
            }
        }

        /// <summary>
        /// Ensure collections have been created if they were not assigned in the constructor.
        /// </summary>
        private void InitCollections()
        {
            // There should never be any valid case where one collection is null but the other is set,
            // so it's safe for us to only check if one is null here.
            if (m_list is null)
            {
                m_list = new List<T>();
                m_set = new HashSet<T>();
            }
        }

        public DistinctList() { }

        public DistinctList(int capacity, IEqualityComparer<T> comparer)
        {
            m_list = new List<T>(capacity);
            m_set = comparer is null ? new HashSet<T>() : new HashSet<T>(comparer);
        }

        public DistinctList(int capacity) : this(capacity, null) { }

        public DistinctList(IEqualityComparer<T> comparer) : this(_defaultCapacity, comparer) { }

        public DistinctList(IEnumerable<T> collection, IEqualityComparer<T> comparer) : this(comparer)
        {
            AddRange(collection);
        }

        public DistinctList(IEnumerable<T> collection) : this(_defaultCapacity, null)
        {
            AddRange(collection);
        }

        public bool Add(T item)
        {
            InitCollections();

            if (!m_set.Add(item))
                return false;

            m_list.Add(item);
            return true;
        }

        public bool AddRange(IEnumerable<T> collection)
        {
            bool modified = false;

            if (collection != null)
                foreach (var item in collection)
                    modified |= Add(item);

            return modified;
        }

        public void Clear()
        {
            m_list?.Clear();
            m_set?.Clear();
        }

        public bool Contains(T item)
        {
            return m_set?.Contains(item) ?? false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_list?.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (m_set is null || !m_set.Remove(item))
                return false;

            m_list.Remove(item);
            return true;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            if (m_set is null)
            {
                // An empty set can still be equal if the other set is also empty.
                if (other != null)
                {
                    if (other is IReadOnlyCollection<T> c)
                        return c.Count == 0;

#pragma warning disable HAA0401 // Possible allocation of reference type enumerator
                    foreach (var _ in other)
                        return false;
#pragma warning restore HAA0401
                }

                return true;
            }

            return m_set.SetEquals(other);
        }

        void ICollection<T>.Add(T item) => this.Add(item);

        public List<T>.Enumerator GetEnumerator()
        {
            return m_list is null
                ? _emptyList.GetEnumerator()
                : m_list.GetEnumerator();
        }

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

#pragma warning restore HAA0601
    }
}
