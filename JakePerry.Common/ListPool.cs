using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// A pool of list objects that can be reused to prevent memory allocations.
    /// </summary>
    public static class ListPool<T>
    {
        public const int kDefaultMaxListCount = ObjectPool<List<T>>.kDefaultCapacity;

        private static readonly ObjectPool<List<T>> s_pool;

        /// <summary>
        /// The number of lists currently in the pool.
        /// </summary>
        public static int ListCount => s_pool.Count;

        /// <summary>
        /// The maximum number of lists that can be stored in the pool at one time (minimum 1).
        /// </summary>
        public static int MaxListCount
        {
            get => s_pool.Capacity;
            set => s_pool.Capacity = value;
        }

        private static void Clear(List<T> list)
        {
            list.Clear();
        }

        /// <summary>
        /// Get a list from the pool, or create a new one if the pool is empty.
        /// </summary>
        /// <returns>A list of type <typeparamref name="T"/>.</returns>
        public static List<T> Get()
        {
            return s_pool.Get();
        }

        /// <inheritdoc cref="Get"/>
        /// <param name="capacity">Shortcut to ensure the returned list has a minimum capacity.</param>
        public static List<T> Get(int capacity)
        {
            var list = s_pool.Get();

            // Ensure the list has the minimum required capacity.
            // Note: It would be ideal if we could check for an existing list with this capacity in the pool
            // first, but this isn't nicely implementable since ObjectPool uses a Stack collection.
            if (list.Capacity < capacity)
            {
                list.Capacity = capacity;
            }

            return list;
        }

        /// <summary>
        /// Release a list into the pool to be reused by a future invocation of the <see cref="Get"/> method.
        /// The list will not be added if the pool is already at capacity.
        /// </summary>
        /// <param name="list">
        /// A reference to the list to release. This will be set to <see langword="null"/>
        /// to ensure the field is not reused after it has been released.
        /// </param>
        public static void Release(ref List<T> list)
        {
            s_pool.Release(ref list);
        }

#pragma warning disable CA1810 // Initialize reference type static fields inline
        static ListPool()
#pragma warning restore CA1810
        {
            // Delegate method to clear a list when it's released to the pool.
            var onRelease = new Action<List<T>>(Clear);

            s_pool = new ObjectPool<List<T>>(null, onRelease);
        }
    }
}
