using System;
using System.Runtime.CompilerServices;

namespace JakePerry
{
    public static partial class Enforce
    {
        public struct ValueContainer<T>
        {
            public T value;
        }

        /// <summary>
        /// Provides a container for the given value which exposes methods to
        /// enforce certain expectations.
        /// </summary>
        public static ValueContainer<T> Value<T>(T value)
            => new() { value = value };
    }

    public static class EnforceValueMethods
    {
        /// <summary>
        /// Assert that the value is within a given range of values.
        /// </summary>
        /// <param name="minInclusive">
        /// The lower bound, inclusive.
        /// </param>
        /// <param name="maxInclusive">
        /// The upper bound, inclusive.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsBetween<T>(this in Enforce.ValueContainer<T> c, T minInclusive, T maxInclusive)
            where T : IComparable<T>
        {
            int cmpMin = c.value.CompareTo(minInclusive);
            int cmpMax = c.value.CompareTo(maxInclusive);

            if (cmpMin < 0 || cmpMax > 0)
            {
                throw new EnforceException($"Must be in range [{minInclusive}..{maxInclusive}]. Value: {c.value}");
            }
        }

        /// <summary>
        /// Assert that the value is greater than a given lower bound value.
        /// </summary>
        /// <param name="bound">
        /// The lower bound.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsGreaterThan<T>(this in Enforce.ValueContainer<T> c, T bound)
            where T : IComparable<T>
        {
            int cmp = c.value.CompareTo(bound);
            if (cmp <= 0)
            {
                throw new EnforceException($"Must be greater than {bound}. Value: {c.value}");
            }
        }

        /// <summary>
        /// Assert that the value is greater than or equal to a specified inclusive lower bound value.
        /// </summary>
        /// <param name="bound">
        /// The lower bound, inclusive.
        /// </param>
        /// <inheritdoc cref="IsGreaterThan"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsGreaterThanOrEqual<T>(this in Enforce.ValueContainer<T> c, T bound)
            where T : IComparable<T>
        {
            int cmp = c.value.CompareTo(bound);
            if (cmp < 0)
            {
                throw new EnforceException($"Must be greater than or equal to {bound}. Value: {c.value}");
            }
        }

        /// <summary>
        /// Assert that the value is less than a given upper bound value.
        /// </summary>
        /// <param name="bound">
        /// The upper bound.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsLessThan<T>(this in Enforce.ValueContainer<T> c, T bound)
            where T : IComparable<T>
        {
            int cmp = c.value.CompareTo(bound);
            if (cmp >= 0)
            {
                throw new EnforceException($"Must be less than {bound}. Value: {c.value}");
            }
        }

        /// <summary>
        /// Assert that the value is less than or equal to a specified inclusive upper bound value.
        /// </summary>
        /// <param name="bound">
        /// The upper bound, inclusive.
        /// </param>
        /// <inheritdoc cref="IsLessThan"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsLessThanOrEqual<T>(this in Enforce.ValueContainer<T> c, T bound)
            where T : IComparable<T>
        {
            int cmp = c.value.CompareTo(bound);
            if (cmp > 0)
            {
                throw new EnforceException($"Must be less than or equal to {bound}. Value: {c.value}");
            }
        }

        /// <summary>
        /// Assert that the value is a defined enum value.
        /// </summary>
        /// <remarks>
        /// This method generally should not be used with flags style enums,
        /// unless you specifically expect an argument to be a single flag,
        /// never a bitwise combination of flags.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsDefinedEnum<T>(this in Enforce.ValueContainer<T> c)
            where T : struct, Enum
        {
            // Note: 'IsDefined(Type, object)' boxes, but 'IsDefined<T>(T)' is not available.
            if (!Enum.IsDefined(typeof(T), c.value))
            {
                throw new EnforceException($"Must be a defined enum value. Value: {c.value}");
            }
        }
    }
}
