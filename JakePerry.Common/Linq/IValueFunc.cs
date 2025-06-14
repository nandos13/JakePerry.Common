using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace JakePerry.Linq
{
    /// <summary>
    /// <inheritdoc cref="Func{T, TResult}" path="/summary"/>
    /// </summary>
    /// <typeparam name="T">
    /// <inheritdoc cref="Func{T, TResult}" path="/typeparam[@name='T']"/>
    /// </typeparam>
    /// <typeparam name="TResult">
    /// <inheritdoc cref="Func{T, TResult}" path="/typeparam[@name='TResult']"/>
    /// </typeparam>
    public interface IValueFunc<in T, out TResult> : IStruct
    {
        /// <summary>
        /// Invoke the delegate function.
        /// </summary>
        /// <param name="arg">
        /// <inheritdoc cref="Func{T, TResult}" path="/param[@name='arg']"/>
        /// </param>
        /// <returns>
        /// <inheritdoc cref="Func{T, TResult}" path="/returns"/>
        /// </returns>
        TResult Invoke(T arg);
    }

    /// <summary>
    /// Implementation of <see cref="IValueFunc{T, TResult}"/>.
    /// <para/>
    /// Encapsulates a <see cref="Func{T, TResult}"/> instance.
    /// </summary>
    public readonly struct VFunc<T, TResult> : IValueFunc<T, TResult>
    {
        public readonly Func<T, TResult> Func { get; }

        public VFunc([DisallowNull] Func<T, TResult> func)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            Func = func;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TResult Invoke(T arg)
        {
            return Func is null ? default : Func.Invoke(arg);
        }
    }

    /// <summary>
    /// Implementation of <see cref="IValueFunc{T, TResult}"/>.
    /// <para/>
    /// Encapsulates a <see cref="Func{T, TState, TResult}"/> instance &amp; a <typeparamref name="TState"/>
    /// argument used as the second invocation parameter.
    /// </summary>
    public readonly struct ParameterizedVFunc<T, TState, TResult> : IValueFunc<T, TResult>
    {
        public readonly Func<T, TState, TResult> Func { get; }

        public readonly TState State { get; }

        public ParameterizedVFunc([DisallowNull] Func<T, TState, TResult> func, TState state)
        {
            Enforce.Argument(func, nameof(func)).IsNotNull();

            Func = func;
            State = state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly TResult Invoke(T arg)
        {
            return Func is null ? default : Func.Invoke(arg, State);
        }
    }

    public static partial class ValueDelegateExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VFunc<T, TResult> AsValueDelegate<T, TResult>(
            [DisallowNull] this Func<T, TResult> func)
        {
            return new(func);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ParameterizedVFunc<T, TState, TResult> AsValueDelegate<T, TState, TResult>(
            [DisallowNull] this Func<T, TState, TResult> func, TState state)
        {
            return new(func, state);
        }
    }
}
