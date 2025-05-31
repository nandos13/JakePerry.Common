using System.Runtime.CompilerServices;

namespace JakePerry
{
    /// <summary>
    /// Interface describing a lazy formatted string.
    /// See the <see cref="FStrings"/> documentation for more info.
    /// </summary>
    public interface IFstring : IMightBeValid, IStruct
    {
        string ToString();
    }

    /// <summary>
    /// API for working with lazy formatted strings. An 'FString' is a
    /// <see langword="struct"/> which couples a format string &amp; a number
    /// of format arguments, and implements the <see cref="IFstring"/>
    /// interface to provide the resulting formatted string when required.
    /// <para/>
    /// <b>Usage:</b><para/>
    /// Add a static using statement for the <see cref="FStrings"/> type,
    /// then construct an FString with desired arguments:
    /// <code>
    /// using static JakePerry.FStrings;
    /// 
    /// void Foo(User user)
    /// {
    ///     print(FString("My name is {0}", user.Name));
    /// }
    /// </code>
    /// </summary>
    public readonly struct FStrings
    {
        /// <summary>
        /// <see cref="IFstring"/> implementation with no arguments.
        /// </summary>
        public struct FString0 : IFstring
        {
            public string str;

            public readonly bool IsValid
                => str is not null;

            public readonly override string ToString() => str;
        }

        /// <summary>
        /// <see cref="IFstring"/> implementation with 1 argument.
        /// </summary>
        public struct FString1<T0> : IFstring
        {
            public string format;
            public T0 arg0;

            public readonly bool IsValid
                => format is not null;

            public readonly override string ToString()
                => format is null ? string.Empty : string.Format(format, arg0?.ToString());
        }

        /// <summary>
        /// <see cref="IFstring"/> implementation with 2 arguments.
        /// </summary>
        public struct FString2<T0, T1> : IFstring
        {
            public string format;
            public T0 arg0;
            public T1 arg1;

            public readonly bool IsValid
                => format is not null;

            public readonly override string ToString()
                => format is null ? string.Empty : string.Format(format, arg0?.ToString(), arg1?.ToString());
        }

        /// <summary>
        /// <see cref="IFstring"/> implementation with 3 arguments.
        /// </summary>
        public struct FString3<T0, T1, T2> : IFstring
        {
            public string format;
            public T0 arg0;
            public T1 arg1;
            public T2 arg2;

            public readonly bool IsValid
                => format is not null;

            public readonly override string ToString()
                => format is null ? string.Empty : string.Format(format, arg0?.ToString(), arg1?.ToString(), arg2?.ToString());
        }

        /// <summary>
        /// Construct an FString with no formatting arguments.
        /// </summary>
        /// <param name="str">
        /// The preformatted string to be treated as an FString.
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FString0 FString(string str)
            => new() { str = str };

        /// <summary>
        /// Construct a formatted string with the given format and argument(s).
        /// <para/>
        /// Example:
        /// <code>
        /// print(FString("My name is {0}", user.Name));
        /// </code>
        /// </summary>
        /// <param name="format">
        /// The format string. ie. <c>"My name is {0}"</c>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FString1<T0> FString<T0>(string format, T0 arg0)
            => new() { format = format, arg0 = arg0 };

        /// <inheritdoc cref="FString{T0}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FString2<T0, T1> FString<T0, T1>(string format, T0 arg0, T1 arg1)
            => new() { format = format, arg0 = arg0, arg1 = arg1 };

        /// <inheritdoc cref="FString{T0}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FString3<T0, T1, T2> FString<T0, T1, T2>(string format, T0 arg0, T1 arg1, T2 arg2)
            => new() { format = format, arg0 = arg0, arg1 = arg1, arg2 = arg2 };
    }
}
