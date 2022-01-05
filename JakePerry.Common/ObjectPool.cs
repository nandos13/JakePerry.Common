using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// Provides a means to pool objects that are no longer in use to be reused at a later time.
    /// </summary>
    public class ObjectPool<T> where T : class, new()
    {
        public const int kDefaultCapacity = 8;

        private readonly Stack<T> m_stack = new Stack<T>(capacity: kDefaultCapacity);

        private readonly Action<T> m_onGet;
        private readonly Action<T> m_onRelease;

        private int m_capacity = kDefaultCapacity;

        /// <summary>
        /// The maximum number of objects that can be stored in the pool at one time (minimum 1).
        /// </summary>
        public int Capacity
        {
            get => m_capacity;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (AssignValueUtility.SetValueType(ref m_capacity, value) &&
                    m_stack.Count > value)
                {
                    // Pop elements from the stack until its count is no longer greater than the desired capacity.
                    while (m_stack.Count > value)
                        m_stack.Pop();

                    // Trim excess to minimize memory usage when downsizing from a large capacity to a smaller value.
                    m_stack.TrimExcess();
                }
            }
        }

        /// <summary>
        /// The number of objects currently in the pool.
        /// </summary>
        public int Count => this.m_stack.Count;

        public ObjectPool(Action<T> onGet, Action<T> onRelease)
        {
            m_onGet = onGet;
            m_onRelease = onRelease;
        }

        /// <summary>
        /// Get an object from the pool, or create a new one if the pool is empty.
        /// </summary>
        /// <returns>An object of type <typeparamref name="T"/>.</returns>
        public T Get()
        {
            // Get element from the stack, or a new object if empty.
            var obj = m_stack.Count > 0
                ? m_stack.Pop()
                : new T();

            // Invoke any get logic.
            m_onGet?.Invoke(obj);

            return obj;
        }

        /// <summary>
        /// Release an object into the pool to be reused by a future invocation of the <see cref="Get"/> method.
        /// The object will not be added if the pool is already at capacity.
        /// </summary>
        /// <param name="obj">
        /// A reference to the object to release. This will be set to <see langword="null"/>
        /// to ensure the field is not reused after it has been released.
        /// </param>
        public void Release(ref T obj)
        {
            // Don't pool a null object.
            if (obj is null)
                return;

            try
            {
                // Invoke any release logic.
                m_onRelease?.Invoke(obj);

                // Don't pool the object if pool is already at capacity.
                if (m_stack.Count >= m_capacity)
                    return;

                // We can only assume the object is in a safe state to pool if the onRelease invocation
                // above completes without throwing an exception.
                m_stack.Push(obj);
            }
            finally { obj = null; }
        }
    }
}
