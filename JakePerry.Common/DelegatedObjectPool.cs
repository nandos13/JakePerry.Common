using System;

namespace JakePerry
{
    /// <summary>
    /// Extends <see cref="ObjectPool{T}"/> to provide easy-to-use
    /// object pooling with custom activation &amp; teardown logic.
    /// </summary>
    public sealed class DelegatedObjectPool<T> : ObjectPool<T> where T : class
    {
        private readonly Func<T> m_activator;
        private readonly Action<T> m_teardownCallback;

        public DelegatedObjectPool(Func<T> activator, Action<T> teardownCallback)
        {
            m_activator = activator ?? throw new ArgumentNullException(nameof(activator));
            m_teardownCallback = teardownCallback;
        }

        public DelegatedObjectPool(Func<T> activator) : this(activator, null) { }

        protected sealed override T Activate()
        {
            return m_activator.Invoke();
        }

        protected sealed override void BeforeReturnToPool(T obj)
        {
            m_teardownCallback?.Invoke(obj);
        }
    }
}
