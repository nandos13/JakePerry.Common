using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JakePerry.Threading
{
    /// <summary>
    /// A non-generic alternative to the <see cref="TaskCompletionSource{TResult}"/> class.
    /// This can be used when an asynchronous task needs to signal for completion, but no
    /// return value is required.
    /// </summary>
    public class TaskCompletionSource
    {
        private readonly TaskCompletionSource<object> m_source;

        /// <summary>
        /// The underlying <see cref="TaskCompletionSource{TResult}"/>.
        /// </summary>
        public TaskCompletionSource<object> Source => this.m_source;

        public TaskCompletionSource()
        {
            this.m_source = new TaskCompletionSource<object>();
        }

        public TaskCompletionSource(TaskCreationOptions creationOptions)
        {
            this.m_source = new TaskCompletionSource<object>(creationOptions);
        }

        public TaskCompletionSource(object state)
        {
            this.m_source = new TaskCompletionSource<object>(state);
        }

        public TaskCompletionSource(object state, TaskCreationOptions creationOptions)
        {
            this.m_source = new TaskCompletionSource<object>(state, creationOptions);
        }

        /// <see cref="TaskCompletionSource{TResult}.Task"/>
        public Task Task => this.m_source.Task;

        /// <see cref="TaskCompletionSource{TResult}.SetCanceled()"/>
        public void SetCanceled() => this.m_source.SetCanceled();

        /// <see cref="TaskCompletionSource{TResult}.SetException(IEnumerable{Exception})"/>
        public void SetException(IEnumerable<Exception> exceptions) => this.m_source.SetException(exceptions);

        /// <see cref="TaskCompletionSource{TResult}.SetException(Exception)"/>
        public void SetException(Exception exception) => this.m_source.SetException(exception);

        /// <see cref="TaskCompletionSource{TResult}.TrySetCanceled()"/>
        public bool TrySetCanceled() => this.m_source.TrySetCanceled();

        /// <see cref="TaskCompletionSource{TResult}.TrySetCanceled(CancellationToken)"/>
        public bool TrySetCanceled(CancellationToken cancellationToken) => this.m_source.TrySetCanceled(cancellationToken);

        /// <see cref="TaskCompletionSource{TResult}.TrySetException(IEnumerable{Exception})"/>
        public bool TrySetException(IEnumerable<Exception> exceptions) => this.m_source.TrySetException(exceptions);

        /// <see cref="TaskCompletionSource{TResult}.TrySetException(Exception)"/>
        public bool TrySetException(Exception exception) => this.m_source.TrySetException(exception);

        /// <summary>
        /// Signal the underlying <see cref="TaskCompletionSource{TResult}"/> to transition the
        /// task into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <see cref="TaskCompletionSource{TResult}.SetResult(TResult)"/>
        public void Signal() => this.m_source.SetResult(null);

        /// <summary>
        /// Attempts to signal the underlying <see cref="TaskCompletionSource{TResult}"/> to transition
        /// the task into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <see cref="TaskCompletionSource{TResult}.TrySetResult(TResult)"/>
        public bool TrySignal() => this.m_source.TrySetResult(null);
    }
}
