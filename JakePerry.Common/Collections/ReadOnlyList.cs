using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    /// <summary>
    /// A struct which wraps an instance of <see cref="List{T}"/> and provides an
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
    /// As such, this type can be used when you wish to pass an immutable list to some other area in code
    /// instead of casting the list to <see cref="IReadOnlyList{T}"/>.
    /// Given the information above, this will prevent additional boxing allocations &amp; associated
    /// performance costs when subsequent code enumerates the list in a <see langword="foreach"/> loop.
    /// </summary>
    public readonly struct ReadOnlyList<T> :
        IEquatable<ReadOnlyList<T>>,
        IEquatable<ReadOnlyList<T>?>,
        IEnumerable<T>,
        IReadOnlyCollection<T>,
        IReadOnlyList<T>
    {
        private static List<T> _empty;

        private readonly List<T> m_list;

        public T this[int index] => m_list[index];

        /// <inheritdoc cref="List{T}.Count"/>
        public int Count => m_list.Count;

        public ReadOnlyList(List<T> list)
        {
            m_list = list;
        }

        /// <inheritdoc cref="List{T}.BinarySearch(int, int, T, IComparer{T})"/>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return m_list.BinarySearch(index, count, item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T, IComparer{T})"/>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return m_list.BinarySearch(item, comparer);
        }

        /// <inheritdoc cref="List{T}.BinarySearch(T)"/>
        public int BinarySearch(T item)
        {
            return m_list.BinarySearch(item);
        }

        /// <inheritdoc cref="List{T}.Contains(T)"/>
        public bool Contains(T item) => m_list.Contains(item);

        /// <inheritdoc cref="List{T}.CopyTo(T[], int)"/>
        public void CopyTo(T[] array, int arrayIndex) => m_list.CopyTo(array, arrayIndex);

        /// <inheritdoc cref="List{T}.CopyTo(T[])"/>
        public void CopyTo(T[] array) => m_list.CopyTo(array);

        /// <inheritdoc cref="List{T}.CopyTo(int, T[], int, int)"/>
        public void CopyTo(int index, T[] array, int arrayIndex, int count) => m_list.CopyTo(index, array, arrayIndex, count);

        public void CopyTo(List<T> list)
        {
            _ = list ?? throw new ArgumentNullException(nameof(list));
            list.AddRange(m_list);
        }

        /// <inheritdoc cref="List{T}.IndexOf(T)"/>
        public int IndexOf(T item) => m_list.IndexOf(item);

        /// <inheritdoc cref="List{T}.IndexOf(T, int)"/>
        public int IndexOf(T item, int index) => m_list.IndexOf(item, index);

        /// <inheritdoc cref="List{T}.IndexOf(T, int, int)"/>
        public int IndexOf(T item, int index, int count) => m_list.IndexOf(item, index, count);

        /// <summary>
        /// Get the underlying list as an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <remarks>
        /// Use this method to avoid boxing when passing the list to a method expecting
        /// the IEnumerable interface type.
        /// </remarks>
        public IEnumerable<T> AsEnumerable() => m_list;

        /// <summary>
        /// Get the underlying list as an <see cref="IReadOnlyCollection{T}"/>.
        /// </summary>
        /// <inheritdoc cref="AsEnumerable"/>
        public IReadOnlyCollection<T> AsReadOnlyCollection() => m_list;

        /// <summary>
        /// Get the underlying list as an <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <inheritdoc cref="AsEnumerable"/>
        public IReadOnlyList<T> AsReadOnlyListInterface() => m_list;

        /// <inheritdoc cref="List{T}.ToArray"/>
        public T[] ToArray() => m_list.ToArray();

        public List<T>.Enumerator GetEnumerator() => m_list.GetEnumerator();

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();
#pragma warning restore HAA0601

        public bool Equals(ReadOnlyList<T> other)
        {
            return m_list == other.m_list;
        }

        public bool Equals(ReadOnlyList<T>? other)
        {
            var otherList = other.HasValue
                ? other.Value.m_list
                : null;

            return m_list == otherList;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return m_list is null;
            if (obj is List<T> other1) return m_list == other1;
            if (obj is ReadOnlyList<T> other2) return this.Equals(other2);

            return false;
        }

        public override int GetHashCode()
        {
            return EqualityComparer<List<T>>.Default.GetHashCode(m_list);
        }

        public static ReadOnlyList<T> Empty()
        {
            return new ReadOnlyList<T>(_empty ??= new());
        }

        public static bool operator ==(ReadOnlyList<T> x, ReadOnlyList<T> y) => x.Equals(y);
        public static bool operator !=(ReadOnlyList<T> x, ReadOnlyList<T> y) => !(x == y);

        /* Note: Implementing equality operators for the nullable type allows us to
         * compare this object to the null literal to check if the m_list field is null.
         * Eg: if (listReadOnly == null) { ... }
         * 
         * Without these, the comparison still compiles but will always evaluate to false.
         */

        public static bool operator ==(ReadOnlyList<T> x, ReadOnlyList<T>? y) => x.Equals(y);
        public static bool operator !=(ReadOnlyList<T> x, ReadOnlyList<T>? y) => !(x == y);

        public static bool operator ==(ReadOnlyList<T>? x, ReadOnlyList<T> y) => y.Equals(x);
        public static bool operator !=(ReadOnlyList<T>? x, ReadOnlyList<T> y) => !(x == y);

        public static implicit operator ReadOnlyList<T>(List<T> list)
        {
            return list is null ? Empty() : new ReadOnlyList<T>(list);
        }
    }
}
