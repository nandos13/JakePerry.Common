using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace JakePerry.Text.Json
{
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
        internal bool IsEnd => type == TokenType.Undefined || type == TokenType.EndOfFile;

        internal JToken(TokenType type, int start, int count)
        {
            this.type = type;
            this.start = start;
            this.count = count;
        }

        /// <summary>
        /// Obtain the corresponding slice of the <paramref name="source"/> span.
        /// </summary>
        internal ReadOnlySpan<char> Slice(ReadOnlySpan<char> source)
        {
            return source.Slice(start, count);
        }
    }
}
