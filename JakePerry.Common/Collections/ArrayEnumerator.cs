using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    /// <summary>
    /// Emulates the internal SZArrayEnumerator type used by the core library
    /// for single-dimension arrays.
    /// </summary>
    public struct ArrayEnumerator<T> : IEnumerator, IEnumerator<T>
    {
        private readonly T[] m_array;
        private readonly int m_endIndex;
        private int m_index;

        public T Current => m_array[m_index];

        object IEnumerator.Current => this.Current;

        public ArrayEnumerator(T[] array)
        {
            _ = array ?? throw new ArgumentNullException(nameof(array));
            m_array = array;
            m_index = -1;
            m_endIndex = m_array.Length;
        }

        public bool MoveNext()
        {
            if (m_index < m_endIndex)
            {
                ++m_index;
                return m_index < m_endIndex;
            }
            return false;
        }

        public void Reset()
        {
            m_index = -1;
        }

        void IDisposable.Dispose() { }
    }
}
