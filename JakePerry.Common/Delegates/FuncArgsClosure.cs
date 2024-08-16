using System;

namespace JakePerry
{
    /// <summary>
    /// A closure which can be used to map a <see cref="Func{T, TResult}"/> to
    /// a <see cref="Func{TResult}"/>, capturing the invoke arguments.
    /// </summary>
    public sealed class FuncArgsClosure<T, TResult>
    {
        private readonly Func<T, TResult> m_func;
        private readonly T m_arg0;

        public FuncArgsClosure(Func<T, TResult> func, T arg0)
        {
            this.m_func = func ?? throw new ArgumentNullException(nameof(func));
            this.m_arg0 = arg0;
        }

        public TResult Func()
        {
            return m_func.Invoke(m_arg0);
        }

        public static implicit operator Func<TResult>(FuncArgsClosure<T, TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }

    /// <summary>
    /// A closure which can be used to map a <see cref="Func{T1, T2, TResult}"/> to
    /// a <see cref="Func{TResult}"/>, capturing the invoke arguments.
    /// </summary>
    public sealed class FuncArgsClosure<T1, T2, TResult>
    {
        private readonly Func<T1, T2, TResult> m_func;
        private readonly T1 m_arg0;
        private readonly T2 m_arg1;

        public FuncArgsClosure(Func<T1, T2, TResult> func, T1 arg0, T2 arg1)
        {
            this.m_func = func ?? throw new ArgumentNullException(nameof(func));
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
        }

        public TResult Func()
        {
            return m_func.Invoke(m_arg0, m_arg1);
        }

        public static implicit operator Func<TResult>(FuncArgsClosure<T1, T2, TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }

    /// <summary>
    /// A closure which can be used to map a <see cref="Func{T1, T2, T3, TResult}"/> to
    /// a <see cref="Func{TResult}"/>, capturing the invoke arguments.
    /// </summary>
    public sealed class FuncArgsClosure<T1, T2, T3, TResult>
    {
        private readonly Func<T1, T2, T3, TResult> m_func;
        private readonly T1 m_arg0;
        private readonly T2 m_arg1;
        private readonly T3 m_arg2;

        public FuncArgsClosure(Func<T1, T2, T3, TResult> func, T1 arg0, T2 arg1, T3 arg2)
        {
            this.m_func = func ?? throw new ArgumentNullException(nameof(func));
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = arg2;
        }

        public TResult Func()
        {
            return m_func.Invoke(m_arg0, m_arg1, m_arg2);
        }

        public static implicit operator Func<TResult>(FuncArgsClosure<T1, T2, T3, TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }

    /// <summary>
    /// A closure which can be used to map a <see cref="Func{T1, T2, T3, T4, TResult}"/> to
    /// a <see cref="Func{TResult}"/>, capturing the invoke arguments.
    /// </summary>
    public sealed class FuncArgsClosure<T1, T2, T3, T4, TResult>
    {
        private readonly Func<T1, T2, T3, T4, TResult> m_func;
        private readonly T1 m_arg0;
        private readonly T2 m_arg1;
        private readonly T3 m_arg2;
        private readonly T4 m_arg3;

        public FuncArgsClosure(
            Func<T1, T2, T3, T4, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3)
        {
            this.m_func = func ?? throw new ArgumentNullException(nameof(func));
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = arg2;
            this.m_arg3 = arg3;
        }

        public TResult Func()
        {
            return m_func.Invoke(
                m_arg0,
                m_arg1,
                m_arg2,
                m_arg3);
        }

        public static implicit operator Func<TResult>(FuncArgsClosure<T1, T2, T3, T4, TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }

    /// <summary>
    /// A closure which can be used to map a <see cref="Func{T1, T2, T3, T4, T5, TResult}"/> to
    /// a <see cref="Func{TResult}"/>, capturing the invoke arguments.
    /// </summary>
    public sealed class FuncArgsClosure<T1, T2, T3, T4, T5, TResult>
    {
        private readonly Func<T1, T2, T3, T4, T5, TResult> m_func;
        private readonly T1 m_arg0;
        private readonly T2 m_arg1;
        private readonly T3 m_arg2;
        private readonly T4 m_arg3;
        private readonly T5 m_arg4;

        public FuncArgsClosure(
            Func<T1, T2, T3, T4, T5, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3,
            T5 arg4)
        {
            this.m_func = func ?? throw new ArgumentNullException(nameof(func));
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = arg2;
            this.m_arg3 = arg3;
            this.m_arg4 = arg4;
        }

        public TResult Func()
        {
            return m_func.Invoke(
                m_arg0,
                m_arg1,
                m_arg2,
                m_arg3,
                m_arg4);
        }

        public static implicit operator Func<TResult>(FuncArgsClosure<T1, T2, T3, T4, T5, TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }

    /// <summary>
    /// A closure which can be used to map a <see cref="Func{T1, T2, T3, T4, T5, T6, TResult}"/> to
    /// a <see cref="Func{TResult}"/>, capturing the invoke arguments.
    /// </summary>
    public sealed class FuncArgsClosure<T1, T2, T3, T4, T5, T6, TResult>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, TResult> m_func;
        private readonly T1 m_arg0;
        private readonly T2 m_arg1;
        private readonly T3 m_arg2;
        private readonly T4 m_arg3;
        private readonly T5 m_arg4;
        private readonly T6 m_arg5;

        public FuncArgsClosure(
            Func<T1, T2, T3, T4, T5, T6, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3,
            T5 arg4,
            T6 arg5)
        {
            this.m_func = func ?? throw new ArgumentNullException(nameof(func));
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = arg2;
            this.m_arg3 = arg3;
            this.m_arg4 = arg4;
            this.m_arg5 = arg5;
        }

        public TResult Func()
        {
            return m_func.Invoke(
                m_arg0,
                m_arg1,
                m_arg2,
                m_arg3,
                m_arg4,
                m_arg5);
        }

        public static implicit operator Func<TResult>(FuncArgsClosure<T1, T2, T3, T4, T5, T6, TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }

    /// <summary>
    /// A closure which can be used to map a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, TResult}"/> to
    /// a <see cref="Func{TResult}"/>, capturing the invoke arguments.
    /// </summary>
    public sealed class FuncArgsClosure<T1, T2, T3, T4, T5, T6, T7, TResult>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, T7, TResult> m_func;
        private readonly T1 m_arg0;
        private readonly T2 m_arg1;
        private readonly T3 m_arg2;
        private readonly T4 m_arg3;
        private readonly T5 m_arg4;
        private readonly T6 m_arg5;
        private readonly T7 m_arg6;

        public FuncArgsClosure(
            Func<T1, T2, T3, T4, T5, T6, T7, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3,
            T5 arg4,
            T6 arg5,
            T7 arg6)
        {
            this.m_func = func ?? throw new ArgumentNullException(nameof(func));
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = arg2;
            this.m_arg3 = arg3;
            this.m_arg4 = arg4;
            this.m_arg5 = arg5;
            this.m_arg6 = arg6;
        }

        public TResult Func()
        {
            return m_func.Invoke(
                m_arg0,
                m_arg1,
                m_arg2,
                m_arg3,
                m_arg4,
                m_arg5,
                m_arg6);
        }

        public static implicit operator Func<TResult>(FuncArgsClosure<T1, T2, T3, T4, T5, T6, T7, TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }

    /// <summary>
    /// A closure which can be used to map a <see cref="Func{T1, T2, T3, T4, T5, T6, T7, T8, TResult}"/> to
    /// a <see cref="Func{TResult}"/>, capturing the invoke arguments.
    /// </summary>
    public sealed class FuncArgsClosure<T1, T2, T3, T4, T5, T6, T7, T8, TResult>
    {
        private readonly Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> m_func;
        private readonly T1 m_arg0;
        private readonly T2 m_arg1;
        private readonly T3 m_arg2;
        private readonly T4 m_arg3;
        private readonly T5 m_arg4;
        private readonly T6 m_arg5;
        private readonly T7 m_arg6;
        private readonly T8 m_arg7;

        public FuncArgsClosure(
            Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3,
            T5 arg4,
            T6 arg5,
            T7 arg6,
            T8 arg7)
        {
            this.m_func = func ?? throw new ArgumentNullException(nameof(func));
            this.m_arg0 = arg0;
            this.m_arg1 = arg1;
            this.m_arg2 = arg2;
            this.m_arg3 = arg3;
            this.m_arg4 = arg4;
            this.m_arg5 = arg5;
            this.m_arg6 = arg6;
            this.m_arg7 = arg7;
        }

        public TResult Func()
        {
            return m_func.Invoke(
                m_arg0,
                m_arg1,
                m_arg2,
                m_arg3,
                m_arg4,
                m_arg5,
                m_arg6,
                m_arg7);
        }

        public static implicit operator Func<TResult>(FuncArgsClosure<T1, T2, T3, T4, T5, T6, T7, T8, TResult> closure)
        {
            return new Func<TResult>(closure.Func);
        }
    }
}
