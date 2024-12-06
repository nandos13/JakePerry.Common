namespace JakePerry
{
    internal static partial class SR
    {
        internal static class Strings
        {
            internal const string @Arg_ArrayPlusOffTooSmall = "Arg_ArrayPlusOffTooSmall";
            internal const string @Arg_WrongType = "Arg_WrongType";
            internal const string @Argument_AddingDuplicate = "Argument_AddingDuplicate";
            internal const string @Argument_InvalidOffLen = "Argument_InvalidOffLen";
            internal const string @ArgumentOutOfRange_Count = "ArgumentOutOfRange_Count";
            internal const string @ArgumentOutOfRange_MustBePositive = "ArgumentOutOfRange_MustBePositive";
            internal const string @ArgumentOutOfRange_NeedNonNegNum = "ArgumentOutOfRange_NeedNonNegNum";
            internal const string @Exception_EndStackTraceFromPreviousThrow = "Exception_EndStackTraceFromPreviousThrow";
            internal const string @NotSupported_KeyCollectionSet = "NotSupported_KeyCollectionSet";
        }

        /// <summary>Destination array is not long enough to copy all the items in the collection. Check array index and length.</summary>
        internal static string @Arg_ArrayPlusOffTooSmall => GetResourceString(Strings.@Arg_ArrayPlusOffTooSmall);

        /// <summary>TWO ARGUMENTS<para/>The value '{0}' is not of type '{1}' and cannot be used in this generic collection.</summary>
        internal static string @Arg_WrongType => GetResourceString(Strings.@Arg_WrongType);

        /// <summary>ONE ARGUMENT<para/>An item with the same key has already been added. Key: {0}</summary>
        internal static string @Argument_AddingDuplicate => GetResourceString(Strings.@Argument_AddingDuplicate);

        /// <summary>Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.</summary>
        internal static string @Argument_InvalidOffLen => GetResourceString(Strings.@Argument_InvalidOffLen);

        /// <summary>Count must be positive and count must refer to a location within the string/array/collection.</summary>
        internal static string @ArgumentOutOfRange_Count => GetResourceString(Strings.@ArgumentOutOfRange_Count);

        /// <summary>'{0}' must be greater than zero.</summary>
        internal static string @ArgumentOutOfRange_MustBePositive => GetResourceString(Strings.@ArgumentOutOfRange_MustBePositive);

        /// <summary>Non-negative number required.</summary>
        internal static string @ArgumentOutOfRange_NeedNonNegNum => GetResourceString(Strings.@ArgumentOutOfRange_NeedNonNegNum);

        /// <summary>--- End of stack trace from previous location ---</summary>
        internal static string @Exception_EndStackTraceFromPreviousThrow => GetResourceString(Strings.@Exception_EndStackTraceFromPreviousThrow);

        /// <summary>Mutating a key collection derived from a dictionary is not allowed.</summary>
        internal static string @NotSupported_KeyCollectionSet => GetResourceString(Strings.@NotSupported_KeyCollectionSet);
    }
}
