using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// An API for pooling <see cref="List{T}"/> objects.
    /// Utilizing this class in areas where lists are frequently created
    /// can help to prevent memory allocations and improve performance.
    /// </summary>
    public static class ListPool
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
        public sealed class Implementation<T> : ObjectPool<List<T>>
        {
            internal static readonly Implementation<T> _pool = new();

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

        /// <summary>
        /// Get the internal <see cref="Implementation{T}"/> object used by this class.
        /// </summary>
        public static Implementation<T> GetInternalPool<T>()
        {
            return Implementation<T>._pool;
        }

        /// <summary>
        /// Rent a list from the pool.
        /// </summary>
        public static List<T> Rent<T>()
        {
            return Implementation<T>._pool.Rent();
        }

        /// <inheritdoc cref="Implementation{T}.RentWithCapacity(int)"/>
        public static List<T> Rent<T>(int capacity)
        {
            return Implementation<T>._pool.RentWithCapacity(capacity);
        }

        /// <summary>
        /// Return a list to the pool.
        /// </summary>
        /// <param name="list">
        /// The list to return.
        /// </param>
        public static void Return<T>(List<T> list)
        {
            Implementation<T>._pool.Return(list);
        }

        /// <param name="list">
        /// The rented list.
        /// </param>
        /// <inheritdoc cref="ObjectPool{T}.RentInScope(out T)"/>
        public static ObjectPool<List<T>>.RentalScope RentInScope<T>(out List<T> list)
        {
            return Implementation<T>._pool.RentInScope(out list);
        }

        /// <inheritdoc cref="ObjectPool{T}.RentInScope(out T)"/>
        /// <seealso cref="Implementation{T}.RentWithCapacity(int)"/>
        public static ObjectPool<List<T>>.RentalScope RentInScope<T>(int capacity, out List<T> list)
        {
            Implementation<T> pool = Implementation<T>._pool;
            list = pool.RentWithCapacity(capacity);
            return new ObjectPool<List<T>>.RentalScope(pool, list);
        }

        /// <param name="list"><inheritdoc cref="Return{T}(List{T})"/></param>
        /// <inheritdoc cref="ObjectPool{T}.ReturnOnExitScope(T)"/>
        public static ObjectPool<List<T>>.RentalScope ReturnOnExitScope<T>(List<T> list)
        {
            return Implementation<T>._pool.ReturnOnExitScope(list);
        }
    }
}
