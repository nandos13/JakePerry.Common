using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    public sealed class ChunkList<T> :
        IEnumerable,
        IEnumerable<T>,
        ICollection,
        ICollection<T>,
        IReadOnlyCollection<T>,
        IList,
        IList<T>,
        IReadOnlyList<T>
    {
        private readonly LinkedList<FixedSizeBuffer<T>> m_linkedList = new();
        private readonly int m_size;

        private object m_syncRoot;
        private int m_count;
        private int m_version;
        private LinkedListNode<FixedSizeBuffer<T>> m_head;

        public int Count => m_count;

        bool ICollection<T>.IsReadOnly => false;

        bool IList.IsReadOnly => false;

        bool ICollection.IsSynchronized => false;

        object ICollection.SyncRoot
        {
            get
            {
                if (m_syncRoot is null)
                {
                    System.Threading.Interlocked.CompareExchange(ref m_syncRoot, new object(), null);
                }

                return m_syncRoot;
            }
        }

        public ChunkList(int size)
        {
            if (size < 1) throw new ArgumentOutOfRangeException(nameof(size));
            m_size = size;
        }

        private static bool IsCompatibleObject(object value)
        {
            return value is T || (value == null && default(T) == null);
        }

        private void NextChunk()
        {
            m_head = m_linkedList.AddLast(new FixedSizeBuffer<T>(m_size));
        }

        public void Add(T item)
        {
            if (m_head is null || m_head.Value.IsFull)
            {
                NextChunk();
            }
            m_head.Value.Add(item);
            ++m_count;
            ++m_version;
        }

        public void Clear()
        {
            if (m_count > 0)
            {
                foreach (var node in m_linkedList) node.Clear();
                m_head = m_linkedList.First;
                m_count = 0;
            }
            ++m_version;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }

        public void CopyTo(int index, T[] array, int offset, int count)
        {
            _ = array ?? throw new ArgumentNullException(nameof(array));
            if (m_count - index < count)
            {
                ThrowHelperEx.ThrowArgumentException(ThrowHelperEx.Msg.Argument_InvalidOffLen);
            }

            //Array.Copy(src, srcIdx, dst, dstIdx, count);
            var node = m_linkedList.First;
            while (node != null)
            {
                if (index < m_size)
                {
                    //node.Value.CopyTo(array, offset);
                    xx;
                }
            }
        }

        public int IndexOf(T item)
        {
            int offset = 0;
            var node = m_linkedList.First;
            while (node is not null)
            {
                int index = node.Value.IndexOf(item);
                if (index > -1) return offset + index;

                if (node == m_head) break;

                offset += m_size;
                node = node.Next;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            xx;
        }

        int IList.Add(object value)
        {
            try
            {
                Add((T)value);
            }
            catch (InvalidCastException)
            {
                ThrowHelperEx.ThrowWrongValueTypeArgumentException(value, typeof(T));
            }

            return m_count - 1;
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value)) return Contains((T)value);
            return false;
        }

        int IList.IndexOf(object value)
        {
            if (IsCompatibleObject(value)) return IndexOf((T)value);
            return -1;
        }
    }
}
