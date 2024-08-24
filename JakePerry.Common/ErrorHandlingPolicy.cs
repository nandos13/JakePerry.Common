using System;

namespace JakePerry
{
    /// <summary>
    /// An enumeration of possible error handling policies.
    /// </summary>
    public enum ErrorHandlingPolicy : byte
    {
        /// <summary>
        /// The default policy is used.
        /// <para/>
        /// Depending on implementation, the default policy may be defined
        /// by some configuration data or simply by the method requesting
        /// the <see cref="ErrorHandlingPolicy"/> value.
        /// </summary>
        Default = 0,

        /// <summary>
        /// The error is ignored.
        /// <para/>
        /// Implementation handles the failure gracefully and exit without
        /// throwing an exception.
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// The error is logged.
        /// <para/>
        /// Implementation handles the failure gracefully and exit without
        /// throwing an exception.
        /// </summary>
        Log = 2,

        /// <summary>
        /// An exception is thrown.
        /// </summary>
        Throw = 3
    }

    /// <summary>
    /// Contains helper methods for the <see cref="ErrorHandlingPolicy"/> enum type.
    /// </summary>
    public static class ErrorHandlingPolicyEx
    {
        /// <summary>
        /// Inline helper to fall back to the relevant global policy.
        /// </summary>
        /// <param name="fallback">
        /// The fallback policy value.
        /// </param>
        /// <returns>
        /// Returns the current policy enum value, <paramref name="fallback"/> if
        /// the current value is equal to <see cref="ErrorHandlingPolicy.Default"/>.
        /// </returns>
        public static ErrorHandlingPolicy OrFallback(this ErrorHandlingPolicy o, ErrorHandlingPolicy fallback)
        {
            return (o == ErrorHandlingPolicy.Default) ? fallback : o;
        }

        /// <summary>
        /// Cast the current policy enum value to a byte.
        /// </summary>
        public static byte ToByte(this ErrorHandlingPolicy value)
        {
            if (value < ErrorHandlingPolicy.Default ||
                value > ErrorHandlingPolicy.Throw)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            return (byte)(int)value;
        }
    }
}
