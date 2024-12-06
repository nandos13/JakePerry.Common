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

        private static JToken ReadString(scoped ReadOnlySpan<char> s, ref int i)
        {
            int start = i++;

            bool escaping = false;
            for (; i < s.Length; ++i)
            {
                if (s[i] == '\\')
                {
                    escaping = !escaping;
                    continue;
                }
                else if (s[i] == '\n')
                {
                    ++i;
                    return new JToken(TokenType.Undefined, start, i - start);
                }
                else if (s[i] == '\"' && !escaping)
                {
                    int end = i++;
                    return new JToken(TokenType.String, start, end - start + 1);
                }

                escaping = false;
            }

            return new JToken(TokenType.EndOfFile, i, 0);
        }

        private static bool IsValidPostValueChar(char c)
        {
            if (char.IsWhiteSpace(c)) return true;

            // As far as the lexer is concerned, a numeric or literal value has come to a valid end
            // if it is followed by whitespace or one of the following chars.
            // Of course, some of these chars would not form valid json, but the parser is responsible
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
                int next = i + 1;
                bool hasDecimal = false;
                bool hasExponent = false;
                for (; next < s.Length; ++next)
                {
                    if (s[next] == 'e' || s[next] == 'E')
                    {
                        // If we already found exponential notation, the char is illegal.
                        if (hasExponent)
                        {
                            i = next + 1;
                            return new JToken(TokenType.Undefined, next, 1);
                        }

                        hasExponent = true;

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
                        // If we already found a decimal or exponent preceeded it, the period char is illegal.
                        if (hasDecimal || hasExponent)
                        {
                            i = next + 1;
                            return new JToken(TokenType.Undefined, next, 1);
                        }

                        hasDecimal = true;
                        continue;
                    }

                    if (!IsBetween(s[next], '0', '9'))
                    {
                        break;
                    }
                }

                // Numbers must end with a digit char.
                if (!IsBetween(s[next - 1], '0', '9'))
                {
                    i = next + 1;
                    return new JToken(TokenType.Undefined, next, 0);
                }

                int start = i;
                i = next;

                if (next < s.Length && !IsValidPostValueChar(s[next]))
                {
                    return new JToken(TokenType.Undefined, i, 0);
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
                    return new JToken(TokenType.Undefined, i++, 0);
                }

                int next = i + size;
                if (next < s.Length && !IsValidPostValueChar(s[next]))
                {
                    return new JToken(TokenType.Undefined, i++, 0);
                }

                int start = i;
                i = next;

                return new JToken(token, start, size);
            }
        }

        /// <summary>
        /// Read the next token, starting from index <paramref name="i"/>.
        /// Any preceding whitespace will be ignored.
        /// </summary>
        /// <param name="s">
        /// Input span of json data.
        /// </param>
        /// <param name="i">
        /// Index at which to read. This method advances the value to the next read position.
        /// </param>
        /// <returns>
        /// A <see cref="JToken"/> representation of the next token that is found
        /// in the data span <paramref name="s"/>.
        /// </returns>
        internal static JToken NextToken(scoped ReadOnlySpan<char> s, ref int i)
        {
            SkipWhitespace(s, ref i);

            if (i == s.Length)
            {
                return new JToken(TokenType.EndOfFile, i++, 0);
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
                        return ReadString(s, ref i);
                    }
                default:
                    {
                        return ReadValue(s, ref i);
                    }
            }
        }

        // TODO: Documentation. Mention how its used with the heap enumerator
        // to work around ref struct limitations.
        // TODO: Possible to make some of this reusable, ie. pagination tokens?
        //       Cursor<T>
        internal readonly struct Cursor
        {
            private readonly int m_index;
            private readonly bool m_end;

            internal int Index => m_index;

            internal bool IsEnd => m_end;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Cursor(int index, bool end)
            {
                m_index = index;
                m_end = end;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Cursor(int index) : this(index, false) { }

            internal static Cursor Start
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(0);
            }

            internal static Cursor End
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => new(-1, true);
            }
        }

        /// <summary>
        /// A stack-only struct that can be used to read json tokens using the
        /// <see cref="NextToken(ReadOnlySpan{char}, ref int)"/> method.
        /// </summary>
        internal ref struct Enumerator
        {
            private readonly ReadOnlySpan<char> m_span;
            private int m_index;
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
                get => m_index > 0 && m_token.IsEnd ? Cursor.End : new(m_index);
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
                m_index = cursor.Index;
                m_token = default;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (m_moved && m_token.IsEnd)
                {
                    return false;
                }

                m_token = NextToken(m_span, ref m_index);
                m_moved = true;
                return true;
            }

            /// <summary>
            /// Reads all tokens from the json data in <paramref name="span"/> into
            /// the <paramref name="output"/> list.
            /// </summary>
            /// <param name="span">
            /// Input span of json data.
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
                _ = output ?? throw new ArgumentNullException(nameof(output));

                var enumerator = new Enumerator(span);
                while (enumerator.MoveNext())
                {
                    output.Add(enumerator.Current);
                }

                return output;
            }

            /// <summary>
            /// Reads a number of tokens from the json data in <paramref name="span"/> into
            /// the <paramref name="output"/> buffer.
            /// </summary>
            /// <param name="span">
            /// Input span of json data.
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
                _ = output ?? throw new ArgumentNullException(nameof(output));

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
        /// A stack-only struct that can be used to read json tokens.
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
        /// A reference-type enumerable implementation that can be used to read json tokens.
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
        /// Enumerates all tokens in the json data <paramref name="span"/>.
        /// <para/>
        /// This method returns the stack-only ref struct <see cref="Enumerable"/>
        /// and can be used to iterate the tokens in place, such as directly in a <see langword="foreach"/> loop.
        /// If an implementation of <see cref="IEnumerable{T}"/> is required (ie. for use with LINQ, or to pass
        /// elsewhere for later use), use the <see cref="TokenizeHeapMemory(ReadOnlyMemory{char})"/> method.
        /// </summary>
        /// <param name="span">
        /// Input span of json data.
        /// </param>
        internal static Enumerable TokenizeSpan(ReadOnlySpan<char> span) => new(span);

        /// <summary>
        /// Enumerates all tokens in the json data stored in <paramref name="memory"/>.
        /// <para/>
        /// This method returns a <see cref="HeapEnumerable"/> object, which implements the <see cref="IEnumerable{T}"/>
        /// interface. If this is not required, the <see cref="TokenizeSpan(ReadOnlySpan{char})"/>
        /// method can be used to iterate json tokens entirely on the stack, avoiding additional overhead.
        /// </summary>
        /// <param name="memory">
        /// Input span of json data.
        /// </param>
        internal static HeapEnumerable TokenizeHeapMemory(ReadOnlyMemory<char> memory) => new(memory);

        /// <inheritdoc cref="TokenizeHeapMemory(ReadOnlyMemory{char})"/>
        internal static HeapEnumerable TokenizeHeapMemory(string buffer)
        {
            _ = buffer ?? throw new ArgumentNullException(nameof(buffer));

            return new(buffer.AsMemory());
        }
    }
}
