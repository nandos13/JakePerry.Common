using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// Defines a pool of <see cref="List{T}"/>.
    /// <para/>
    /// Lists returned to the pool are cleared automatically to prevent erroneously
    /// keeping elements in memory.
    /// </summary>
    /// <remarks>
    /// This implementation is used internally by the non-generic <see cref="ListPool"/> type,
    /// but you can still create &amp; manage your own instance for more specialized areas of code.
    /// </remarks>
    public sealed class ListPool<T> : ObjectPool<List<T>>
    {
        /// <summary>
        /// Helper for finding a pooled list with a given available capacity.
        /// Optimized to prevent subsequent allocations.
        /// </summary>
        private sealed class CapacityHelper : IComparer<List<T>>
        {
            // Single allocation delegate for the 'HasCapacity' method
            internal readonly Predicate<List<T>> minCapacityPredicate;

            internal int Capacity { get; set; }

            internal CapacityHelper() => minCapacityPredicate = HasCapacity;

            private bool HasCapacity(List<T> list) => list.Capacity >= Capacity;

            int IComparer<List<T>>.Compare(List<T> x, List<T> y)
            {
                return x.Capacity.CompareTo(y.Capacity);
            }
        }

        [ThreadStatic]
        private static CapacityHelper _capacityHelper;

        protected sealed override List<T> Activate() => new();

        protected sealed override void BeforeReturnToPool(List<T> obj)
        {
            obj.Clear();
        }

        /// <summary>
        /// <inheritdoc cref="ObjectPool{T}.Rent()"/>
        /// <para/>
        /// Searches pooled lists to find one with adequate capacity.
        /// If no adequate lists are found, increases the capacity of the largest list.
        /// </summary>
        /// <param name="capacity">
        /// The minimum capacity of the rented list.
        /// </param>
        public List<T> RentWithCapacity(int capacity)
        {
            CapacityHelper helper = _capacityHelper ??= new();
            helper.Capacity = capacity;

            if (TryRent(helper.minCapacityPredicate, out List<T> match))
            {
                return match;
            }

            List<T> list = RentBest(comparer: helper);
            list.Capacity = capacity;

            return list;
        }
    }
}
