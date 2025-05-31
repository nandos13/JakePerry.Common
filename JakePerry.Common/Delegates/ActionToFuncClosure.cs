using System;

namespace JakePerry
{
    /// <summary>
    /// A closure which can be used to map an <see cref="Action"/> to a <see cref="Func{TResult}"/>.
    /// When the resulting <see cref="Func{TResult}"/> is invoked, the wrapped <see cref="Action"/> will
    /// be invoked before returning a predetermined <typeparamref name="TResult"/> value.
    /// </summary>
    public sealed class ActionToFuncClosure<TResult>
    {
        private readonly Action m_action;
        private readonly TResult m_result;

        public ActionToFuncClosure(Action action, TResult result)
        {
            Enforce.Argument(action, nameof(action)).IsNotNull();

            m_action = action;
            m_result = result;
        }

        public ActionToFuncClosure(Action action) : this(action, default) { }

        public TResult Func()
        {
            // Invoke the wrapped action.
            m_action.Invoke();

            // Return the predetermined result.
            return m_result;
        }

        public static implicit operator Func<TResult>(ActionToFuncClosure<TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }
}
