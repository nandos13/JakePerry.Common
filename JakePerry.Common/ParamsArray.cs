using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// This struct is a modification of the internal System.ParamsArray
    /// type used by the mscorlib library.
    /// <para>
    /// This type should be used in cases where a method can accept an array of arguments of any length,
    /// but expects 3 or less in most cases.
    /// </para>
    /// </summary>
    internal readonly struct ParamsArray : IEquatable<ParamsArray>, IEnumerable
    {
        // Sentinel fixed-length arrays eliminate the need for a "count" field keeping this
        // struct down to just 4 fields. These are only used for their "Length" property,
        // that is, their elements are never set or referenced.
        private static readonly object[] s_oneArgArray = new object[1];
        private static readonly object[] s_twoArgArray = new object[2];
        private static readonly object[] s_threeArgArray = new object[3];

        private readonly object m_arg0;
        private readonly object m_arg1;
        private readonly object m_arg2;

        // After construction, the first three elements of this array will never be accessed
        // because the indexer will retrieve those values from arg0, arg1, and arg2.
        private readonly object[] m_args;

        public ParamsArray(object arg0)
        {
            this.m_arg0 = arg0;
            this.m_arg1 = null;
            this.m_arg2 = null;

            // Always assign this.m_args to make use of its "Length" property
            this.m_args = s_oneArgArray;
        }

        public ParamsArray(object arg0, object arg1)
        {
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = null;

            // Always assign this.m_args to make use of its "Length" property
            this.m_args = s_twoArgArray;
        }

        public ParamsArray(object arg0, object arg1, object arg2)
        {
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = arg2;

            // Always assign this.m_args to make use of its "Length" property
            this.m_args = s_threeArgArray;
        }

        public ParamsArray(object[] args)
        {
            _ = args ?? throw new ArgumentNullException(nameof(args));
            int len = args.Length;
            this.m_arg0 = len > 0 ? args[0] : null;
            this.m_arg1 = len > 1 ? args[1] : null;
            this.m_arg2 = len > 2 ? args[2] : null;
            this.m_args = args;
        }

        public static ParamsArray Empty => new ParamsArray(Array.Empty<object>());

        public int Length => this.m_args is null ? 0 : this.m_args.Length;

        private object GetAtSlow(int index)
        {
            if (index == 1)
                return this.m_arg1;
            if (index == 2)
                return this.m_arg2;
            return this.m_args[index];
        }

        public object this[int index] => index == 0 ? this.m_arg0 : GetAtSlow(index);

        /// <summary>
        /// An object that enumerates all arguments in the <see cref="ParamsArray"/> object which spawned it.
        /// </summary>
        public struct Enumerator : IEnumerator
        {
            private readonly ParamsArray m_args;
            private int m_index;
            private object m_currentValue;

            public object Current => m_currentValue;

            public Enumerator(ParamsArray args)
            {
                this.m_args = args;
                m_index = 0;
                m_currentValue = null;
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
                m_currentValue = null;
                return false;
            }

            public void Reset()
            {
                m_index = 0;
                m_currentValue = null;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
        // Justification: Boxing is unavoidable when this object is cast to
        // an IEnumerable.
        // Boxing will not occur for cases where this object is the source object in
        // a foreach loop, as the ParamsArray::GetEnumerator() method will be invoked.

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

#pragma warning restore HAA0601

        public bool Equals(ParamsArray other, EqualityComparer<object> elementComparer)
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
                object currentX = enumeratorX.Current;
                object currentY = enumeratorY.Current;

                // Both enumerators moved to a new element, compare it
                if (!elementComparer.Equals(currentX, currentY))
                    return false;
            }

            return true;
        }

        public bool Equals(ParamsArray other)
        {
            return Equals(other, EqualityComparer<object>.Default);
        }

        public override bool Equals(object obj) => (obj is ParamsArray other) && this.Equals(other);

        public int GetHashCode(EqualityComparer<object> elementComparer)
        {
            _ = elementComparer ?? throw new ArgumentNullException(nameof(elementComparer));

            int hash = 29;

            // GetEnumerator() returns a value-type enumerator object.
            // The foreach recognizes this at compile-time and will avoid boxing to IEnumerator.
            foreach (object element in this)
            {
                hash *= 31 + elementComparer.GetHashCode(element);
            }

            return hash;
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<object>.Default);
        }

        public static bool operator ==(ParamsArray arg0, ParamsArray arg1)
        {
            return arg0.Equals(arg1);
        }

        public static bool operator !=(ParamsArray arg0, ParamsArray arg1)
        {
            return !arg0.Equals(arg1);
        }
    }
}
