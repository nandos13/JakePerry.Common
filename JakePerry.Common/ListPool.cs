using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// A pool of list objects that can be reused to prevent memory allocations.
    /// </summary>
    public static class ListPool
    {
        /// <summary>
        /// Contains the static pool of lists of type <typeparamref name="T"/>
        /// as well as some other helpers.
        /// </summary>
        private static class GenericListPool<T>
        {
            /// <summary>
            /// Implementation of the list pool.
            /// Hadles clearing lists before they are returned to the pool.
            /// </summary>
            internal sealed class ListPoolImpl : ObjectPool<List<T>>
            {
                protected override List<T> Activate() => new();

                protected override void BeforeReturnToPool(List<T> obj)
                {
                    obj.Clear();
                }
            }

            /// <summary>
            /// Helper for finding a pooled list with a given available capacity.
            /// Optimized to prevent subsequent allocations.
            /// </summary>
            private sealed class CapacityHelper : IComparer<List<T>>
            {
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

            internal static readonly ListPoolImpl _pool = new();

            internal static List<T> RentWithCapacity(int capacity)
            {
                var helper = _capacityHelper ??= new();
                helper.Capacity = capacity;

                if (_pool.TryRent(helper.minCapacityPredicate, out List<T> match))
                {
                    return match;
                }

                var list = _pool.RentBest(comparer: helper);
                list.Capacity = capacity;

                return list;
            }
        }

        /// <summary>
        /// Get the internal <see cref="ObjectPool{T}"/> object.
        /// </summary>
        public static ObjectPool<List<T>> GetInternalPool<T>()
        {
            return GenericListPool<T>._pool;
        }

        /// <summary>
        /// Rent a list from the pool.
        /// </summary>
        public static List<T> Rent<T>()
        {
            return GenericListPool<T>._pool.Rent();
        }

        /// <summary>
        /// <inheritdoc cref="Rent()"/>
        /// Searches pooled lists to find one with adequate capacity.
        /// If no adequate lists are found, increases the capacity of the largest list.
        /// </summary>
        /// <param name="capacity">
        /// The minimum capacity of the rented list.
        /// </param>
        public static List<T> Rent<T>(int capacity)
        {
            return GenericListPool<T>.RentWithCapacity(capacity);
        }

        /// <summary>
        /// Return a list to the pool.
        /// </summary>
        /// <param name="list">
        /// The list to return.
        /// </param>
        public static void Return<T>(List<T> list)
        {
            GenericListPool<T>._pool.Return(list);
        }

        /// <param name="list">
        /// The rented list.
        /// </param>
        /// <inheritdoc cref="ObjectPool{T}.RentInScope(out T)"/>
        public static ObjectPool<List<T>>.RentalScope RentInScope<T>(out List<T> list)
        {
            return GenericListPool<T>._pool.RentInScope(out list);
        }

        /// <param name="list"><inheritdoc cref="Return{T}(List{T})"/></param>
        /// <inheritdoc cref="ObjectPool{T}.ReturnOnExitScope(T)"/>
        public static ObjectPool<List<T>>.RentalScope ReturnOnExitScope<T>(List<T> list)
        {
            return GenericListPool<T>._pool.ReturnOnExitScope(list);
        }
    }
}
