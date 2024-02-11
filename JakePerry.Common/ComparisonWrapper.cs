using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// Utility object for supporting method overloads for both <see cref="Comparison{T}"/> &amp;
    /// <see cref="IComparer{T}"/> types without duplicating code.
    /// </summary>
    public sealed class ComparisonWrapper<T> : IComparer<T>
    {
        [ThreadStatic]
        private static ComparisonWrapper<T> _inst;

        private Comparison<T> m_comparison;

        public int Compare(T x, T y)
        {
            return m_comparison.Invoke(x, y);
        }

        /// <summary>
        /// Acquire a comparer object wrapping the given comparison delegate.
        /// </summary>
        /// <exception cref="ArgumentNullException"/>
        public static ComparisonWrapper<T> Acquire(Comparison<T> comparison)
        {
            _ = comparison ?? throw new ArgumentNullException(nameof(comparison));

            var wrapper = _inst ?? new();
            _inst = null;

            wrapper.m_comparison = comparison;

            return wrapper;
        }

        /// <summary>
        /// Return the wrapper object so it may be reused, preventing future allocations.
        /// </summary>
        public static void Return(ComparisonWrapper<T> obj)
        {
            if (obj is null) return;

            obj.m_comparison = null;
            _inst = obj;
        }
    }
}
