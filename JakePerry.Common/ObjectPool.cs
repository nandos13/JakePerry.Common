using JakePerry.Threading;
using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// Defines a threadsafe pool of reusable objects.
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        private readonly List<T> m_pool = new();

        private readonly Func<T> m_activator;
        private readonly Action<T> m_teardownCallback;

        private SpinLockSlim m_lock = SpinLockSlim.Create();

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
        /// 
        /// </summary>
        /// <param name="activator">
        /// A delegate function which is used to activate a new object instance
        /// when the pool is empty.
        /// </param>
        /// <param name="teardownCallback">
        /// [Optional] A delegate which handles any necessary teardown logic
        /// before an object instance is returned to the pool.
        /// </param>
        /// <param name="dontThrowOnActivatorNull">
        /// Indicates whether an exception should not be thrown if <paramref name="activator"/>
        /// is <see langword="null"/>.
        /// <para/>
        /// A <see langword="null"/> <paramref name="activator"/> is valid when a derived class overrides
        /// the <see cref="Activate"/> method and does not invoke the base method implementation,
        /// in which case this argument should be <see langword="true"/>.
        /// In all other cases, a <see langword="null"/> <paramref name="activator"/> is invalid and this
        /// argument should remain <see langword="false"/>.
        /// </param>
        protected ObjectPool(Func<T> activator, Action<T> teardownCallback, bool dontThrowOnActivatorNull = false)
        {
            if (!dontThrowOnActivatorNull && activator is null)
            {
                throw new ArgumentNullException(nameof(activator));
            }

            m_activator = activator;
            m_teardownCallback = teardownCallback;
        }

        /// <inheritdoc cref="ObjectPool{T}(Func{T}, Action{T}, bool)"/>
        public ObjectPool(Func<T> activator, Action<T> teardownCallback)
            : this(activator, teardownCallback, false)
        { }

        /// <inheritdoc cref="ObjectPool{T}(Func{T}, Action{T}, bool)"/>
        public ObjectPool(Func<T> activator)
            : this(activator, null, false)
        { }

        /// <summary>
        /// Constructs a new instance of an object pool.
        /// <para/>
        /// This constructor uses the default constructor to activate new object
        /// instances when the pool is empty and as such cannot be used if
        /// <typeparamref name="T"/> does not have a publicly available default constructor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// <typeparamref name="T"/> does not provide a publicly accessible default constructor.
        /// </exception>
        /// <inheritdoc cref="ObjectPool{T}(Func{T}, Action{T}, bool)"/>
        public ObjectPool(Action<T> teardownCallback)
        {
            var ctor = ReflectionEx.GetConstructor(typeof(T), throwOnError: false);

            // The default constructor ObjectPool() may be implicitly invoked by
            // derived class constructors. In such case, a null constructor may be valid,
            // as the derived class likely overrides the Activate method; if it doesn't,
            // an exception is thrown at runtime during activation.
            // 
            // In all other cases a missing default constructor is an exception, and user
            // code must provide an activation func.
            if (ctor is null && this.GetType() != typeof(ObjectPool<T>))
            {
                // TODO: Static analysis would be handy here to detect invalid uses of this construcor.
                throw new InvalidOperationException(
                    $"No publicly accessible default constructor exists for type {typeof(T)}" +
                    $". An activator function must be supplied");
            }

            Func<T> activator = () =>
            {
                return (T)ctor.Invoke(Array.Empty<object>());
            };

            m_activator = activator;
            m_teardownCallback = teardownCallback;
        }

        /// <inheritdoc cref="ObjectPool{T}(Action{T})"/>
        public ObjectPool() : this(teardownCallback: null) { }

        /// <summary>
        /// Activates a new instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <returns>
        /// The object instance that was activated.
        /// </returns>
        protected virtual T Activate()
        {
            var func = m_activator;
            if (func is null)
            {
                throw new NotImplementedException(
                    "ObjectPool's activator function is null. This can occur if a derived class does not provide " +
                    "an activator function to the base class during construction and erroneously invokes the " +
                    "base class Activate method implementation.");
            }

            return func.Invoke();
        }

        /// <summary>
        /// Invoked before a rented object is returned into the pool for reuse.
        /// </summary>
        /// <param name="obj">
        /// The rented object that is returning to the pool.
        /// </param>
        protected virtual void BeforeReturnToPool(T obj)
        {
            m_teardownCallback?.Invoke(obj);
        }

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
                var pool = m_pool;
                if (pool.Capacity == 0 || pool.Count < pool.Capacity)
                {
                    BeforeReturnToPool(obj);
                    pool.Add(obj);
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
