using System;
using System.Threading;

namespace JakePerry.Threading
{
    /// <summary>
    /// An incredibly lightweight locking mechanism, specifically designed
    /// for situations where lock acquisition time is very minimal.
    /// <para/>
    /// It is important to adhere to these guidelines for correct usage of this type:
    /// <para/>
    /// * DO NOT copy the struct (assign to a local, obtain from a property, pass to a method, etc);
    /// You should always directly access the declaration field.
    /// <para/>
    /// * DO NOT run expensive code within the lock, as waiting threads will not yield.
    /// <para/>
    /// * DO always use with the try/finally pattern described in the documented example.
    /// </summary>
    /// <example>
    /// <code>
    /// bool acquiredLock = false;
    /// try
    /// {
    ///     m_lock.AcquireLock(ref acquiredLock);
    ///     // Lock acquired. Do some work (but keep it minimal).
    /// }
    /// finally { m_lock.ReleaseLock(acquiredLock); }
    /// </code>
    /// </example>
    internal struct LightweightSpinLock
    {
        private readonly bool m_created;

        // Token used for locking. 0 = unlocked, 1 = locked.
        private int m_lockToken;

        // Id of the thread that currently possesses the lock.
        private int m_ownerThreadId;

        /// <summary>
        /// Constructor used by the <see cref="Create"/> method
        /// to set correct default values.
        /// </summary>
        /// <param name="dummy">Unused.</param>
        private LightweightSpinLock(bool dummy)
        {
            m_lockToken = 0;
            m_ownerThreadId = -1;

            m_created = true;
        }

        internal void AcquireLock(ref bool lockTaken)
        {
            if (lockTaken)
            {
                throw new ArgumentException("Value must be false", nameof(lockTaken));
            }

            if (!m_created)
            {
                throw new InvalidOperationException("LightweightSpinLock must be created via the Create method");
            }

            int threadId = Thread.CurrentThread.ManagedThreadId;

            // Check if this thread has already acquired the lock.
            // This prevents a dead-lock due to design issues where a method obtains
            // the lock, then within the lock calls another method which does the same.
            // Note:
            // m_ownerThreadId is only assigned while the lock is obtained.
            // As such, checking it here - while technically not threadsafe - is safe enough.
            // If the lock is taken by this thread, the condition below will succeed;
            // Otherwise, it doesn't really matter what value the m_ownerThreadId read returns,
            // because we know it won't be equal to threadId and as such the condition will fail.
            if (threadId == m_ownerThreadId) return;

            // Very simple spinlock implementation for basic thread safety.
            // This lock will almost never be contested & when obtained it will
            // be released near immediately.
            // This implementation avoids overhead of the lock(object) statement.
            while (Interlocked.CompareExchange(ref m_lockToken, 1, 0) != 0) { }

            // We must immediately record that this thread has the lock.
            // Technically if the current thread died unexpectedly after the while loop
            // and just before this call, we'd deadlock. But really, I just don't care.
            lockTaken = true;
            m_ownerThreadId = threadId;
        }

        internal void ReleaseLock(bool lockWasTaken)
        {
            if (!lockWasTaken) return;

            int threadId = Thread.CurrentThread.ManagedThreadId;

            // We must verify the current thread actually has ownership of the lock.
            if (m_ownerThreadId != threadId)
            {
                throw new InvalidOperationException("Thread did not acquire the lock!");
            }

            m_ownerThreadId = -1;
            m_lockToken = 0;
        }

        internal static LightweightSpinLock Create()
        {
            return new LightweightSpinLock(true);
        }
    }
}
