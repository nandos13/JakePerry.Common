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
    public readonly struct Comparers<T> : IComparer<T>, IEqualityComparer<T>, IMightBeValid
    {
        private readonly IComparer<T> m_cmp;
        private readonly IEqualityComparer<T> m_eqcmp;

        /// <summary>
        /// The <see cref="IComparer{T}"/> implementation wrapped by the current instance.
        /// </summary>
        public readonly IComparer<T> OrderComparer => m_cmp;

        /// <summary>
        /// The <see cref="IEqualityComparer{T}"/> implementation wrapped by the current instance.
        /// </summary>
        public readonly IEqualityComparer<T> EqualityComparer => m_eqcmp;

        public readonly bool IsValid => m_cmp is not null;

        /// <summary>
        /// Get a comparer object backed by the <see cref="Comparer{T}.Default"/> &amp;
        /// <see cref="EqualityComparer{T}.Default"/> comparers.
        /// </summary>
        public static Comparers<T> Default => GetDefault(throwIfNotComparable: false);

        public Comparers(IComparer<T> order, IEqualityComparer<T> equality)
        {
            Enforce.Argument(order, nameof(order)).IsNotNull();
            Enforce.Argument(equality, nameof(equality)).IsNotNull();

            (m_cmp, m_eqcmp) = (order, equality);
        }

        public readonly int Compare(T x, T y)
        {
            return m_cmp.Compare(x, y);
        }

        public readonly bool Equals(T x, T y)
        {
            return m_eqcmp.Equals(x, y);
        }

        public readonly int GetHashCode(T obj)
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
            if (throwIfNotComparable)
            {
                Enforce.Argument(typeof(T), nameof(T)).IsAssignableTo(typeof(IComparable<T>));
            }

            return new Comparers<T>(
                order: Comparer<T>.Default,
                equality: EqualityComparer<T>.Default);
        }
    }
}
