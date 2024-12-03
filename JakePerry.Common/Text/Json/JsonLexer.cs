using System;
using System.Runtime.CompilerServices;

namespace JakePerry.Text.Json
{
    internal static class JsonLexer
    {
        private const string kLiteralFalse = "false";
        private const string kLiteralTrue = "true";
        private const string kLiteralNull = "null";

        // Copy of char.IsBetween
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsBetween(char c, char minInclusive, char maxInclusive) =>
            (uint)(c - minInclusive) <= (uint)(maxInclusive - minInclusive);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEscaped(scoped ReadOnlySpan<char> s, int i)
        {
            int escaperCount = 0;
            for (; i > 0 && s[i - 1] == '\\'; --i)
            {
                ++escaperCount;
            }
            return escaperCount % 2 == 1;
        }

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

        private static JToken NextToken(scoped ReadOnlySpan<char> s, ref int i)
        {
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
                        return ReadString(s, ref i);
                    }
                default:
                    {
                        return ReadValue(s, ref i);
                    }
            }
        }

        internal ref struct Enumerator
        {
            private readonly ReadOnlySpan<char> m_span;
            private int m_index;
            private JToken m_token;

            public readonly JToken Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => m_token;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(ReadOnlySpan<char> span)
            {
                m_span = span;
                m_index = -1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                if (m_index > 0 && m_token.IsEnd)
                {
                    return false;
                }

                m_token = NextToken(m_span, ref m_index);
                return true;
            }
        }

        internal readonly ref struct Tokenizer
        {
            private readonly ReadOnlySpan<char> m_span;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Tokenizer(ReadOnlySpan<char> span)
            {
                m_span = span;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator() => new(m_span);
        }

        internal static Tokenizer Tokenize(ReadOnlySpan<char> span) => new(span);
    }
}
