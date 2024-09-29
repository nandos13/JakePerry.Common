using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace JakePerry
{
    public partial struct Substring
    {
        /// <summary>
        /// Splits a string into substrings that are based on the provided string separator.
        /// </summary>
        /// <param name="value">
        /// Defines the source string to be split.
        /// </param>
        /// <param name="separator">
        /// A string that delimits the substrings in this string.
        /// </param>
        /// <param name="options">
        /// A bitwise combination of the enumeration values that specifies whether to trim
        /// substrings and include empty substrings.
        /// </param>
        private static ParamsArray<Substring> SplitInternal(Substring value, string separator, List<Substring> output, bool wantsParamsArray, StringSplitOptions options = StringSplitOptions.None)
        {
            if (value.m_value is null) return new ParamsArray<Substring>(Substring.Empty);

            if ((separator?.Length ?? 0) == 0)
            {
                output?.Add(value);
                return new ParamsArray<Substring>(value);
            }

            bool keepEmpty = (options & StringSplitOptions.RemoveEmptyEntries) == 0;
            int bound = value.m_start + value.m_length;

            Substring v0 = default, v1 = default, v2 = default;
            List<Substring> list = null;

            int i = 0, j = 0;
            while (i < value.m_length)
            {
                int index = i + value.m_start;

                int next = value.m_value.IndexOf(separator, index, value.m_length - i, StringComparison.Ordinal);

                if (next == -1 || next > bound)
                {
                    next = bound;
                }

                Substring? s = null;
                int size = next - index;
                if (size > 0)
                {
                    s = new Substring(value, i, size);
                }
                else if (keepEmpty)
                {
                    s = new Substring(string.Empty, 0, 0);
                }

                if (s.HasValue)
                {
                    if (j == 0) v0 = s.Value;
                    else if (j == 1) v1 = s.Value;
                    else if (j == 2) v2 = s.Value;
                    else if (wantsParamsArray) (list ??= new()).Add(s.Value);

                    ++j;
                    output?.Add(s.Value);
                }

                i += size + separator.Length;
            }

            if (list is not null)
            {
                var array = new Substring[list.Count + 3];

                array[0] = v0;
                array[1] = v1;
                array[2] = v2;

                list.CopyTo(array, 3);

                return new ParamsArray<Substring>(array);
            }
            else if (j == 1)
            {
                return new ParamsArray<Substring>(v0);
            }
            else if (j == 2)
            {
                return new ParamsArray<Substring>(v0, v1);
            }
            else if (j == 3)
            {
                return new ParamsArray<Substring>(v0, v1, v2);
            }

            return new ParamsArray<Substring>(Substring.Empty);
        }

        /// <inheritdoc cref="SplitInternal(Substring, string, List{Substring}, bool, StringSplitOptions)"/>
        internal ParamsArray<Substring> Split(string separator, StringSplitOptions options = StringSplitOptions.None)
        {
            return SplitInternal(this, separator, null, true, options);
        }

        /// <param name="output">
        /// The output buffer to receive the results.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <inheritdoc cref="SplitInternal(Substring, string, List{Substring}, bool, StringSplitOptions)"/>
        public void Split(string separator, List<Substring> output, StringSplitOptions options = StringSplitOptions.None)
        {
            _ = output ?? throw new ArgumentNullException(nameof(output));

            SplitInternal(this, separator, output, false, options);
        }

        // TODO: Move overloads for split, including single char, char array, string array.

        private static unsafe Substring TrimWhiteSpaceHelper(in Substring s, bool head, bool tail)
        {
            if (s.m_value is null) return default;

            int start = s.m_start;
            int length = s.m_length;

            fixed (char* ptr = s.m_value)
            {
                int bound = start + length;
                int end = bound - 1;

                if (head)
                    for (; start < bound; ++start)
                        if (!char.IsWhiteSpace(ptr[start]))
                        {
                            break;
                        }

                if (tail)
                    for (; end >= start; --end)
                        if (!char.IsWhiteSpace(ptr[end]))
                        {
                            break;
                        }

                length = end - start + 1;
            }

            return new Substring(s.m_value, start, length);
        }

        private static unsafe Substring TrimHelper(in Substring s, char* trimChars, int charCount, bool head, bool tail)
        {
            JPDebug.Assert(trimChars != null);
            JPDebug.Assert(charCount > 0);

            int start = s.m_start;
            int length = s.m_length;

            fixed (char* ptr = s.m_value)
            {
                int bound = start + length;
                int end = bound - 1;

                if (head)
                    for (; start < bound; ++start)
                    {
                        int i = 0;
                        char ch = ptr[start];
                        for (i = 0; i < charCount; i++)
                            if (trimChars[i] == ch)
                            {
                                break;
                            }

                        if (i == charCount) break;
                    }

                if (tail)
                    for (; end >= start; --end)
                    {
                        int i = 0;
                        char ch = ptr[end];
                        for (i = 0; i < charCount; i++)
                            if (trimChars[i] == ch)
                            {
                                break;
                            }

                        if (i == charCount) break;
                    }

                length = end - start + 1;
            }

            return new Substring(s.m_value, start, length);
        }

        /// <summary>
        /// Removes all occurrences of the given <paramref name="trimChar"/>
        /// from both ends of the string.
        /// </summary>
        public unsafe Substring Trim(char trimChar)
        {
            // Optimization mitigates processing if we can easily check no trimming is required.
            if (Length == 0 || (this[0] != trimChar && this[^1] != trimChar))
            {
                return this;
            }

            return TrimHelper(this, &trimChar, 1, head: true, tail: true);
        }

        /// <summary>
        /// Removes all occurrences of a set of characters from both ends of the string.
        /// </summary>
        public unsafe Substring Trim(params char[] trimChars)
        {
            if (trimChars == null || trimChars.Length == 0)
            {
                return TrimWhiteSpaceHelper(this, head: true, tail: true);
            }

            fixed (char* ptr = &trimChars[0])
            {
                return TrimHelper(this, ptr, trimChars.Length, head: true, tail: true);
            }
        }

        /// <summary>
        /// Removes all occurrences of a set of characters from both ends of the string.
        /// </summary>
        public unsafe Substring Trim(ReadOnlySpan<char> trimChars)
        {
            if (trimChars.IsEmpty)
            {
                return TrimWhiteSpaceHelper(this, head: true, tail: true);
            }

            fixed (char* ptr = &MemoryMarshal.GetReference(trimChars))
            {
                return TrimHelper(this, ptr, trimChars.Length, head: true, tail: true);
            }
        }

        /// <summary>
        /// Trims whitespace characters from both ends of the string.
        /// </summary>
        public unsafe Substring Trim()
        {
            return TrimWhiteSpaceHelper(this, head: true, tail: true);
        }

        /// <summary>
        /// Removes all occurrences of the given <paramref name="trimChar"/>
        /// from the start of the string.
        /// </summary>
        public unsafe Substring TrimStart(char trimChar)
        {
            // Optimization mitigates processing if we can easily check no trimming is required.
            if (Length == 0 || (this[0] != trimChar && this[^1] != trimChar))
            {
                return this;
            }

            return TrimHelper(this, &trimChar, 1, head: true, tail: false);
        }

        /// <summary>
        /// Removes all occurrences of a set of characters from the start of the string.
        /// </summary>
        public unsafe Substring TrimStart(params char[] trimChars)
        {
            if (trimChars == null || trimChars.Length == 0)
            {
                return TrimWhiteSpaceHelper(this, head: true, tail: false);
            }

            fixed (char* ptr = &trimChars[0])
            {
                return TrimHelper(this, ptr, trimChars.Length, head: true, tail: false);
            }
        }

        /// <summary>
        /// Removes all occurrences of a set of characters from the start of the string.
        /// </summary>
        public unsafe Substring TrimStart(ReadOnlySpan<char> trimChars)
        {
            if (trimChars.IsEmpty)
            {
                return TrimWhiteSpaceHelper(this, head: true, tail: false);
            }

            fixed (char* ptr = &MemoryMarshal.GetReference(trimChars))
            {
                return TrimHelper(this, ptr, trimChars.Length, head: true, tail: false);
            }
        }

        /// <summary>
        /// Trims whitespace characters from the start of the string.
        /// </summary>
        public unsafe Substring TrimStart()
        {
            return TrimWhiteSpaceHelper(this, true, false);
        }

        /// <summary>
        /// Removes all occurrences of the given <paramref name="trimChar"/>
        /// from the end of the string.
        /// </summary>
        public unsafe Substring TrimEnd(char trimChar)
        {
            // Optimization mitigates processing if we can easily check no trimming is required.
            if (Length == 0 || (this[0] != trimChar && this[^1] != trimChar))
            {
                return this;
            }

            return TrimHelper(this, &trimChar, 1, head: false, tail: true);
        }

        /// <summary>
        /// Removes all occurrences of a set of characters from the end of the string.
        /// </summary>
        public unsafe Substring TrimEnd(params char[] trimChars)
        {
            if (trimChars == null || trimChars.Length == 0)
            {
                return TrimWhiteSpaceHelper(this, head: false, tail: true);
            }

            fixed (char* ptr = &trimChars[0])
            {
                return TrimHelper(this, ptr, trimChars.Length, head: false, tail: true);
            }
        }

        /// <summary>
        /// Removes all occurrences of a set of characters from the end of the string.
        /// </summary>
        public unsafe Substring TrimEnd(ReadOnlySpan<char> trimChars)
        {
            if (trimChars.IsEmpty)
            {
                return TrimWhiteSpaceHelper(this, head: false, tail: true);
            }

            fixed (char* ptr = &MemoryMarshal.GetReference(trimChars))
            {
                return TrimHelper(this, ptr, trimChars.Length, head: false, tail: true);
            }
        }

        /// <summary>
        /// Trims whitespace characters from the end of the string.
        /// </summary>
        public unsafe Substring TrimEnd()
        {
            return TrimWhiteSpaceHelper(this, false, true);
        }
    }
}
