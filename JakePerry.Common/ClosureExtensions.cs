using System;

namespace JakePerry
{
    /// <summary>
    /// A collection of utility extension methods that relate to delegate closures.
    /// </summary>
    public static class ClosureExtensions
    {
        /// <summary>
        /// See: <see cref="ActionToFuncClosure{TResult}"/>.
        /// </summary>
        public static Func<TResult> AsFunc<TResult>(this Action action, TResult result = default)
        {
            Enforce.Argument(action, nameof(action)).IsNotNull();

            return new ActionToFuncClosure<TResult>(action, result);
        }

        /// <summary>
        /// See: <see cref="FuncArgsClosure{T, TResult}"/>.
        /// </summary>
        public static Func<TResult> CaptureArgs<T, TResult>(this Func<T, TResult> func, T arg0)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            return new FuncArgsClosure<T, TResult>(func, arg0);
        }

        /// <summary>
        /// See: <see cref="FuncArgsClosure{T1, T2, TResult}"/>.
        /// </summary>
        public static Func<TResult> CaptureArgs<T1, T2, TResult>(this Func<T1, T2, TResult> func, T1 arg0, T2 arg1)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            return new FuncArgsClosure<T1, T2, TResult>(func, arg0, arg1);
        }

        /// <summary>
        /// See: <see cref="FuncArgsClosure{T1, T2, T3, TResult}"/>.
        /// </summary>
        public static Func<TResult> CaptureArgs<T1, T2, T3, TResult>(this Func<T1, T2, T3, TResult> func, T1 arg0, T2 arg1, T3 arg2)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            return new FuncArgsClosure<T1, T2, T3, TResult>(func, arg0, arg1, arg2);
        }

        /// <summary>
        /// See: <see cref="FuncArgsClosure{T1, T2, T3, T4, TResult}"/>.
        /// </summary>
        public static Func<TResult> CaptureArgs<T1, T2, T3, T4, TResult>(
            this Func<T1, T2, T3, T4, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            return new FuncArgsClosure<T1, T2, T3, T4, TResult>(func, arg0, arg1, arg2, arg3);
        }

        /// <summary>
        /// See: <see cref="FuncArgsClosure{T1, T2, T3, T4, T5, TResult}"/>.
        /// </summary>
        public static Func<TResult> CaptureArgs<T1, T2, T3, T4, T5, TResult>(
            this Func<T1, T2, T3, T4, T5, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3,
            T5 arg4)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            return new FuncArgsClosure<T1, T2, T3, T4, T5, TResult>(func, arg0, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// See: <see cref="FuncArgsClosure{T1, T2, T3, T4, T5, T6, TResult}"/>.
        /// </summary>
        public static Func<TResult> CaptureArgs<T1, T2, T3, T4, T5, T6, TResult>(
            this Func<T1, T2, T3, T4, T5, T6, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3,
            T5 arg4,
            T6 arg5)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            return new FuncArgsClosure<T1, T2, T3, T4, T5, T6, TResult>(func, arg0, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// See: <see cref="FuncArgsClosure{T1, T2, T3, T4, T5, T6, T7, TResult}"/>.
        /// </summary>
        public static Func<TResult> CaptureArgs<T1, T2, T3, T4, T5, T6, T7, TResult>(
            this Func<T1, T2, T3, T4, T5, T6, T7, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3,
            T5 arg4,
            T6 arg5,
            T7 arg6)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            return new FuncArgsClosure<T1, T2, T3, T4, T5, T6, T7, TResult>(func, arg0, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// See: <see cref="FuncArgsClosure{T1, T2, T3, T4, T5, T6, T7, T8, TResult}"/>.
        /// </summary>
        public static Func<TResult> CaptureArgs<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
            this Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> func,
            T1 arg0,
            T2 arg1,
            T3 arg2,
            T4 arg3,
            T5 arg4,
            T6 arg5,
            T7 arg6,
            T8 arg7)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            return new FuncArgsClosure<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(func, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
    }
}
