using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace JakePerry.Text.Json
{
    /// <summary>
    /// Representation of a token found in some json data.
    /// Contains the <see cref="TokenType"/> as well as the start index and count
    /// of the token in the source string.
    /// </summary>
    internal struct JToken
    {
        private byte m_type;

        internal int start;
        internal int count;

        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        internal TokenType type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (TokenType)m_type;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => m_type = (byte)value;
        }

        /// <summary>
        /// Indicates whether the token is undefined or if the end of file was reached.
        /// </summary>
        internal bool IsEnd
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => type == TokenType.Undefined || type == TokenType.EndOfFile;
        }

        internal JToken(TokenType type, int start, int count)
        {
            this.type = type;
            this.start = start;
            this.count = count;
        }

        /// <summary>
        /// Obtain the corresponding slice of the <paramref name="source"/> span.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlySpan<char> Slice(ReadOnlySpan<char> source)
        {
            return source.Slice(start, count);
        }
    }
}
