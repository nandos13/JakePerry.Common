using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JakePerry.Text.Json
{
    internal static class JsonLexer
    {
        private const string kLiteralFalse = "false";
        private const string kLiteralTrue = "true";
        private const string kLiteralNull = "null";

        internal struct LexerState
        {
            internal int i;
            internal bool insideEscapedString;
        }

        // Copy of char.IsBetween - unavailable
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsBetween(char c, char minInclusive, char maxInclusive) =>
            (uint)(c - minInclusive) <= (uint)(maxInclusive - minInclusive);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SkipWhitespace(scoped ReadOnlySpan<char> s, ref int i)
        {
            while (i < s.Length)
            {
                if (!char.IsWhiteSpace(s[i])) break;

                // TODO: If i decide to track lines for debugging...
                //if (s[i] == '\n') ++line;
                ++i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ReadEscapedUnicodeHex(scoped ReadOnlySpan<char> s, int start)
        {
            if (start + 3 >= s.Length) return false;

            for (int u = 0; u < 4; ++u)
            {
                if (!IsBetween(s[start + u], '0', '9') &&
                    !IsBetween(s[start + u], 'a', 'f') &&
                    !IsBetween(s[start + u], 'A', 'F'))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Read the next segment of an escaped string and produce a token representing the escaped char
        /// or a span of raw (unescaped) string data.
        /// <para/>
        /// Note that validation of all escaped chars is performed in the <see cref="ReadString"/> method.
        /// This method blindly trusts that data is valid; no additional checks are performed.
        /// </summary>
        private static JToken ReadNextEscapedStringToken(scoped ReadOnlySpan<char> s, ref int i, ref LexerState state)
        {
            // End of string
            if (s[i] == '\"')
            {
                state.insideEscapedString = false;
                return new(TokenType.EndEscapedString, i++, 1);
            }

            // Escaper
            if (s[i] == '\\')
            {
                int next = i + 1;
                JToken token = s[next] switch
                {
                    '\"' => new(TokenType.EscapedQuotationMark, i, 2),
                    '/' => new(TokenType.EscapedSolidus, i, 2),
                    '\\' => new(TokenType.EscapedReverseSolidus, i, 2),
                    'b' => new(TokenType.EscapedBackspace, i, 2),
                    'f' => new(TokenType.EscapedFormfeed, i, 2),
                    'n' => new(TokenType.EscapedLinefeed, i, 2),
                    'r' => new(TokenType.EscapedCarriageReturn, i, 2),
                    't' => new(TokenType.EscapedHorizontalTab, i, 2),
                    'u' => new(TokenType.EscapedUnicodeHex, i, 6),

                    // Invalid escaped chars should be caught in ReadString and result in an Undefined token.
                    // This code path should not run under normal circumstances.
                    _ => throw new InternalFailureException("Unexpected invalid escape token encountered after validation pass.")
                };

                i += token.count;
                return token;
            }

            int start = i++;
            for (; i < s.Length; ++i)
            {
                if (s[i] == '\n' || s[i] == '\"' || s[i] == '\\')
                    break;
            }

            return new JToken(TokenType.RawStringData, start, i - start);
        }

        /// <summary>
        /// Read a string from the opening quotation mark, up until the closing unescaped quotation mark is found.
        /// <para/>
        /// If the string contains one or more escaped chars:
        /// <para/>
        /// <list type="bullet">
        /// <item>
        /// All escaped chars are fully validated (eg. escaped unicode hex '\u' tokens must be followed
        /// by exactly 4 hex digits).
        /// </item>
        /// <item>
        /// <paramref name="state"/>'s <see cref="LexerState.insideEscapedString"/> is set to <see langword="true"/>.
        /// </item>
        /// <item>
        /// A token with type <see cref="TokenType.BeginEscapedString"/> is returned.
        /// </item>
        /// <item>
        /// Subsequent token reads must use the <see cref="ReadNextEscapedStringToken"/> method to read segments of the string.
        /// </item>
        /// </list>
        /// Otherwise, a token with type <see cref="TokenType.String"/> is returned, representing the entire string.
        /// </summary>
        private static JToken ReadString(scoped ReadOnlySpan<char> s, ref int i, ref LexerState state)
        {
            int start = i++;

            bool escaped = false;
            for (; i < s.Length; ++i)
            {
                // Unescaped newline is invalid
                if (s[i] == '\n')
                {
                    return JToken.Undefined(start);
                }
                // String end
                if (s[i] == '\"')
                {
                    // If no escaped chars were encountered, return a regular string token
                    if (!escaped)
                    {
                        int end = i++;
                        return new JToken(TokenType.String, start, end - start + 1);
                    }

                    // Otherwise, return the BeginEscapedString
                    i = start + 1;
                    state.insideEscapedString = true;
                    return new JToken(TokenType.BeginEscapedString, start, 1);
                }

                // Escaper
                if (s[i] == '\\')
                {
                    escaped = true;

                    // Validate up to the end of the string
                    int next = i + 1;
                    if (next >= s.Length) break;

                    // Escaped Unicode must have exactly 4 hex digits
                    if (s[next] == 'u')
                    {
                        if (!ReadEscapedUnicodeHex(s, next + 1))
                        {
                            return JToken.Undefined(start);
                        }
                        // Advance index to last hex digit (for loop will advance past it)
                        i += 5;
                    }
                    else
                    {
                        char c = s[next];
                        if (c == '\"' || c == '/' || c == '\\' ||
                            c == 'b' || c == 'f' || c == 'n' || c == 'r' || c == 't')
                        {
                            ++i;
                        }
                        else
                        {
                            return JToken.Undefined(start);
                        }
                    }
                }
            }

            // Reaching EOF before string ends is invalid
            return JToken.Undefined(start);
        }

        private static bool IsValidPostValueChar(char c)
        {
            if (char.IsWhiteSpace(c)) return true;

            // As far as the lexer is concerned, a numeric or literal value has come to a valid end
            // if it is followed by whitespace or one of the following chars.
            // Of course, some of these chars would not form valid JSON, but the parser is responsible
            // for figuring that out.
            Span<char> validChars = stackalloc char[7] { '{', '}', '[', ']', ':', ',', '\"' };
            foreach (var c2 in validChars)
                if (c == c2)
                {
                    return true;
                }
            return false;
        }

        private static JToken ReadValue(scoped ReadOnlySpan<char> s, ref int i)
        {
            // Number values
            if (s[i] == '-' || IsBetween(s[i], '0', '9'))
            {
                bool anyDigit = s[i] != '-';
                bool anyNonZeroDigit = anyDigit && s[i] != '0';

                int next = i + 1;
                int fractionIndex = -1;
                int exponentIndex = -1;
                for (; next < s.Length; ++next)
                {
                    if (s[next] == 'e' || s[next] == 'E')
                    {
                        // More than one exponent is invalid.
                        // '1e5' - valid
                        // '1e5e6' - invalid
                        if (exponentIndex > -1)
                        {
                            return JToken.Undefined(i);
                        }

                        // Exponents without leading digits are invalid. Digit(s) do not have to be non-zero.
                        // '0e5' - valid
                        // '1e5' - valid
                        // 'e5' - invalid
                        // '-e5' - invalid
                        if (!anyDigit)
                        {
                            return JToken.Undefined(i);
                        }

                        exponentIndex = i;

                        // Exponent allows optional positive (+) and negative (-) signs.
                        // Checking here means we don't have to introduce a branch in every loop iteration.
                        if (next + 1 < s.Length && (s[next + 1] == '+' || s[next + 1] == '-'))
                        {
                            // Advance the index since we know it's valid
                            ++next;
                        }

                        continue;
                    }

                    if (s[next] == '.')
                    {
                        // More than one fraction part is invalid.
                        // Fraction after exponent is invalid.
                        // '1.1.1' - invalid
                        // '1e5.1' - invalid
                        if (fractionIndex > -1 || exponentIndex > -1)
                        {
                            return JToken.Undefined(i);
                        }

                        // Fractions without leading digits are invalid. Digit(s) do not have to be non-zero.
                        // '.01' - invalid
                        if (!anyDigit)
                        {
                            // TODO: Consider supporting some enum of 'lexer failure reasons' on jtoken? (only when its undefined)
                            return JToken.Undefined(i);
                        }

                        fractionIndex = next;
                        continue;
                    }

                    if (IsBetween(s[next], '0', '9'))
                    {
                        anyDigit = true;
                        anyNonZeroDigit |= s[next] != 0;
                    }
                    else
                    {
                        break;
                    }
                }

                // Numbers must end with a digit char.
                // '1.0' - valid
                // '1.' - invalid
                // '1e+5 - valid
                // '1e+' - invalid
                // '-' - invalid
                if (!IsBetween(s[next - 1], '0', '9'))
                {
                    return JToken.Undefined(i);
                }

                // Leading zeros are not valid.
                // '0' - valid
                // '0.0' - valid
                // '0.01' - valid
                // '00' - invalid
                // '01' - invalid
                int firstDigitIndex = s[i] == '-' ? i + 1 : i;
                if (s[firstDigitIndex] == 0)
                {
                    int secondDigitIndex = firstDigitIndex + 1;
                    bool zeroDigitIsValid =
                        secondDigitIndex == next ||
                        secondDigitIndex == exponentIndex ||
                        secondDigitIndex == fractionIndex;

                    if (!zeroDigitIsValid)
                    {
                        return JToken.Undefined(i);
                    }
                }

                int start = i;
                i = next;

                if (next < s.Length && !IsValidPostValueChar(s[next]))
                {
                    return JToken.Undefined(i);
                }

                return new JToken(TokenType.Number, start, next - start);
            }
            // Literal values (false, true, null)
            else
            {
                var substr = s.Slice(i);

                TokenType token;
                int size;
                if (substr.StartsWith(kLiteralFalse, StringComparison.Ordinal))
                {
                    token = TokenType.False;
                    size = kLiteralFalse.Length;
                }
                else if (substr.StartsWith(kLiteralTrue, StringComparison.Ordinal))
                {
                    token = TokenType.True;
                    size = kLiteralTrue.Length;
                }
                else if (substr.StartsWith(kLiteralNull, StringComparison.Ordinal))
                {
                    token = TokenType.Null;
                    size = kLiteralNull.Length;
                }
                else
                {
                    return JToken.Undefined(i);
                }

                int next = i + size;
                if (next < s.Length && !IsValidPostValueChar(s[next]))
                {
                    return JToken.Undefined(i);
                }

                int start = i;
                i = next;

                return new JToken(token, start, size);
            }
        }

        /// <summary>
        /// Read the next token, starting from the current index held by <paramref name="state"/>.
        /// Any preceding whitespace will be ignored.
        /// </summary>
        /// <param name="s">
        /// Input span of JSON data.
        /// </param>
        /// <param name="state">
        /// The current state of the lexer. This contains the index at which to read the next token,
        /// as well as whether the cursor is within an escaped string.
        /// This method advances the state to the next read position.
        /// </param>
        /// <returns>
        /// A <see cref="JToken"/> representation of the next token that is found
        /// in the data span <paramref name="s"/>.
        /// </returns>
        internal static JToken NextToken(scoped ReadOnlySpan<char> s, ref LexerState state)
        {
            ref int i = ref state.i;

            // Continue reading an escaped string sequence
            if (state.insideEscapedString)
            {
                if (i == s.Length)
                {
                    return new JToken(TokenType.EndOfFile, i, 0);
                }

                return ReadNextEscapedStringToken(s, ref i, ref state);
            }

            SkipWhitespace(s, ref i);

            if (i == s.Length)
            {
                return new JToken(TokenType.EndOfFile, i, 0);
            }

            switch (s[i])
            {
                case '{':
                    {
                        return new JToken(TokenType.BeginObject, i++, 1);
                    }
                case '}':
                    {
                        return new JToken(TokenType.EndObject, i++, 1);
                    }
                case '[':
                    {
                        return new JToken(TokenType.BeginArray, i++, 1);
                    }
                case ']':
                    {
                        return new JToken(TokenType.EndArray, i++, 1);
                    }
                case ':':
                    {
                        return new JToken(TokenType.Colon, i++, 1);
                    }
                case ',':
                    {
                        return new JToken(TokenType.Comma, i++, 1);
                    }
                case '\"':
                    {
                        return ReadString(s, ref i, ref state);
                    }
                default:
                    {
                        return ReadValue(s, ref i);
                    }
            }
        }

        // TODO: Documentation. Mention how its used with the heap enumerator
        // to work around ref struct limitations.
        internal readonly struct Cursor
        {
            private readonly LexerState m_state;
            private readonly bool m_end;

            internal LexerState State => m_state;

            internal bool IsEnd => m_end;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Cursor(LexerState state, bool end)
            {
                m_state = state;
                m_end = end;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Cursor(LexerState state) : this(state, false) { }

            internal static Cursor Start
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(new LexerState());
            }

            internal static Cursor End
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(new LexerState() { i = -1 }, true);
            }
        }

        /// <summary>
        /// A stack-only struct that can be used to read JSON tokens using the
        /// <see cref="NextToken"/> method.
        /// </summary>
        internal ref struct Enumerator
        {
            private readonly ReadOnlySpan<char> m_span;
            private LexerState m_state;
            private JToken m_token;
            private bool m_moved;

            public readonly JToken Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_token;
            }

            /// <summary>
            /// Get a <see cref="JsonLexer.Cursor"/> representing the enumerator's current read position.
            /// </summary>
            internal readonly Cursor Cursor
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_moved && m_token.IsEnd ? Cursor.End : new(m_state);
            }

            internal ReadOnlySpan<char> Span
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_span;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ReadOnlySpan<char> span)
            {
                m_span = span;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ReadOnlySpan<char> span, Cursor cursor)
            {
                m_span = span;
                m_state = cursor.State;
                m_token = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (m_moved && m_token.IsEnd)
                {
                    return false;
                }

                m_token = NextToken(m_span, ref m_state);
                m_moved = true;
                return true;
            }

            /// <summary>
            /// Reads all tokens from the JSON data in <paramref name="span"/> into
            /// the <paramref name="output"/> list.
            /// </summary>
            /// <param name="span">
            /// Input span of JSON data.
            /// </param>
            /// <param name="output">
            /// The output token list.
            /// </param>
            /// <returns>
            /// Returns the <paramref name="output"/> list for fluent method chaining.
            /// </returns>
            /// <exception cref="ArgumentNullException"/>
            internal static List<JToken> Read(in ReadOnlySpan<char> span, List<JToken> output)
            {
                Enforce.Argument(output, nameof(output)).IsNotNull();

                var enumerator = new Enumerator(span);
                while (enumerator.MoveNext())
                {
                    output.Add(enumerator.Current);
                }

                return output;
            }

            /// <summary>
            /// Reads a number of tokens from the JSON data in <paramref name="span"/> into
            /// the <paramref name="output"/> buffer.
            /// </summary>
            /// <param name="span">
            /// Input span of JSON data.
            /// </param>
            /// <param name="cursor">
            /// The cursor to a location in the <paramref name="span"/>.
            /// </param>
            /// <param name="offset">
            /// An offset at which to write into the <paramref name="output"/> buffer.
            /// </param>
            /// <param name="count">
            /// The number of tokens to be read.
            /// </param>
            /// <param name="output">
            /// The output token buffer.
            /// </param>
            /// <returns>
            /// Returns the number of tokens that were read into the <paramref name="output"/> buffer.
            /// </returns>
            /// <exception cref="ArgumentNullException"/>
            internal static int Read(in ReadOnlySpan<char> span, ref Cursor cursor, int offset, int count, JToken[] output)
            {
                Enforce.Argument(output, nameof(output)).IsNotNull();

                if (cursor.IsEnd) throw new ArgumentException("Cursor reached end.", nameof(cursor));

                if (offset < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset), offset, SR.ArgumentOutOfRange_NeedNonNegNum);
                }
                if (count <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count), count, SR.ArgumentOutOfRange_MustBePositive);
                }
                if (offset + count > output.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(count), count, SR.@Argument_InvalidOffLen);
                }

                int i = 0;

                var enumerator = new Enumerator(span, cursor);
                while (enumerator.MoveNext())
                {
                    output[i + offset] = enumerator.Current;
                    cursor = enumerator.Cursor;

                    if (++i == count) break;
                }

                return i;
            }
        }

        /// <summary>
        /// A stack-only struct that can be used to read JSON tokens.
        /// </summary>
        /// <seealso cref="Enumerator"/>
        internal readonly ref struct Enumerable
        {
            private readonly ReadOnlySpan<char> m_span;

            internal ReadOnlySpan<char> Span
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_span;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerable(ReadOnlySpan<char> span)
            {
                m_span = span;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() => new(m_span);

            /// <inheritdoc cref="Enumerator.Read(in ReadOnlySpan{char}, List{JToken})"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal List<JToken> Read(List<JToken> output)
            {
                return Enumerator.Read(m_span, output);
            }

            /// <inheritdoc cref="Enumerator.Read(in ReadOnlySpan{char}, ref Cursor, int, int, JToken[])"/>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal int Read(ref Cursor cursor, int count, JToken[] output, int offset = 0)
            {
                return Enumerator.Read(m_span, ref cursor, offset, count, output);
            }
        }

        /// <summary>
        /// A reference-type enumerable implementation that can be used to read JSON tokens.
        /// <para/>
        /// This type fully implements the <see cref="IEnumerable{T}"/> interface, allowing it to be
        /// used with LINQ methods, etc.
        /// </summary>
        internal sealed class HeapEnumerable : IEnumerable<JToken>
        {
            private struct HeapEnumerator : IEnumerator<JToken>
            {
                private readonly ReadOnlyMemory<char> m_memory;
                private Cursor m_cursor;
                private JToken m_token;

                public JToken Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => m_token;
                }

                object IEnumerator.Current => this.Current;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal HeapEnumerator(ReadOnlyMemory<char> memory)
                {
                    m_memory = memory;
                    m_cursor = Cursor.Start;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    if (m_cursor.IsEnd) return false;

                    var enumerator = new Enumerator(m_memory.Span, m_cursor);

                    bool moved = enumerator.MoveNext();
                    m_cursor = enumerator.Cursor;
                    m_token = enumerator.Current;

                    return moved;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Reset() => m_cursor = Cursor.Start;

                void IDisposable.Dispose() { }
            }

            private readonly ReadOnlyMemory<char> m_memory;

            internal HeapEnumerable(ReadOnlyMemory<char> memory)
            {
                m_memory = memory;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator GetEnumerator() => new(m_memory.Span);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private HeapEnumerator GetHeapEnumerator() => new(m_memory);

            IEnumerator IEnumerable.GetEnumerator() => this.GetHeapEnumerator();
            IEnumerator<JToken> IEnumerable<JToken>.GetEnumerator() => this.GetHeapEnumerator();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerable ToStackEnumerable() => new(m_memory.Span);
        }

        /// <summary>
        /// Enumerates all tokens in the JSON data <paramref name="span"/>.
        /// <para/>
        /// This method returns the stack-only ref struct <see cref="Enumerable"/>
        /// and can be used to iterate the tokens in place, such as directly in a <see langword="foreach"/> loop.
        /// If an implementation of <see cref="IEnumerable{T}"/> is required (ie. for use with LINQ, or to pass
        /// elsewhere for later use), use the <see cref="TokenizeHeapMemory(ReadOnlyMemory{char})"/> method.
        /// </summary>
        /// <param name="span">
        /// Input span of JSON data.
        /// </param>
        internal static Enumerable TokenizeSpan(ReadOnlySpan<char> span) => new(span);

        /// <summary>
        /// Enumerates all tokens in the JSON data stored in <paramref name="memory"/>.
        /// <para/>
        /// This method returns a <see cref="HeapEnumerable"/> object, which implements the <see cref="IEnumerable{T}"/>
        /// interface. If this is not required, the <see cref="TokenizeSpan(ReadOnlySpan{char})"/>
        /// method can be used to iterate JSON tokens entirely on the stack, avoiding additional overhead.
        /// </summary>
        /// <param name="memory">
        /// Input span of JSON data.
        /// </param>
        internal static HeapEnumerable TokenizeHeapMemory(ReadOnlyMemory<char> memory) => new(memory);

        /// <inheritdoc cref="TokenizeHeapMemory(ReadOnlyMemory{char})"/>
        internal static HeapEnumerable TokenizeHeapMemory(string buffer)
        {
            Enforce.Argument(buffer, nameof(buffer)).IsNotNull();

            return new(buffer.AsMemory());
        }
    }
}
