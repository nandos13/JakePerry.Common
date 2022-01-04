using System;
using System.Collections.Generic;

namespace JakePerry
{
    public static class AssignValueUtility
    {
        /// <summary>
        /// Assign a value <paramref name="newValue"/> to a variable referenced via <paramref name="currentValue"/>
        /// and get a boolean indicating whether the variable value was changed.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="currentValue">A reference to the variable to be assigned to.</param>
        /// <param name="newValue">The new value to assign.</param>
        /// <param name="comparer">A comparer by which to check if both values are equal.</param>
        /// <returns>
        /// <see langword="false"/> if <paramref name="currentValue"/> is already equal
        /// to <paramref name="newValue"/>; otherwise, <see langword="true"/>.
        /// </returns>
        public static bool Set<T>(ref T currentValue, T newValue, IEqualityComparer<T> comparer)
        {
            _ = comparer ?? throw new ArgumentNullException(nameof(comparer));

            // If both values are equal (value is not changing), return false.
            if (comparer.Equals(currentValue, newValue))
                return false;

            // The value has changed, set it and return true.
            currentValue = newValue;
            return true;
        }

        /// <inheritdoc cref="Set{T}(ref T, T, IEqualityComparer{T})"/>
        public static bool SetValueType<T>(ref T currentValue, T newValue) where T : struct
        {
            // Use default comparer for value types.
            var comparer = EqualityComparer<T>.Default;

            return Set(ref currentValue, newValue, comparer);
        }

        /// <inheritdoc cref="Set{T}(ref T, T, IEqualityComparer{T})"/>
        public static bool SetReferenceType<T>(ref T currentValue, T newValue) where T : class
        {
            // Use the default object comparer so as to only check if the reference has changed.
            var comparer = EqualityComparer<object>.Default;

            return Set(ref currentValue, newValue, comparer);
        }
    }
}
