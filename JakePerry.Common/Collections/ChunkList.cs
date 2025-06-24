using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JakePerry.Collections
{
    public class ChunkList<T> : IEnumerable<T>
    {
        private const int DefaultChunkSize = 64;

        internal sealed class Chunk
        {
            public readonly T[] items;

            public int count;
            public Chunk next;

            public Chunk(int size)
            {
                items = new T[size];
            }

            public bool IsFull
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => count == items.Length;
            }
        }

        public struct Enumerator : IEnumerator, IEnumerator<T>
        {
            private Chunk m_chunk;
            private int m_current;

            public readonly T Current => m_chunk.items[m_current];

            readonly object IEnumerator.Current => Current;

            internal Enumerator(Chunk chunk, int startIndex = 0)
            {
                Enforce.Argument(chunk, nameof(chunk)).IsNotNull();
                Enforce.Argument(startIndex, nameof(startIndex)).IsValidIndex(chunk.count);

                m_chunk = chunk;
                m_current = startIndex - 1;
            }

            public bool MoveNext()
            {
                int next = m_current + 1;
                if (next >= m_chunk.count)
                {
                    m_chunk = m_chunk.next;
                    m_current = 0;

                    return m_chunk is not null;
                }
                else
                {
                    m_current = next;
                    return true;
                }
            }

            readonly void IEnumerator.Reset() => throw new NotSupportedException();

            readonly void IDisposable.Dispose() { }
        }

        private readonly int m_chunkSize;

        private Chunk m_head;
        private Chunk m_tail;

        private int m_count;

        public int Count => m_count;

        public ChunkList(int chunkSize = DefaultChunkSize)
        {
            Enforce.Argument(chunkSize, nameof(chunkSize)).IsGreaterThan(0);

            m_chunkSize = chunkSize;
            m_head = m_tail = new Chunk(m_chunkSize);
        }

        public void Add(T item)
        {
            if (m_tail.IsFull)
            {
                Chunk newChunk = new(m_chunkSize);
                m_tail.next = newChunk;
                m_tail = newChunk;
            }

            m_tail.items[m_tail.count++] = item;
            ++m_count;
        }

        public T this[int index]
        {
            get
            {
                Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

                (int localIndex, Chunk chunk) = GetChunkAndLocalIndex(index);
                return chunk.items[localIndex];
            }
            set
            {
                Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

                (int localIndex, Chunk chunk) = GetChunkAndLocalIndex(index);
                chunk.items[localIndex] = value;
            }
        }

        private (int localIndex, Chunk chunk) GetChunkAndLocalIndex(int index)
        {
            Chunk chunk = m_head;
            while (chunk != null)
            {
                if (index < chunk.count)
                {
                    return (index, chunk);
                }

                index -= chunk.count;
                chunk = chunk.next;
            }

            // Note: Callers are responsible for ensuring the index is in range.
            // This exception will probably only arise from unsafe concurrent access.
            throw new ArgumentOutOfRangeException();
        }

        public ref T UnsafeGetByRef(int index)
        {
            Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

            (int localIndex, Chunk chunk) = GetChunkAndLocalIndex(index);
            return ref chunk.items[localIndex];
        }

        private void InsertIntoChunk(Chunk chunk, int index, T item)
        {
            // If the chunk is full, we need to make room
            if (chunk.IsFull)
            {
                Chunk newChunk = new(m_chunkSize)
                {
                    next = chunk.next
                };
                chunk.next = newChunk;

                // Move last item to the new chunk
                newChunk.items[0] = chunk.items[chunk.count - 1];
                newChunk.count = 1;
                --chunk.count;

                if (chunk == m_tail)
                    m_tail = newChunk;
            }

            // Shift items to make space
            for (int i = chunk.count; i > index; --i)
            {
                chunk.items[i] = chunk.items[i - 1];
            }

            chunk.items[index] = item;
            ++chunk.count;
        }

        public void Insert(int index, T item)
        {
            Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

            (int localIndex, Chunk chunk) = GetChunkAndLocalIndex(index);

            InsertIntoChunk(chunk, localIndex, item);
            ++m_count;
        }

        public void RemoveAt(int index)
        {
            static void RemoveFromChunk(Chunk chunk, int index)
            {
                // Shift items down
                for (int i = index; i < chunk.count - 1; ++i)
                {
                    chunk.items[i] = chunk.items[i + 1];
                }

                --chunk.count;

                // Remove the item reference from the array so it can be collected by the GC
                chunk.items[chunk.count] = default;
            }

            Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

            (int localIndex, Chunk chunk) = GetChunkAndLocalIndex(index);

            RemoveFromChunk(chunk, localIndex);
            --m_count;
        }

        public void Clear()
        {
            m_head = m_tail = new Chunk(m_chunkSize);
            m_count = 0;
        }

        public Enumerator GetEnumerator() => new(m_head);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
