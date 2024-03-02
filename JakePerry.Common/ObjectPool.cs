using JakePerry.Threading;
using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// Defines a base class for a threadsafe pool of reusable objects.
    /// </summary>
    public abstract class ObjectPool<T> where T : class
    {
        private readonly List<T> m_pool = new();
        private readonly LightweightSpinLock m_lock = new();

        public struct RentalScope : IDisposable
        {
            private readonly ObjectPool<T> m_pool;
            private T m_obj;

            internal RentalScope(ObjectPool<T> pool, T obj)
            {
                m_pool = pool;
                m_obj = obj;
            }

            public void Dispose()
            {
                m_pool.Return(m_obj);
                m_obj = null;
            }
        }

        /// <summary>
        /// Gets or sets the total number of objects that can be held
        /// by the pool at one time. Minimum capacity is 1.
        /// </summary>
        public int Capacity
        {
            get
            {
                bool acquiredLock = false;
                try
                {
                    m_lock.AcquireLock(ref acquiredLock);

                    return m_pool.Capacity;
                }
                finally { m_lock.ReleaseLock(acquiredLock); }
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(value));

                bool acquiredLock = false;
                try
                {
                    m_lock.AcquireLock(ref acquiredLock);

                    // Remove excess elements when downsizing.
                    for (int i = m_pool.Count - 1; i >= value; --i)
                    {
                        m_pool.RemoveAt(i);
                    }

                    m_pool.Capacity = value;
                }
                finally { m_lock.ReleaseLock(acquiredLock); }
            }
        }

        /// <summary>
        /// Indicates the current number of objects in the pool.
        /// </summary>
        public int Count
        {
            get
            {
                bool acquiredLock = false;
                try
                {
                    m_lock.AcquireLock(ref acquiredLock);

                    return m_pool.Count;
                }
                finally { m_lock.ReleaseLock(acquiredLock); }
            }
        }

        /// <summary>
        /// When overridden in a derived class, activates a new instance
        /// of type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>
        /// The object instance that was activated.
        /// </returns>
        protected abstract T Activate();

        /// <summary>
        /// Invoked before a rented object is returned into the pool for reuse.
        /// </summary>
        /// <param name="obj">
        /// The rented object that is returning to the pool.
        /// </param>
        protected virtual void BeforeReturnToPool(T obj) { }

        /// <summary>
        /// Clear the pool.
        /// </summary>
        public void Clear()
        {
            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                m_pool.Clear();
            }
            finally { m_lock.ReleaseLock(acquiredLock); }
        }

        /// <summary>
        /// Check if the object is present in the pool, ignoring any custom equality logic
        /// implemented by type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// Note: Assumes the lock has already been taken.
        /// </remarks>
        private bool FastContains(T obj)
        {
            var asObject = (object)obj;
            foreach (var o in m_pool)
            {
                if (o == asObject) return true;
            }

            return false;
        }

        /// <summary>
        /// Rent an object from the pool.
        /// </summary>
        public T Rent()
        {
            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                int index = m_pool.Count - 1;

                // Rent from the pool, if it's not empty.
                if (index > -1)
                {
                    var obj = m_pool[index];
                    m_pool.RemoveAt(index);

                    return obj;
                }
            }
            finally { m_lock.ReleaseLock(acquiredLock); }

            // Activate a new instance
            return Activate();
        }

        /// <summary>
        /// Rent the best scoring object from the pool.
        /// </summary>
        /// <param name="comparer">
        /// A comparison delegate that compares the arbitrary score of two pooled objects
        /// and returns the appropriate comparison value.
        /// </param>
        /// <remarks>
        /// The following code example shows a comparison method that would
        /// rent the list with the largest capacity.
        /// <code>
        /// public int Compare(List&lt;int&gt; x, List&lt;int&gt; y)
        /// {
        ///     return x.Capacity.CompareTo(y.Capacity);
        /// }
        /// </code>
        /// </remarks>
        public T RentBest(IComparer<T> comparer)
        {
            _ = comparer ?? throw new ArgumentNullException(nameof(comparer));

            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                if (m_pool.Count > 1)
                {
                    int bestIndex = 0;
                    var best = m_pool[0];
                    for (int i = 1; i < m_pool.Count; ++i)
                    {
                        var other = m_pool[i];
                        if (comparer.Compare(best, other) < 0)
                        {
                            bestIndex = i;
                            best = other;
                        }
                    }

                    m_pool.RemoveAt(bestIndex);
                    return best;
                }
            }
            finally { m_lock.ReleaseLock(acquiredLock); }

            // Use default logic if there are less than two objects in the pool.
            return Rent();
        }

        /// <inheritdoc cref="RentBest(IComparer{T})"/>
        /// <param name="comparison">
        /// A comparison delegate that compares the arbitrary score of two pooled objects
        /// and returns the appropriate comparison value.
        /// </param>
        public T RentBest(Comparison<T> comparison)
        {
            var comparer = ComparisonWrapper<T>.Acquire(comparison);
            var rental = RentBest(comparer);

            ComparisonWrapper<T>.Return(comparer);

            return rental;
        }

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        /// <param name="obj">
        /// The object to return.
        /// </param>
        public void Return(T obj)
        {
            // Don't pool a null object.
            if (obj is null) return;

            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                if (FastContains(obj))
                {
                    throw new InvalidOperationException("Cannot return an object that is already present in the pool.");
                }

                // Don't exceed the maximum capacity.
                if (m_pool.Count < m_pool.Capacity)
                {
                    BeforeReturnToPool(obj);
                    m_pool.Add(obj);
                }
            }
            finally { m_lock.ReleaseLock(acquiredLock); }
        }

        /// <summary>
        /// Rent an object from the pool and obtain a scope object that will
        /// handle returning said object when it is disposed.
        /// </summary>
        /// <param name="obj">
        /// The rented object.
        /// </param>
        /// <returns>
        /// A disposable scope object that returns <paramref name="obj"/> to
        /// the pool when disposed.
        /// </returns>
        /// <remarks>
        /// The returned scope object should be used in a <c>using</c> statement.
        /// See example below:
        /// <code>
        /// using var scope = pool.RentInScope(out object obj);
        /// </code>
        /// </remarks>
        public RentalScope RentInScope(out T obj)
        {
            return new RentalScope(this, obj = Rent());
        }

        /// <summary>
        /// Helper method that returns an object to the pool when exiting scope.
        /// </summary>
        /// <param name="obj"><inheritdoc cref="Return(T)"/></param>
        /// <returns><inheritdoc cref="RentInScope(out T)"/></returns>
        /// <remarks>
        /// The returned scope object should be used in a <c>using</c> statement.
        /// See example below:
        /// <code>
        /// using var scope = pool.ReturnOnExitScope(obj);
        /// </code>
        /// </remarks>
        public RentalScope ReturnOnExitScope(T obj)
        {
            return new RentalScope(this, obj);
        }

        /// <summary>
        /// Rent an object matching <paramref name="predicate"/> from the pool
        /// if one is found.
        /// </summary>
        /// <param name="predicate">
        /// The search predicate.
        /// </param>
        /// <param name="match">
        /// The rented object, or <see langword="null"/> if no match was found.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a match is found; Otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public bool TryRent(Predicate<T> predicate, out T match)
        {
            _ = predicate ?? throw new ArgumentNullException(nameof(predicate));

            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                for (int i = 0; i < m_pool.Count; ++i)
                {
                    var obj = m_pool[i];
                    if (predicate.Invoke(obj))
                    {
                        m_pool.RemoveAt(i);

                        match = obj;
                        return true;
                    }
                }
            }
            finally { m_lock.ReleaseLock(acquiredLock); }

            match = null;
            return false;
        }
    }
}
