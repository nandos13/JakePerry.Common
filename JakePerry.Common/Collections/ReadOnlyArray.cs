﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    /// <summary>
    /// A struct which wraps an instance of <typeparamref name="T"/>[] and provides an
    /// interface for read-only operations.
    /// <para/>
    /// This type's main purpose is to take advantage of an optimization performed
    /// by the compiler regarding <see langword="foreach"/> loops. Explanation below:
    /// <para/>
    /// When a collection's type is not known (ie. a method iterates over an <see cref="IEnumerable"/>
    /// object instance), the compiler emits a local <see cref="IEnumerator"/> variable which
    /// is used by the loop; However, the compiler is able to optimize the loop when the collection's
    /// type can be determined at compile-time.
    /// It does so by finding the GetEnumerator() method declared by the collection type and
    /// directly using the return type for the local enumerator variable.
    /// <para/>
    /// The <see cref="List{T}"/> type is one example that takes advantage of this and implements its own
    /// enumerator type as a struct to prevent allocations on every foreach loop.
    /// <para/>
    /// As such, this type can be used when you wish to pass an immutable array to some other area in code
    /// instead of casting the array to <see cref="IReadOnlyList{T}"/>.
    /// Given the information above, this will prevent additional boxing allocations &amp; associated
    /// performance costs when subsequent code enumerates the array in a <see langword="foreach"/> loop.
    /// </summary>
    public readonly struct ReadOnlyArray<T> :
        IEquatable<ReadOnlyArray<T>>,
        IEquatable<ReadOnlyArray<T>?>,
        IEnumerable<T>,
        IReadOnlyCollection<T>,
        IReadOnlyList<T>,
        IMightBeValid
    {
        private readonly T[] m_array;

        public readonly T this[int index] => m_array[index];

        /// <inheritdoc cref="Array.Length"/>
        public readonly int Length => m_array.Length;

        public readonly bool IsValid => m_array is not null;

        int IReadOnlyCollection<T>.Count => this.Length;

        public ReadOnlyArray(T[] array)
        {
            m_array = array;
        }

        /// <inheritdoc cref="Array.BinarySearch{T}(T[], int, int, T, IComparer{T})"/>
        public readonly int BinarySearch(int index, int length, T item, IComparer<T> comparer)
        {
            return Array.BinarySearch(m_array, index, length, item, comparer);
        }

        /// <inheritdoc cref="Array.BinarySearch{T}(T[], int, int, T)"/>
        public readonly int BinarySearch(int index, int length, T item)
        {
            return Array.BinarySearch(m_array, index, length, item);
        }

        /// <inheritdoc cref="Array.BinarySearch{T}(T[], T, IComparer{T})"/>
        public readonly int BinarySearch(T item, IComparer<T> comparer)
        {
            return Array.BinarySearch(m_array, item, comparer);
        }

        /// <inheritdoc cref="Array.BinarySearch{T}(T[], T)"/>
        public readonly int BinarySearch(T item)
        {
            return Array.BinarySearch(m_array, item);
        }

        /// <summary>
        /// Determines whether an element is in the <typeparamref name="T"/>[].
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <typeparamref name="T"/>[]. The value can
        /// be <see langword="null"/> for reference types.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> is found in the
        /// <typeparamref name="T"/>[]; otherwise, <see langword="false"/>.
        /// </returns>
        public readonly bool Contains(T item) => Array.IndexOf(m_array, item) > -1;

        /// <summary>
        /// Copy the array's contents to a new array.
        /// <para/>
        /// Note that this is an allocating call.
        /// </summary>
        public readonly T[] Copy()
        {
            if (m_array is null) throw new InvalidOperationException();
            var len = m_array.Length;
            var cpy = new T[len];
            Array.Copy(m_array, 0, cpy, 0, len);
            return cpy;
        }

        public readonly void CopyTo(T[] array, int index) => m_array.CopyTo(array, index);

        public readonly void CopyTo(Span<T> destination) => m_array.CopyTo(destination);

        public readonly void CopyTo(Memory<T> destination) => m_array.CopyTo(destination);

        public readonly void CopyTo(List<T> list)
        {
            Enforce.Argument(list, nameof(list)).IsNotNull();

            list.AddRange(m_array);
        }

        public readonly int IndexOf(T item) => Array.IndexOf(m_array, item);

        public readonly int IndexOf(T item, int startIndex) => Array.IndexOf(m_array, item, startIndex);

        public readonly int IndexOf(T item, int startIndex, int count) => Array.IndexOf(m_array, item, startIndex, count);

        /// <summary>
        /// Get the underlying array as an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <remarks>
        /// Use this method to avoid boxing when passing the array to a method expecting
        /// the IEnumerable interface type.
        /// </remarks>
        public readonly IEnumerable<T> AsEnumerable() => m_array;

        /// <summary>
        /// Get the underlying array as an <see cref="IReadOnlyCollection{T}"/>.
        /// </summary>
        /// <inheritdoc cref="AsEnumerable"/>
        public readonly IReadOnlyCollection<T> AsReadOnlyCollection() => m_array;

        /// <summary>
        /// Get the underlying array as an <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <inheritdoc cref="AsEnumerable"/>
        public readonly IReadOnlyList<T> AsReadOnlyList() => m_array;

        public readonly ArrayEnumerator<T> GetEnumerator() => new(m_array);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

        public readonly bool Equals(ReadOnlyArray<T> other)
        {
            return m_array == other.m_array;
        }

        public readonly bool Equals(ReadOnlyArray<T>? other)
        {
            var otherList = other.HasValue
                ? other.Value.m_array
                : null;

            return m_array == otherList;
        }

        public readonly override bool Equals(object obj)
        {
            if (obj is null) return m_array is null;
            if (obj is T[] other1) return m_array == other1;
            if (obj is ReadOnlyList<T> other2) return this.Equals(other2);

            return false;
        }

        public readonly override int GetHashCode()
        {
            return EqualityComparer<T[]>.Default.GetHashCode(m_array);
        }

        public static ReadOnlyArray<T> Empty()
        {
            return new ReadOnlyArray<T>(Array.Empty<T>());
        }

        public static bool operator ==(ReadOnlyArray<T> x, ReadOnlyArray<T> y) => x.Equals(y);
        public static bool operator !=(ReadOnlyArray<T> x, ReadOnlyArray<T> y) => !(x == y);

        /* Note: Implementing equality operators for the nullable type allows us to
         * compare this object to the null literal to check if the m_array field is null.
         * Eg: if (arrayReadOnly == null) { ... }
         * 
         * Without these, the comparison still compiles but will always evaluate to false.
         */

        public static bool operator ==(ReadOnlyArray<T> x, ReadOnlyArray<T>? y) => x.Equals(y);
        public static bool operator !=(ReadOnlyArray<T> x, ReadOnlyArray<T>? y) => !(x == y);

        public static bool operator ==(ReadOnlyArray<T>? x, ReadOnlyArray<T> y) => y.Equals(x);
        public static bool operator !=(ReadOnlyArray<T>? x, ReadOnlyArray<T> y) => !(x == y);

        public static implicit operator ReadOnlyArray<T>(T[] array)
        {
            return array is null ? Empty() : new ReadOnlyArray<T>(array);
        }
    }
}
