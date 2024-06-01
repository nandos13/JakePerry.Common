using System;
using System.Collections.Generic;

namespace JakePerry.Collections
{
    /// <summary>
    /// Pairs an implementation of <see cref="IComparer{T}"/> &amp; <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of objects to compare.
    /// </typeparam>
    public readonly struct Comparers<T> : IComparer<T>, IEqualityComparer<T>
    {
        private readonly IComparer<T> m_cmp;
        private readonly IEqualityComparer<T> m_eqcmp;

        /// <summary>
        /// The <see cref="IComparer{T}"/> implementation wrapped by the current instance.
        /// </summary>
        public IComparer<T> OrderComparer => m_cmp;

        /// <summary>
        /// The <see cref="IEqualityComparer{T}"/> implementation wrapped by the current instance.
        /// </summary>
        public IEqualityComparer<T> EqualityComparer => m_eqcmp;

        /// <summary>
        /// Indicates whether the current instance is set to the uninitialized default value.
        /// </summary>
        public bool IsNull => m_cmp is null;

        /// <summary>
        /// Get a comparer object backed by the <see cref="Comparer{T}.Default"/> &amp;
        /// <see cref="EqualityComparer{T}.Default"/> comparers.
        /// </summary>
        public static Comparers<T> Default => GetDefault(throwIfNotComparable: false);

        public Comparers(IComparer<T> order, IEqualityComparer<T> equality)
        {
            m_cmp = order ?? throw new ArgumentNullException(nameof(order));
            m_eqcmp = equality ?? throw new ArgumentNullException(nameof(equality));
        }

        public int Compare(T x, T y)
        {
            return m_cmp.Compare(x, y);
        }

        public bool Equals(T x, T y)
        {
            return m_eqcmp.Equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return m_eqcmp.GetHashCode(obj);
        }

        /// <summary>
        /// Wrap a comparer object using a single object that implements both the required interfaces.
        /// </summary>
        /// <typeparam name="TComparer">
        /// A type which implements both the required
        /// <see cref="IComparer{T}"/> &amp; <see cref="IEqualityComparer{T}"/> interfaces.
        /// </typeparam>
        /// <param name="comparer">
        /// The comparer object to wrap.
        /// </param>
        public static Comparers<T> Create<TComparer>(TComparer comparer)
            where TComparer : IComparer<T>, IEqualityComparer<T>
        {
            return new Comparers<T>(comparer, comparer);
        }

        public static Comparers<T> GetDefault(bool throwIfNotComparable = false)
        {
            if (throwIfNotComparable &&
                typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
            {
                throw new ArgumentException("Generic argument T must implement IComparable<T> to use the Default comparer.", nameof(T));
            }

            return new Comparers<T>(
                order: Comparer<T>.Default,
                equality: EqualityComparer<T>.Default);
        }
    }
}
