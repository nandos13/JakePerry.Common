using System.Runtime.CompilerServices;
using static JakePerry.FStrings;

namespace JakePerry
{
    /// <summary>
    /// API for enforcing code expectations.
    /// </summary>
    public static partial class Enforce
    {
        /// <summary>
        /// Assert some <paramref name="condition"/> is <see langword="true"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Condition<T>(bool condition, T message)
            where T : struct, IFstring
        {
            if (!condition)
            {
                throw new EnforceException(message.ToString());
            }
        }

        /// <inheritdoc cref="Condition{T}(bool, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Condition(bool condition, string message = null)
        {
            Condition(condition, FString(message));
        }
    }
}
