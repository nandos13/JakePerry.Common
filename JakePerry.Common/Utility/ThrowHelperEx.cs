using System;

namespace JakePerry
{
    /// <summary>
    /// Emulates the core library's internal ThrowHelper class.
    /// </summary>
    internal static class ThrowHelperEx
    {
        /// <summary>
        /// Contains resource names for error messages.
        /// </summary>
        internal static class Msg
        {
            internal const string Argument_InvalidOffLen = "Argument_InvalidOffLen";
        }

        internal static void ThrowArgumentException(string resource)
        {
            throw new ArgumentException(EnvironmentEx.GetResourceString(resource));
        }

        internal static void ThrowWrongValueTypeArgumentException(object value, Type targetType)
        {
            throw new ArgumentException(EnvironmentEx.GetResourceString("Arg_WrongType", value, targetType), "value");
        }
    }
}
