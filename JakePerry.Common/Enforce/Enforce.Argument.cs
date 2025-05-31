using System;
using System.Collections;
using System.Runtime.CompilerServices;
using static JakePerry.FStrings;

namespace JakePerry
{
    public static partial class Enforce
    {
        public struct ArgumentContainer<T>
        {
            public T value;
            public string parameterName;
        }

        /// <summary>
        /// Provides a container for the given method argument which exposes
        /// methods to enforce certain rules &amp; expectations.
        /// <para/>
        /// <b>Usage:</b>
        /// <code>
        /// void Foo(string text)
        /// {
        ///     Enforce.Argument(text, nameof(text)).IsNotNull();
        /// }
        /// </code>
        /// </summary>
        /// <param name="arg">
        /// The value of the parameter argument.
        /// </param>
        /// <param name="parameterName">
        /// The name of the parameter. Hint: use <see langword="nameof"/>.
        /// </param>
        /// <remarks>
        /// Enforce.Argument methods will throw an <see cref="ArgumentException"/> (or one of
        /// its derived classes) on failure rather than the <see cref="EnforceException"/>.
        /// </remarks>
        public static ArgumentContainer<T> Argument<T>(T arg, string parameterName)
            => new() { value = arg, parameterName = parameterName };
    }

    public static class EnforceArgumentMethods
    {
        /// <summary>
        /// Assert some condition related to the argument is <see langword="true"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Condition<TArg, TMsg>(this in Enforce.ArgumentContainer<TArg> c, bool condition, TMsg message)
            where TMsg : struct, IFstring
        {
            if (!condition)
            {
                throw new ArgumentException(message.ToString(), c.parameterName);
            }
        }

        /// <inheritdoc cref="Condition{TArg, TMsg}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Condition<T>(this in Enforce.ArgumentContainer<T> c, bool condition, string message = null)
        {
            Condition(in c, condition, FString(message));
        }

        /// <summary>
        /// Assert that the argument is within a given range of values.
        /// </summary>
        /// <param name="minInclusive">
        /// The lower bound, inclusive.
        /// </param>
        /// <param name="maxInclusive">
        /// The upper bound, inclusive.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsBetween<T>(this in Enforce.ArgumentContainer<T> c, T minInclusive, T maxInclusive)
            where T : IComparable<T>
        {
            int cmpMin = c.value.CompareTo(minInclusive);
            int cmpMax = c.value.CompareTo(maxInclusive);

            if (cmpMin < 0 || cmpMax > 0)
            {
                throw new ArgumentOutOfRangeException(c.parameterName, c.value, $"Must be in range [{minInclusive}..{maxInclusive}]");
            }
        }

        /// <summary>
        /// Assert that the argument is greater than a given lower bound value.
        /// </summary>
        /// <param name="bound">
        /// The lower bound.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsGreaterThan<T>(this in Enforce.ArgumentContainer<T> c, T bound)
            where T : IComparable<T>
        {
            int cmp = c.value.CompareTo(bound);
            if (cmp <= 0)
            {
                throw new ArgumentOutOfRangeException(c.parameterName, c.value, $"Must be greater than {bound}");
            }
        }

        /// <summary>
        /// Assert that the argument is greater than or equal to a specified inclusive lower bound value.
        /// </summary>
        /// <param name="bound">
        /// The lower bound, inclusive.
        /// </param>
        /// <inheritdoc cref="IsGreaterThan"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsGreaterThanOrEqual<T>(this in Enforce.ArgumentContainer<T> c, T bound)
            where T : IComparable<T>
        {
            int cmp = c.value.CompareTo(bound);
            if (cmp < 0)
            {
                throw new ArgumentOutOfRangeException(c.parameterName, c.value, $"Must be greater than or equal to {bound}");
            }
        }

        /// <summary>
        /// Assert that the argument is less than a given upper bound value.
        /// </summary>
        /// <param name="bound">
        /// The upper bound.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsLessThan<T>(this in Enforce.ArgumentContainer<T> c, T bound)
            where T : IComparable<T>
        {
            int cmp = c.value.CompareTo(bound);
            if (cmp >= 0)
            {
                throw new ArgumentOutOfRangeException(c.parameterName, c.value, $"Must be less than {bound}");
            }
        }

        /// <summary>
        /// Assert that the argument is less than or equal to a specified inclusive upper bound value.
        /// </summary>
        /// <param name="bound">
        /// The upper bound, inclusive.
        /// </param>
        /// <inheritdoc cref="IsLessThan"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsLessThanOrEqual<T>(this in Enforce.ArgumentContainer<T> c, T bound)
            where T : IComparable<T>
        {
            int cmp = c.value.CompareTo(bound);
            if (cmp > 0)
            {
                throw new ArgumentOutOfRangeException(c.parameterName, c.value, $"Must be less than or equal to {bound}");
            }
        }

        /// <summary>
        /// Assert that the argument is a defined enum value.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="EnforceValueMethods.IsDefinedEnum" path="/remarks"/>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsDefinedEnum<T>(this in Enforce.ArgumentContainer<T> c)
            where T : struct, Enum
        {
            if (!Enum.IsDefined(typeof(T), c.value))
            {
                throw new ArgumentException($"Must be a defined enum value. Value: {c.value}", c.parameterName);
            }
        }

        /// <summary>
        /// Assert that the argument is not a <see langword="null"/> reference.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotNull<T>(this in Enforce.ArgumentContainer<T> c)
        {
            /* This method intentionally does not use the 'where T : class' constraint so that it
             * still works with open generic types. In the following example, type param 'T' could be
             * a value type, but the method should still enforce the argument is not null.
             * 
             * puublic void Foo<T>(T sequence) where T : IEnumerable
             * {
             *     Enforce.Argument(sequence, nameof(sequence)).IsNotNull();
             * }
             * 
             * Luckily the JIT compiler is smart enough to optimize 'IsValueType' check away,
             * and simply not run any code when 'T' is a value type.
             */

            if (typeof(T).IsValueType) return;

            if (c.value is null)
            {
                throw new ArgumentNullException(c.parameterName);
            }
        }

        /// <summary>
        /// Assert that the <see cref="string"/> argument is not a <see langword="null"/> reference or empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotNullOrEmpty(this in Enforce.ArgumentContainer<string> c)
        {
            IsNotNull(c);

            if (c.value.Length == 0)
            {
                throw new ArgumentException("String is empty.", c.parameterName);
            }
        }

        /// <summary>
        /// Assert that the <see cref="Substring"/> argument is not empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotEmpty(this in Enforce.ArgumentContainer<Substring> c)
        {
            if (c.value.Length == 0)
            {
                throw new ArgumentException("String is empty.", c.parameterName);
            }
        }

        /// <summary>
        /// Assert that a collection argument is not a <see langword="null"/> reference or empty.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsNotNullOrEmpty<T>(this in Enforce.ArgumentContainer<T> c)
            where T : IEnumerable
        {
            IsNotNull(c);

            T collection = c.value;
            IEnumerator enumerator = collection.GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                {
                    throw new ArgumentException("Collection is empty.", c.parameterName);
                }
            }
            finally { DisposeUtility.DisposeEnumerator(enumerator); }
        }

        /// <summary>
        /// Assert that the argument is in a valid state.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsInValidState<T>(this in Enforce.ArgumentContainer<T> c)
            where T : struct, IMightBeValid
        {
            if (!c.value.IsValid)
            {
                throw new ArgumentException("Object is invalid.", c.parameterName);
            }
        }

        /// <summary>
        /// Assert that the argument is assignable to a given type.
        /// </summary>
        /// <param name="assignable">
        /// The type which the current argument must be assignable to.
        /// </param>
        /// <param name="throwOnAbstract">
        /// Whether an exception should be thrown if the argument is an abstract type.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsAssignableTo(this in Enforce.ArgumentContainer<Type> c, Type assignable, bool throwOnAbstract = false)
        {
            _ = assignable ?? throw new ArgumentNullException(nameof(assignable),
                "Cannot check assignability; Argument 'assignable' is null.");

            IsNotNull(c);

            Type type = c.value;

            if (!assignable.IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type {assignable} is not assignable from the argument type {type}", c.parameterName);
            }

            if (throwOnAbstract && type.IsAbstract)
            {
                throw new ArgumentException("Cannot be abstract.", c.parameterName);
            }
        }
    }
}
