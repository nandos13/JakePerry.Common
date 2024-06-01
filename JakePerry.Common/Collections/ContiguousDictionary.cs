using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    public sealed class ContiguousDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        IEnumerable,
        IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private readonly List<KeyValuePair<TKey, TValue>> m_buffer = new();

        private readonly KeyOnlyComparer<TKey, TValue> m_comparer;

        public int Count => m_buffer?.Count ?? 0;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        private int BinarySearch(in TKey key)
        {
            var dummy = new KeyValuePair<TKey, TValue>(key, default);
            return m_buffer.BinarySearch(dummy, m_comparer);
        }

        public TValue this[TKey key]
        {
            get
            {
                _ = key ?? throw new ArgumentNullException(nameof(key));

                if (m_buffer is not null)
                {
                    int index = BinarySearch(in key);
                    if (index > -1)
                    {
                        return m_buffer[index].Value;
                    }
                }
                throw new KeyNotFoundException();
            }
            set
            {
                _ = key ?? throw new ArgumentNullException(nameof(key));
                Insert(in key, in value, add: false);
            }
        }

        public KeyCollection Keys => new KeyCollection(this);

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => this.Keys;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        public ValueCollection Values => new ValueCollection(this);

        ICollection<TValue> IDictionary<TKey, TValue>.Values => this.Values;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

        public ContiguousDictionary()
        {
            m_comparer = KeyOnlyComparer<TKey, TValue>.Default;
        }

        public ContiguousDictionary(Comparers<TKey> comparers)
        {
            m_comparer = new KeyOnlyComparer<TKey, TValue>(comparers);
        }

        public ContiguousDictionary(IComparer<TKey> comparer, IEqualityComparer<TKey> equalityComparer)
        {
            m_comparer = new KeyOnlyComparer<TKey, TValue>(comparer, equalityComparer);
        }

        public ReadOnlyList<KeyValuePair<TKey, TValue>> AsReadOnlyList()
        {
            return m_buffer;
        }

        public List<KeyValuePair<TKey, TValue>>.Enumerator GetEnumerator()
        {
            return m_buffer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private void Insert(in TKey key, in TValue value, bool add)
        {
            int index = BinarySearch(in key);
            if (index > -1)
            {
                if (add)
                {
                    ThrowHelperEx.ThrowArgumentException(ThrowHelperEx.Msg.Argument_AddingDuplicate);
                }
                else
                {
                    m_buffer[index] = new KeyValuePair<TKey, TValue>(key, value);
                }
            }
            else
            {
                m_buffer.Insert(~index, new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public void Add(TKey key, TValue value)
        {
            _ = key ?? throw new ArgumentNullException(nameof(key));
            Insert(in key, in value, add: true);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            m_buffer?.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return BinarySearch(in key) > -1;
        }

        public bool ContainsValue(TValue value)
        {
            if (value == null)
            {
                foreach (var pair in m_buffer)
                {
                    if (pair.Value == null) return true;
                }
            }
            else
            {
                var c = EqualityComparer<TValue>.Default;
                foreach (var pair in m_buffer)
                {
                    if (c.Equals(pair.Value, value)) return true;
                }
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return m_buffer?.Contains(item) ?? false;
        }

        private static void PrepareCopyTo<T>(T[] array, int arrayIndex, List<KeyValuePair<TKey, TValue>> buffer)
        {
            _ = array ?? throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (array.Length - arrayIndex < buffer.Count)
            {
                ThrowHelperEx.ThrowArgumentException(ThrowHelperEx.Msg.Arg_ArrayPlusOffTooSmall);
            }
        }

        private void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            PrepareCopyTo(array, arrayIndex, m_buffer);
            m_buffer?.CopyTo(array, arrayIndex);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        public bool Remove(TKey key)
        {
            int index = BinarySearch(in key);
            if (index > -1)
            {
                m_buffer.RemoveAt(index);
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            int index = BinarySearch(item.Key);
            if (index > -1 && EqualityComparer<TValue>.Default.Equals(m_buffer[index].Value, item.Value))
            {
                m_buffer.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = BinarySearch(in key);
            if (index > -1)
            {
                value = m_buffer[index].Value;
                return true;
            }

            value = default;
            return false;
        }

        public readonly struct KeyCollection : ICollection<TKey>, IEnumerable, IEnumerable<TKey>
        {
            public struct Enumerator : IEnumerator, IEnumerator<TKey>
            {
                private readonly List<KeyValuePair<TKey, TValue>> m_buffer;
                private List<KeyValuePair<TKey, TValue>>.Enumerator m_enumerator;

                public TKey Current => m_enumerator.Current.Key;

                object IEnumerator.Current => this.Current;

                public Enumerator(ContiguousDictionary<TKey, TValue> dictionary)
                {
                    m_buffer = dictionary.m_buffer;
                    m_enumerator = m_buffer.GetEnumerator();
                }

                public bool MoveNext()
                {
                    return m_enumerator.MoveNext();
                }

                public void Reset()
                {
                    var newEnumerator = m_buffer.GetEnumerator();
                    ((IEnumerator)m_enumerator).Reset();

                    m_enumerator = newEnumerator;
                }

                void IDisposable.Dispose() { }
            }

            private readonly ContiguousDictionary<TKey, TValue> m_dictionary;

            public int Count => m_dictionary.Count;

            bool ICollection<TKey>.IsReadOnly => true;

            public KeyCollection(ContiguousDictionary<TKey, TValue> dictionary)
            {
                m_dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            }

            void ICollection<TKey>.Add(TKey item) { ThrowHelperEx.ThrowNotSupportedException(ThrowHelperEx.Msg.NotSupported_KeyCollectionSet); }

            void ICollection<TKey>.Clear() { ThrowHelperEx.ThrowNotSupportedException(ThrowHelperEx.Msg.NotSupported_KeyCollectionSet); }

            bool ICollection<TKey>.Contains(TKey item) { return m_dictionary.ContainsKey(item); }

            void ICollection<TKey>.CopyTo(TKey[] array, int arrayIndex)
            {
                var buffer = m_dictionary.m_buffer;
                PrepareCopyTo(array, arrayIndex, buffer);

                int count = buffer.Count;
                for (int i = 0; i < count; ++i)
                {
                    array[arrayIndex++] = buffer[i].Key;
                }
            }

            bool ICollection<TKey>.Remove(TKey item) { ThrowHelperEx.ThrowNotSupportedException(ThrowHelperEx.Msg.NotSupported_KeyCollectionSet); return false; }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(m_dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        public readonly struct ValueCollection : ICollection<TValue>, IEnumerable, IEnumerable<TValue>
        {
            public struct Enumerator : IEnumerator, IEnumerator<TValue>
            {
                private readonly List<KeyValuePair<TKey, TValue>> m_buffer;
                private List<KeyValuePair<TKey, TValue>>.Enumerator m_enumerator;

                public TValue Current => m_enumerator.Current.Value;

                object IEnumerator.Current => this.Current;

                public Enumerator(ContiguousDictionary<TKey, TValue> dictionary)
                {
                    m_buffer = dictionary.m_buffer;
                    m_enumerator = m_buffer.GetEnumerator();
                }

                public bool MoveNext()
                {
                    return m_enumerator.MoveNext();
                }

                public void Reset()
                {
                    var newEnumerator = m_buffer.GetEnumerator();
                    ((IEnumerator)m_enumerator).Reset();

                    m_enumerator = newEnumerator;
                }

                void IDisposable.Dispose() { }
            }

            private readonly ContiguousDictionary<TKey, TValue> m_dictionary;

            public int Count => m_dictionary.Count;

            bool ICollection<TValue>.IsReadOnly => true;

            public ValueCollection(ContiguousDictionary<TKey, TValue> dictionary)
            {
                m_dictionary = dictionary ?? throw new ArgumentNullException(nameof(dictionary));
            }

            void ICollection<TValue>.Add(TValue item) { ThrowHelperEx.ThrowNotSupportedException(ThrowHelperEx.Msg.NotSupported_KeyCollectionSet); }

            void ICollection<TValue>.Clear() { ThrowHelperEx.ThrowNotSupportedException(ThrowHelperEx.Msg.NotSupported_KeyCollectionSet); }

            bool ICollection<TValue>.Contains(TValue item) { return m_dictionary.ContainsValue(item); }

            void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
            {
                var buffer = m_dictionary.m_buffer;
                PrepareCopyTo(array, arrayIndex, buffer);

                int count = buffer.Count;
                for (int i = 0; i < count; ++i)
                {
                    array[arrayIndex++] = buffer[i].Value;
                }
            }

            bool ICollection<TValue>.Remove(TValue item) { ThrowHelperEx.ThrowNotSupportedException(ThrowHelperEx.Msg.NotSupported_KeyCollectionSet); return false; }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(m_dictionary);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

    }
}
