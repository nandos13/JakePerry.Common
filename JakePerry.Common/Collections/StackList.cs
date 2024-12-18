using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace JakePerry.Collections
{
    internal ref struct StackList<T>
    {
        private Span<T> m_span;
        private T[] m_fromPool;
        private int m_pos;

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_pos;
        }

        public StackList(Span<T> span)
        {
            m_span = span;
            m_pos = 0;
        }

        public ref T this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                JPDebug.Assert(i > -1 && i < m_pos);
                return ref m_span[i];
            }
        }

        private void Expand(int required)
        {
            int l = m_span.Length;
            int capacity = Math.Max(l != 0 ? l * 2 : 4, l + required);

            var array = ArrayPool<T>.Shared.Rent(capacity);
            m_span.CopyTo(array);

            var toReturn = m_fromPool;
            m_span = m_fromPool = array;

            if (toReturn is not null)
            {
                ArrayPool<T>.Shared.Return(toReturn);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ExpandAndAppend(T item)
        {
            // This should only be called when we've filled the span.
            JPDebug.Assert(m_pos == m_span.Length);

            int pos = m_pos;
            Expand(1);

            m_span[pos] = item;
            m_pos = pos + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T item)
        {
            int pos = m_pos;
            Span<T> span = m_span;

            if ((uint)pos < (uint)span.Length)
            {
                span[pos] = item;
                m_pos = pos + 1;
            }
            else
            {
                ExpandAndAppend(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Peek()
        {
            int pos = m_pos - 1;
            if ((uint)pos < 0)
            {
                throw new InvalidOperationException("List empty");
            }

            Span<T> span = m_span;

            ref T item = ref span[pos];

            return ref item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(ref T item)
        {
            int pos = m_pos - 1;
            if ((uint)pos < 0)
            {
                item = default;
                return false;
            }

            Span<T> span = m_span;

            item = ref span[pos];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            int pos = m_pos - 1;
            if ((uint)pos < 0)
            {
                throw new InvalidOperationException("List empty");
            }

            Span<T> span = m_span;

            T item = span[pos];
            m_pos = pos;

            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T item)
        {
            int pos = m_pos - 1;
            if ((uint)pos < 0)
            {
                item = default;
                return false;
            }

            Span<T> span = m_span;

            item = span[pos];
            m_pos = pos;

            return true;
        }

        public ReadOnlySpan<T> AsSpan()
        {
            return m_span.Slice(0, m_pos);
        }

        public T[] ToArray()
        {
            return AsSpan().ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            var toReturn = m_fromPool;
            if (toReturn is not null)
            {
                m_fromPool = null;
                ArrayPool<T>.Shared.Return(toReturn);
            }
        }
    }
}
