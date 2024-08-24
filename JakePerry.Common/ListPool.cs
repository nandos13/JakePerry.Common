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
        /// Container class for the static instance of <see cref="ListPool{T}"/>
        /// with the given generic type argument.
        /// </summary>
        private static class GenericListPool<T>
        {
            internal static readonly ListPool<T> _pool = new();
        }

        /// <summary>
        /// Get the internal <see cref="ListPool{T}"/> object used by this class.
        /// </summary>
        public static ListPool<T> GetInternalPool<T>()
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

        /// <inheritdoc cref="ListPool{T}.RentWithCapacity(int)"/>
        public static List<T> Rent<T>(int capacity)
        {
            return GenericListPool<T>._pool.RentWithCapacity(capacity);
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

        /// <inheritdoc cref="ObjectPool{T}.RentInScope(out T)"/>
        /// <seealso cref="ListPool{T}.RentWithCapacity(int)"/>
        public static ObjectPool<List<T>>.RentalScope RentInScope<T>(int capacity, out List<T> list)
        {
            var pool = GenericListPool<T>._pool;
            list = pool.RentWithCapacity(capacity);
            return new ObjectPool<List<T>>.RentalScope(pool, list);
        }

        /// <param name="list"><inheritdoc cref="Return{T}(List{T})"/></param>
        /// <inheritdoc cref="ObjectPool{T}.ReturnOnExitScope(T)"/>
        public static ObjectPool<List<T>>.RentalScope ReturnOnExitScope<T>(List<T> list)
        {
            return GenericListPool<T>._pool.ReturnOnExitScope(list);
        }
    }
}
