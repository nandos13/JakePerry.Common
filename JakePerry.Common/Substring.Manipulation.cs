using System;
using System.Runtime.InteropServices;

namespace JakePerry
{
    public partial struct Substring
    {
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
