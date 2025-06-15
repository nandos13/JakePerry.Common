using System;
using System.Runtime.CompilerServices;

namespace JakePerry.Utility
{
    public static class NullableEx
    {
        /// <summary>
        /// Get the value, if the nullable has one.
        /// </summary>
        /// <param name="value">
        /// When this method returns, contains the value held by the nullable, if it has one;
        /// Otherwise, contains the <see langword="default"/> instance.
        /// </param>
        /// <remarks>
        /// Upcasting to <see cref="object"/> or an interface type is supported.
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the nullable has a value; Otherwise, <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<TIn, TOut>(this Nullable<TIn> o, out TOut value)
            where TIn : struct, TOut
        {
            if (o.HasValue)
            {
                // Note: Nullable<T>.Value property performs a safety check which we know is redundant here
                // since we just checked it has a value. As such, GetValueOrDefault() is preferable here.
                value = o.GetValueOrDefault();
                return true;
            }

            value = default;
            return false;
        }
    }
}
