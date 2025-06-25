using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JakePerry.Collections
{
    /// <summary>
    /// A generic collection that provides dynamic array functionality using chunked storage for
    /// optimized memory management.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the collection.
    /// </typeparam>
    public class ChunkList<T> : IEnumerable,
        IEnumerable<T>,
        IReadOnlyCollection<T>,
        ICollection<T>,
        IReadOnlyList<T>,
        IList<T>
    {
        private const int DefaultChunkSize = 64;

        /// <remarks>
        /// This type implements the <see cref="IEnumerable"/> interface purely to allow use of the
        /// 'collection initializer' syntax. Enumerating this type is not supported
        /// </remarks>
        internal sealed class Chunk : IEnumerable
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

            /// <summary>
            /// Adds an item, no validation performed.
            /// This method only exists to open up the Add syntax when initializing a new chunk.
            /// </summary>
            internal void Add(T item)
            {
                // Note: Though this method doesn't validate, still don't want to mutate the count property
                // right away, just in case we're going out of bounds on a full array.
                int c = count + 1;

                items[c] = item;
                count = c;
            }

            IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
        }

        /// <summary>
        /// Enumerates the elements of a <see cref="ChunkList{T}"/>.
        /// </summary>
        public struct Enumerator : IEnumerator, IEnumerator<T>
        {
            private Chunk m_chunk;
            private int m_current;

            public readonly T Current => m_chunk.items[m_current];

            readonly object IEnumerator.Current => Current;

            public Enumerator(ChunkList<T> list)
            {
                Enforce.Argument(list, nameof(list)).IsNotNull();

                m_chunk = list.m_head;
                m_current = -1;
            }

            internal Enumerator(Chunk chunk, int startIndex = 0)
            {
                Enforce.Argument(chunk, nameof(chunk)).IsNotNull();

                // Allow enumeration if the ChunkList is empty. This is the only case where an empty
                // chunk is possible. All non-head chunks are guaranteed to contain at least one item.
                if (chunk.count > 0)
                {
                    Enforce.Argument(startIndex, nameof(startIndex)).IsValidIndex(chunk.count);
                }

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

        private readonly Chunk m_head;
        private Chunk m_tail;

        private int m_count;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ChunkList{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="ChunkList{T}"/>.
        /// </returns>
        public int Count => m_count;

        bool ICollection<T>.IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

                GetChunkIndex(index, out int localIndex, out Chunk chunk);
                return chunk.items[localIndex];
            }
            set
            {
                Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

                GetChunkIndex(index, out int localIndex, out Chunk chunk);
                chunk.items[localIndex] = value;
            }
        }

        public ChunkList(int chunkSize = DefaultChunkSize)
        {
            Enforce.Argument(chunkSize, nameof(chunkSize)).IsGreaterThan(0);

            m_chunkSize = chunkSize;
            m_head = m_tail = new Chunk(m_chunkSize);
        }

        private void GetChunkIndex(int index, out int localIndex, out Chunk chunk)
        {
            chunk = m_head;
            while (chunk != null)
            {
                if (index < chunk.count)
                {
                    localIndex = index;
                    return;
                }

                index -= chunk.count;
                chunk = chunk.next;
            }

            // Note: Callers are responsible for ensuring the index is in range.
            // This exception will probably only arise from unsafe concurrent access.
            throw new ArgumentOutOfRangeException();
        }

        private bool Find(T item, out int index, out int localIndex, out Chunk chunk)
        {
            int offset = 0;
            chunk = m_head;
            do
            {
                localIndex = Array.IndexOf(chunk.items, item, 0, chunk.count);
                if (localIndex > -1)
                {
                    return TryGet.Pass(out index, localIndex + offset);
                }
                offset += chunk.count;
                chunk = chunk.next;
            }
            while (chunk is not null);

            return TryGet.Fail(out index, -1);
        }

        /// <summary>
        /// Returns a reference to the element at the specified index, allowing for direct manipulation without copying.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element.
        /// </param>
        /// <returns>
        /// A reference to the element at the specified index.
        /// </returns>
        /// <remarks>
        /// This method provides direct access to the internal storage.
        /// Use with caution as it bypasses normal encapsulation.
        /// </remarks>
        public ref T UnsafeGetByRef(int index)
        {
            Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

            GetChunkIndex(index, out int localIndex, out Chunk chunk);
            return ref chunk.items[localIndex];
        }

        public void Add(T item)
        {
            if (m_tail.IsFull)
            {
                Chunk newChunk = new(m_chunkSize) { item };
                m_tail.next = newChunk;
                m_tail = newChunk;
            }
            else
            {
                m_tail.items[m_tail.count++] = item;
            }
            ++m_count;
        }

        public void Clear()
        {
            m_tail = m_head;

            m_head.next = null;
            m_head.count = 0;
            m_count = 0;

            Array.Clear(m_head.items, 0, m_head.items.Length);
        }

        public bool Contains(T item)
        {
            return IndexOf(item) > -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Enforce.Argument(array, nameof(array)).IsNotNull();
            Enforce.Argument(arrayIndex, nameof(arrayIndex)).IsGreaterThanOrEqual(0);

            if (arrayIndex + m_count > array.Length)
            {
                throw new ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
            }

            if (m_count == 0) return;

            int currentIndex = arrayIndex;
            Chunk chunk = m_head;

            do
            {
                Array.Copy(chunk.items, 0, array, currentIndex, chunk.count);
                currentIndex += chunk.count;
                chunk = chunk.next;
            }
            while (chunk is not null);
        }

        public int IndexOf(T item)
        {
            return Find(item, out int index, out _, out _) ? index : -1;
        }

        private void InsertIntoChunk(Chunk chunk, int localIndex, T item)
        {
            while (chunk is not null)
            {
                if (!chunk.IsFull)
                {
                    // Chunk has space, shift items and insert
                    if (localIndex < chunk.count)
                    {
                        Array.Copy(chunk.items, localIndex, chunk.items, localIndex + 1, chunk.count - localIndex);
                    }

                    chunk.items[localIndex] = item;
                    ++chunk.count;
                    return;
                }
                else
                {
                    // Chunk is full. Cascade into subsequent chunk
                    T lastItem = chunk.items[chunk.count - 1];

                    // Shift items to make space for the new item
                    if (localIndex < chunk.count - 1)
                    {
                        Array.Copy(chunk.items, localIndex, chunk.items, localIndex + 1, chunk.count - 1 - localIndex);
                    }

                    chunk.items[localIndex] = item;

                    // Current chunk's last item becomes the item to insert into next chunk
                    item = lastItem;
                    localIndex = 0;

                    chunk = chunk.next;
                }
            }

            // Code will only reach this point if the tail chunk is at full capacity.
            // In this case, we need to create a new chunk to add onto the tail.
            Chunk newChunk = new(m_chunkSize) { item };

            m_tail.next = newChunk;
            m_tail = newChunk;
        }

        public void Insert(int index, T item)
        {
            Enforce.Argument(index, nameof(index)).IsBetween(0, m_count, maxDisplay: nameof(Count));

            GetChunkIndex(index, out int localIndex, out Chunk chunk);

            InsertIntoChunk(chunk, localIndex, item);
            ++m_count;
        }

        private void RemoveFromChunk(Chunk chunk, int localIndex)
        {
            while (chunk is not null)
            {
                if (localIndex < chunk.count - 1)
                {
                    // Shift items within the current chunk
                    Array.Copy(chunk.items, localIndex + 1, chunk.items, localIndex, chunk.count - localIndex - 1);
                }

                // If there's a next chunk, move its first item to fill the last position
                if (chunk.next is not null)
                {
                    chunk.items[chunk.count - 1] = chunk.next.items[0];
                    if (chunk.next.count > 1)
                    {
                        // Move to the next chunk, starting at the beginning
                        chunk = chunk.next;
                        localIndex = 0;
                    }
                    else
                    {
                        JPDebug.Assert(m_tail == chunk.next);

                        // The next chunk only had one element, which we just moved to this chunk.
                        chunk.next = null;
                        m_tail = chunk;
                    }
                }
                else
                {
                    // This is the last chunk or next chunk is empty
                    --chunk.count;
                    chunk.items[chunk.count] = default;
                    chunk = null;
                }
            }
        }

        public bool Remove(T item)
        {
            if (Find(item, out _, out int localIndex, out Chunk chunk))
            {
                RemoveFromChunk(chunk, localIndex);
                --m_count;
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            Enforce.Argument(index, nameof(index)).IsValidIndex(m_count);

            GetChunkIndex(index, out int localIndex, out Chunk chunk);

            RemoveFromChunk(chunk, localIndex);
            --m_count;
        }

        public Enumerator GetEnumerator() => new(m_head);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
