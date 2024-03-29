using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    /// <summary>
    /// A buffer with a fixed size.
    /// </summary>
    internal sealed class FixedSizeBuffer<T> :
        IEnumerable,
        IEnumerable<T>,
        IReadOnlyCollection<T>,
        IReadOnlyList<T>
    {
        private readonly T[] m_buffer;
        private int m_count;

        internal int Capacity => m_buffer.Length;

        internal int Count => m_count;

        internal T this[int index]
        {
            get => m_buffer[index];
            set => m_buffer[index] = value;
        }

        int IReadOnlyCollection<T>.Count => this.Count;

        T IReadOnlyList<T>.this[int index]
        {
            get => this[index];
        }

        internal FixedSizeBuffer(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));

            m_buffer = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }

        internal bool Add(T item)
        {
            if (m_count != Capacity)
            {
                m_buffer[m_count++] = item;
                return true;
            }

            return false;
        }

        internal void Clear()
        {
            if (m_count > 0)
            {
                Array.Clear(m_buffer, 0, m_count);
                m_count = 0;
            }
        }

        internal void CopyTo(T[] array, int offset) => Array.Copy(m_buffer, 0, array, offset, m_count);

        internal void CopyTo(T[] array) => CopyTo(array, 0);

        internal bool Insert(int index, T item)
        {
            if (index < 0 || index > m_count) throw new ArgumentOutOfRangeException(nameof(index));

            if (m_count != Capacity)
            {
                if (index < m_count)
                {
                    Array.Copy(m_buffer, index, m_buffer, index + 1, m_count - index);
                }

                m_buffer[index] = item;
                ++m_count;
                return true;
            }

            return false;
        }

        internal void RemoveAt(int index)
        {
            if (index >= m_count) throw new ArgumentOutOfRangeException(nameof(index));

            m_count--;
            if (index < m_count)
            {
                Array.Copy(m_buffer, index + 1, m_buffer, index, m_count - index);
            }

            m_buffer[m_count] = default;
        }

        internal bool Remove(T item)
        {
            int index = Array.IndexOf(m_buffer, item);
            if (index > -1)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        internal void Pop()
        {
            if (m_count == 0) throw new InvalidOperationException();

            RemoveAt(m_count - 1);
        }

        internal ReadOnlyArray<T> AsReadOnly() => new ReadOnlyArray<T>(m_buffer);

        internal ArrayEnumerator<T> GetEnumerator() => new ArrayEnumerator<T>(m_buffer);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();
    }
}
