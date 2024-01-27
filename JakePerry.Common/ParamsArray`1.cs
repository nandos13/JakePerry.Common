using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// A generic alternative to the <see cref="ParamsArray"/> struct.
    /// </summary>
    internal readonly struct ParamsArray<T> : IEquatable<ParamsArray<T>>, IEnumerable, IEnumerable<T>
    {
        // Sentinel fixed-length arrays eliminate the need for a "count" field keeping this
        // struct down to just 4 fields. These are only used for their "Length" property,
        // that is, their elements are never set or referenced.
        private static readonly T[] s_oneArgArray = new T[1];
        private static readonly T[] s_twoArgArray = new T[2];
        private static readonly T[] s_threeArgArray = new T[3];

        private readonly T m_arg0;
        private readonly T m_arg1;
        private readonly T m_arg2;

        // After construction, the first three elements of this array will never be accessed
        // because the indexer will retrieve those values from arg0, arg1, and arg2.
        private readonly T[] m_args;

        public ParamsArray(T arg0)
        {
            this.m_arg0 = arg0;
            this.m_arg1 = default;
            this.m_arg2 = default;

            // Always assign this.args to make use of its "Length" property
            this.m_args = s_oneArgArray;
        }

        public ParamsArray(T arg0, T arg1)
        {
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = default;

            // Always assign this.args to make use of its "Length" property
            this.m_args = s_twoArgArray;
        }

        public ParamsArray(T arg0, T arg1, T arg2)
        {
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = arg2;

            // Always assign this.args to make use of its "Length" property
            this.m_args = s_threeArgArray;
        }

        public ParamsArray(T[] args)
        {
            _ = args ?? throw new ArgumentNullException(nameof(args));
            int len = args.Length;
            this.m_arg0 = len > 0 ? args[0] : default;
            this.m_arg1 = len > 1 ? args[1] : default;
            this.m_arg2 = len > 2 ? args[2] : default;
            this.m_args = args;
        }

        public static ParamsArray<T> Empty => new ParamsArray<T>(Array.Empty<T>());

        public int Length => this.m_args is null ? 0 : this.m_args.Length;

        private T GetAtSlow(int index)
        {
            if (index == 1)
                return this.m_arg1;
            if (index == 2)
                return this.m_arg2;
            return this.m_args[index];
        }

        public T this[int index] => index == 0 ? this.m_arg0 : GetAtSlow(index);

        /// <summary>
        /// An object that enumerates all arguments in the <see cref="ParamsArray{T}"/> object which spawned it.
        /// </summary>
        public struct Enumerator : IEnumerator, IEnumerator<T>
        {
            private readonly ParamsArray<T> m_args;
            private int m_index;
            private T m_currentValue;

            public T Current => m_currentValue;

            object IEnumerator.Current => Current;

            public Enumerator(ParamsArray<T> args)
            {
                this.m_args = args;
                m_index = 0;
                m_currentValue = default;
            }

            public bool MoveNext()
            {
                int count = m_args.Length;
                while ((uint)m_index < (uint)count)
                {
                    m_currentValue = m_args[m_index];
                    m_index++;
                    return true;
                }
                m_index = count + 1;
                m_currentValue = default;
                return false;
            }

            public void Reset()
            {
                m_index = 0;
                m_currentValue = default;
            }

            public void Dispose() { /* Not implemented */ }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
        // Justification: Boxing is unavoidable when this object is cast to
        // an IEnumerable or IEnumerable<T>.
        // Boxing will not occur for cases where this object is the source object in
        // a foreach loop, as the ParamsArray::GetEnumerator() method will be invoked.

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

#pragma warning restore HAA0601

        /// <summary>
        /// Copy contents to a new array.
        /// </summary>
        /// <remarks>This is an allocating call.</remarks>
        public T[] ToArray()
        {
            var len = Length;
            if (len == 0) return Array.Empty<T>();

            var copy = new T[len];
            copy[0] = m_arg0;
            for (int i = 1; i < len; ++i)
            {
                copy[i] = GetAtSlow(i);
            }

            return copy;
        }

        public bool Equals(ParamsArray<T> other, IEqualityComparer<T> elementComparer)
        {
            _ = elementComparer ?? throw new ArgumentNullException(nameof(elementComparer));

            if (Length != other.Length)
                return false;

            // GetEnumerator() returns a value-type enumerator object. This will not allocate memory on the heap.
            var enumeratorX = GetEnumerator();
            var enumeratorY = other.GetEnumerator();

            // Iterate through all elements of each ParamsArray
            while (enumeratorX.MoveNext() & enumeratorY.MoveNext())
            {
                T currentX = enumeratorX.Current;
                T currentY = enumeratorY.Current;

                // Both enumerators moved to a new element, compare it
                if (!elementComparer.Equals(currentX, currentY))
                    return false;
            }

            return true;
        }

        public bool Equals(ParamsArray<T> other)
        {
            return Equals(other, EqualityComparer<T>.Default);
        }

        public override bool Equals(object obj) => (obj is ParamsArray<T> other) && this.Equals(other);

        public int GetHashCode(EqualityComparer<T> elementComparer)
        {
            _ = elementComparer ?? throw new ArgumentNullException(nameof(elementComparer));

            int hash = 29;

            // GetEnumerator() returns a value-type enumerator object.
            // The foreach recognizes this at compile-time and will avoid boxing to IEnumerator.
            foreach (T element in this)
            {
                hash *= 31 + elementComparer.GetHashCode(element);
            }

            return hash;
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<T>.Default);
        }

        public static bool operator ==(ParamsArray<T> arg0, ParamsArray<T> arg1)
        {
            return arg0.Equals(arg1);
        }

        public static bool operator !=(ParamsArray<T> arg0, ParamsArray<T> arg1)
        {
            return !arg0.Equals(arg1);
        }
    }
}
