using JakePerry.Threading;
using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry
{
    internal abstract class MutableDelegate
    {
        protected sealed class ImmutableCapture
        {
            public static readonly object _disposeMarker;

            private readonly int m_count;
            public object o;

            public int Count => m_count;

            public ImmutableCapture(int count) { m_count = count; }

            public Delegate GetAtIndex(int index)
            {
                if (index < 0 || index >= m_count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (o is MutableDelegate mutable)
                {
                    bool acquiredLock = false;
                    try
                    {
                        mutable.m_lock.AcquireLock(ref acquiredLock);

                        if (ReferenceEquals(o, mutable))
                        {
                            return mutable.m_delegates[index];
                        }
                    }
                    finally { mutable.m_lock.ReleaseLock(acquiredLock); }
                }

                if (o is List<Delegate> copy)
                {
                    return copy[index];
                }

                if (ReferenceEquals(o, _disposeMarker))
                {
                    throw new ObjectDisposedException(nameof(ImmutableCapture));
                }

                throw new InvalidOperationException("Unknown object state.");
            }
        }

        private readonly int m_initialCapacity;

        private SpinLockSlim m_lock = SpinLockSlim.Create();

        private List<ImmutableCapture> m_immutables;
        private List<Delegate> m_delegates;

        private void BeforeMutate()
        {
            var list = m_immutables;
            if (list is null) return;

            var copy = m_delegates?.ToArray() ?? Array.Empty<Delegate>();
            foreach (var c in list)
            {
                c.o = copy;
            }

            list.Clear();
        }

        protected ImmutableCapture Capture()
        {
            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                var c = new ImmutableCapture(m_delegates?.Count ?? 0) { o = this };
                (m_immutables ??= new(capacity: 4)).Add(c);

                return c;
            }
            finally { m_lock.ReleaseLock(acquiredLock); }
        }

        protected void DisposeImmutable(ImmutableCapture c)
        {
            if (c is null) return;

            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                var list = m_immutables;
                if (list is not null)
                {
                    int index = list.IndexOf(c);
                    if (index > -1)
                    {
                        c.o = ImmutableCapture._disposeMarker;
                        list.RemoveAt(index);
                        return;
                    }
                }
            }
            finally { m_lock.ReleaseLock(acquiredLock); }

            c.o = ImmutableCapture._disposeMarker;
        }

        protected void AddDelegate(Delegate @delegate)
        {
            if (@delegate is null) return;

            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                BeforeMutate();
                var list = (m_delegates ??= m_initialCapacity > 0 ? new(m_initialCapacity) : new());
                list.Add(@delegate);
            }
            finally { m_lock.ReleaseLock(acquiredLock); }
        }

        protected bool RemoveDelegate(Delegate @delegate)
        {
            if (@delegate is null) return false;

            bool acquiredLock = false;
            try
            {
                m_lock.AcquireLock(ref acquiredLock);

                var list = m_delegates;
                if (list is not null)
                {
                    int index = list.LastIndexOf(@delegate);
                    if (index > -1)
                    {
                        BeforeMutate();
                        list.RemoveAt(index);
                        return true;
                    }
                }
            }
            finally { m_lock.ReleaseLock(acquiredLock); }

            return false;
        }
    }

    /// <summary>
    /// A mutable implementation of a delegate, intended to improve performance where
    /// a delegate is expected to be mutated - especially with a large invocation
    /// list - many times throughout the application lifecycle.
    /// <para/>
    /// This class is completely threadsafe and behaves identically to a regular C#
    /// <see cref="Delegate"/> when invoked.
    /// </summary>
    internal sealed class MutableDelegate<T> : MutableDelegate where T : Delegate
    {
        /// <summary>
        /// An immutable representation of a mutable delegate.
        /// Once an <see cref="Immutable"/> handle is obtained, any mutations applied
        /// to the source delegate will cause the invocation list to be copied.
        /// <para/>
        /// The immutable handle should be disposed once it is no longer required to prevent
        /// needless copy allocations during subsequent mutations.
        /// </summary>
        public readonly struct Immutable : IDisposable, IEnumerable, IEnumerable<T>
        {
            private readonly MutableDelegate<T> m_delegate;
            private readonly ImmutableCapture m_capture;

            public Immutable(MutableDelegate<T> @delegate)
            {
                Enforce.Argument(@delegate, nameof(@delegate)).IsNotNull();

                m_delegate = @delegate;
                m_capture = @delegate.Capture();
            }

            public struct Enumerator : IEnumerator, IEnumerator<T>
            {
                private readonly ImmutableCapture m_c;
                private int m_index;
                private T m_current;

                public readonly T Current => m_current;

                readonly object IEnumerator.Current => this.Current;

                public Enumerator(Immutable o)
                {
                    m_c = o.m_capture;
                    m_index = 0;
                    m_current = default;
                }

                readonly void IDisposable.Dispose() { }

                public bool MoveNext()
                {
                    while ((uint)m_index < (uint)m_c.Count)
                    {
                        m_current = (T)m_c.GetAtIndex(m_index);
                        ++m_index;
                        return true;
                    }
                    m_index = m_c.Count;
                    m_current = default;
                    return false;
                }

                void IEnumerator.Reset()
                {
                    m_index = 0;
                    m_current = default;
                }
            }

            public void Dispose()
            {
                m_delegate.DisposeImmutable(m_capture);
            }

            public Enumerator GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)GetEnumerator();

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => (IEnumerator<T>)GetEnumerator();
        }

        public void Add(T @delegate)
        {
            AddDelegate(@delegate);
        }

        public bool Remove(T @delegate)
        {
            return RemoveDelegate(@delegate);
        }

        public Immutable AsImmutable()
        {
            return new Immutable(this);
        }
    }

    /* TODO: Would love to use source generators one day to generate the Invoke methods
     * matching the signature of the delegate type, but I'm unable to get source generators
     * to work at all with unity just yet.
     */
    internal static class MutableDelegateInvokeImpl
    {
        public static void Invoke(this MutableDelegate<Action>.Immutable @delegate)
        {
            foreach (var o in @delegate) o.Invoke();
        }

        public static void Invoke(this MutableDelegate<Action> @delegate)
        {
            using var immutable = @delegate.AsImmutable();
            Invoke(immutable);
        }

        public static void Invoke<T>(this MutableDelegate<Action<T>>.Immutable @delegate, T arg)
        {
            foreach (var o in @delegate) o.Invoke(arg);
        }

        public static void Invoke<T>(this MutableDelegate<Action<T>> @delegate, T arg)
        {
            using var immutable = @delegate.AsImmutable();
            Invoke(immutable, arg);
        }

        public static void Invoke<T1, T2>(this MutableDelegate<Action<T1, T2>>.Immutable @delegate, T1 arg1, T2 arg2)
        {
            foreach (var o in @delegate) o.Invoke(arg1, arg2);
        }

        public static void Invoke<T1, T2>(this MutableDelegate<Action<T1, T2>> @delegate, T1 arg1, T2 arg2)
        {
            using var immutable = @delegate.AsImmutable();
            Invoke(immutable, arg1, arg2);
        }

        public static void Invoke<T1, T2, T3>(this MutableDelegate<Action<T1, T2, T3>>.Immutable @delegate, T1 arg1, T2 arg2, T3 arg3)
        {
            foreach (var o in @delegate) o.Invoke(arg1, arg2, arg3);
        }

        public static void Invoke<T1, T2, T3>(this MutableDelegate<Action<T1, T2, T3>> @delegate, T1 arg1, T2 arg2, T3 arg3)
        {
            using var immutable = @delegate.AsImmutable();
            Invoke(immutable, arg1, arg2, arg3);
        }

        public static TResult Invoke<TResult>(this MutableDelegate<Func<TResult>>.Immutable @delegate)
        {
            TResult result = default;
            foreach (var o in @delegate) result = o.Invoke();
            return result;
        }

        public static TResult Invoke<TResult>(this MutableDelegate<Func<TResult>> @delegate)
        {
            using var immutable = @delegate.AsImmutable();
            return Invoke(immutable);
        }

        public static TResult Invoke<T, TResult>(this MutableDelegate<Func<T, TResult>>.Immutable @delegate, T arg)
        {
            TResult result = default;
            foreach (var o in @delegate) result = o.Invoke(arg);
            return result;
        }

        public static TResult Invoke<T, TResult>(this MutableDelegate<Func<T, TResult>> @delegate, T arg)
        {
            using var immutable = @delegate.AsImmutable();
            return Invoke(immutable, arg);
        }

        public static TResult Invoke<T1, T2, TResult>(this MutableDelegate<Func<T1, T2, TResult>>.Immutable @delegate, T1 arg1, T2 arg2)
        {
            TResult result = default;
            foreach (var o in @delegate) result = o.Invoke(arg1, arg2);
            return result;
        }

        public static TResult Invoke<T1, T2, TResult>(this MutableDelegate<Func<T1, T2, TResult>> @delegate, T1 arg1, T2 arg2)
        {
            using var immutable = @delegate.AsImmutable();
            return Invoke(immutable, arg1, arg2);
        }

        public static TResult Invoke<T1, T2, T3, TResult>(this MutableDelegate<Func<T1, T2, T3, TResult>>.Immutable @delegate, T1 arg1, T2 arg2, T3 arg3)
        {
            TResult result = default;
            foreach (var o in @delegate) result = o.Invoke(arg1, arg2, arg3);
            return result;
        }

        public static TResult Invoke<T1, T2, T3, TResult>(this MutableDelegate<Func<T1, T2, T3, TResult>> @delegate, T1 arg1, T2 arg2, T3 arg3)
        {
            using var immutable = @delegate.AsImmutable();
            return Invoke(immutable, arg1, arg2, arg3);
        }
    }
}
