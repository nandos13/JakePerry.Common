using JakePerry.Collections;
using JakePerry.Text;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JakePerry
{
    public partial struct Substring
    {
        /// <summary>
        /// Processes a split substring per the given <paramref name="options"/>, and adds
        /// it to the <paramref name="splits"/> list unless the substring is empty and empty
        /// entries should be ignored.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ProcessSplit(
            in Substring s,
            int initialOffset,
            scoped ref StackList<(int Offset, int Count)> splits,
            StringSplitOptions options)
        {
            if (!s.IsEmpty || (options & StringSplitOptions.RemoveEmptyEntries) == 0)
            {
                splits.Append((s.StartIndex - initialOffset, s.Length));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Once the split delimiters are known, this method actually finds and processes the
        /// substring splits.
        /// </summary>
        private static void SplitFromKnownIndexes(
            in Substring s,
            int initialOffset,
            scoped ref StackList<(int Offset, int Count)> splits,
            scoped ReadOnlySpan<int> indexes,
            scoped ReadOnlySpan<int> lengths,
            int len,
            int count,
            StringSplitOptions options)
        {
            int offset = 0;
            int splitCount = 0;

            Substring current;

            for (int i = 0; i < indexes.Length; i++)
            {
                current = s.Slice(offset, indexes[i] - offset);
                if (ProcessSplit(current, initialOffset, ref splits, options))
                {
                    splitCount++;
                }

                offset = indexes[i] + (lengths.IsEmpty ? len : lengths[i]);
                if (splitCount == count - 1)
                {
                    if ((options & StringSplitOptions.RemoveEmptyEntries) != 0)
                        while (++i < indexes.Length)
                        {
                            current = s.Slice(offset, indexes[i] - offset);
                            if (!current.IsEmpty) break;

                            offset = indexes[i] + (lengths.IsEmpty ? len : lengths[i]);
                        }

                    break;
                }
            }

            current = s.Slice(offset);
            ProcessSplit(current, initialOffset, ref splits, options);
        }

        private static void SplitInternal(
            Substring s,
            scoped ref StackList<(int Offset, int Count)> splits,
            scoped ReadOnlySpan<char> separators,
            int count,
            StringSplitOptions options)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, SR.ArgumentOutOfRange_NeedNonNegNum);
            }

            int initialOffset = s.StartIndex;

            if (count <= 1 || s.IsEmpty)
            {
                ProcessSplit(s, initialOffset, ref splits, options);
                return;
            }

            var indexes = new StackList<int>(stackalloc int[Math.Min(s.Length, 128)]);

            if (separators.IsEmpty)
            {
                StringUtility.FindWhitespaceChars(s.AsSpan(), ref indexes);
            }
            else
            {
                StringUtility.FindSeparatorChars(s.AsSpan(), separators, ref indexes);
            }

            var indexSpan = indexes.AsSpan();
            if (indexSpan.IsEmpty)
            {
                ProcessSplit(s, initialOffset, ref splits, options);
                return;
            }

            SplitFromKnownIndexes(s, initialOffset, ref splits, indexSpan, default, 1, count, options);

            indexes.Dispose();
        }

        private static void SplitInternal(
            Substring s,
            scoped ref StackList<(int Offset, int Count)> splits,
            string separator,
            scoped ReadOnlySpan<string> separators,
            int count,
            StringSplitOptions options)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, SR.ArgumentOutOfRange_NeedNonNegNum);
            }

            JPDebug.Assert(separator is not null || !separators.IsEmpty);

            int initialOffset = s.StartIndex;

            // This value is -1 if we're using multiple separators
            int sepLength = separator?.Length ?? -1;

            if (count <= 1 || s.IsEmpty || sepLength == 0)
            {
                ProcessSplit(s, initialOffset, ref splits, options);
                return;
            }

            int allocSize = Math.Min(s.Length, 128);

            var indexes = new StackList<int>(stackalloc int[allocSize]);
            var lengths = separator is null ? default : new StackList<int>(stackalloc int[allocSize]);

            if (separator is not null)
            {
                StringUtility.FindSeparatorString(s.AsSpan(), separator, ref indexes);
            }
            else
            {
                StringUtility.FindSeparatorStrings(s.AsSpan(), separators, ref indexes, ref lengths);
            }

            var indexSpan = indexes.AsSpan();
            if (indexSpan.IsEmpty)
            {
                ProcessSplit(s, initialOffset, ref splits, options);
                return;
            }

            SplitFromKnownIndexes(s, initialOffset, ref splits, indexSpan, lengths.AsSpan(), sepLength, count, options);

            indexes.Dispose();
            lengths.Dispose();
        }

        /// <summary>
        /// Split the string using the specified delimiting characters or strings.
        /// </summary>
        /// <param name="splits">
        /// A stack list which receives the offset and count of splits, relative to the current
        /// Substring instance. A new Substring can be constructed from this value via
        /// <c>new Substring(s, split.Offset, split.Count)</c>, where <c>s</c> is the current
        /// Substring and <c>split</c> is an element in the <paramref name="splits"/> list.
        /// </param>
        /// <param name="separator">The string or char that delimits the substrings in this instance.</param>
        /// <param name="separators">A span of strings or chars that delimits the substrings in this instance.</param>
        /// <param name="count">The maximum number of elements expected in the output.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
        private static void Split_DocumentationStub(object splits, object separator, object separators, object count)
        {
            /* I was unable to get 'inheritdoc' documentation working when the documentation existed
             * on one of the split methods below. I believe this is related to the tuple (int, int)
             * type used in the splits output parameter.
             * As such, I've put the documentation on this empty method as a workaround.
             */
        }

        /// <inheritdoc cref="Split_DocumentationStub"/>
        internal unsafe void Split(
            scoped ref StackList<(int Offset, int Count)> splits,
            char separator,
            int? count = null,
            StringSplitOptions options = StringSplitOptions.None)
        {
            void* ptr = &separator;
            SplitInternal(this, ref splits, new ReadOnlySpan<char>(ptr, 1), count ?? int.MaxValue, options);
        }

        /// <inheritdoc cref="Split_DocumentationStub"/>
        internal void Split(
            scoped ref StackList<(int Offset, int Count)> splits,
            scoped ReadOnlySpan<char> separators,
            int? count = null,
            StringSplitOptions options = StringSplitOptions.None)
        {
            SplitInternal(this, ref splits, separators, count ?? int.MaxValue, options);
        }

        /// <inheritdoc cref="Split_DocumentationStub"/>
        internal void Split(
            scoped ref StackList<(int Offset, int Count)> splits,
            string separator,
            int? count = null,
            StringSplitOptions options = StringSplitOptions.None)
        {
            int c = count ?? int.MaxValue;
            if (separator is null)
            {
                SplitInternal(this, ref splits, ReadOnlySpan<char>.Empty, c, options);
            }
            else
            {
                SplitInternal(this, ref splits, separator, ReadOnlySpan<string>.Empty, c, options);
            }
        }

        /// <inheritdoc cref="Split_DocumentationStub"/>
        internal void Split(
            scoped ref StackList<(int Offset, int Count)> splits,
            scoped ReadOnlySpan<string> separators,
            int? count = null,
            StringSplitOptions options = StringSplitOptions.None)
        {
            int c = count ?? int.MaxValue;
            if (separators.IsEmpty)
            {
                SplitInternal(this, ref splits, ReadOnlySpan<char>.Empty, c, options);
            }
            else if (separators.Length == 1)
            {
                var separator = separators[0];
                if (separator is null)
                {
                    SplitInternal(this, ref splits, ReadOnlySpan<char>.Empty, c, options);
                }
                else
                {
                    SplitInternal(this, ref splits, separator, ReadOnlySpan<string>.Empty, c, options);
                }
            }
            else
            {
                SplitInternal(this, ref splits, null, separators, c, options);
            }
        }

        private Substring[] ConsolidateSplitArray(scoped ref StackList<(int Offset, int Count)> splits)
        {
            var span = splits.AsSpan();
            if (span.IsEmpty) return Array.Empty<Substring>();

            var result = new Substring[span.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                (int Offset, int Count) = span[i];
                result[i] = new Substring(this, Offset, Count);
            }

            splits.Dispose();

            return result;
        }

        private void ConsolidateSplitList(scoped ref StackList<(int Offset, int Count)> splits, List<Substring> output)
        {
            var span = splits.AsSpan();
            foreach ((int Offset, int Count) in splits.AsSpan())
            {
                output.Add(new Substring(this, Offset, Count));
            }

            splits.Dispose();
        }

        /// <inheritdoc cref="Split_DocumentationStub"/>
        /// <returns>
        /// An array that contains substrings from this instance that are delimited by the specified separator(s).
        /// If <paramref name="count"/> is specified, the array will contain this number of elements at most.
        /// </returns>
        public Substring[] Split(char separator, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            var splits = new StackList<(int Offset, int Count)>(stackalloc (int, int)[Math.Min(this.Length, 128)]);
            Split(ref splits, separator, count, options);

            return ConsolidateSplitArray(ref splits);
        }

        /// <inheritdoc cref="Split(char, int?, StringSplitOptions)"/>
        public Substring[] Split(ReadOnlySpan<char> separators, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            var splits = new StackList<(int Offset, int Count)>(stackalloc (int, int)[Math.Min(this.Length, 128)]);
            Split(ref splits, separators, count, options);

            return ConsolidateSplitArray(ref splits);
        }

        /// <inheritdoc cref="Split(char, int?, StringSplitOptions)"/>
        public Substring[] Split(string separator, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            var splits = new StackList<(int Offset, int Count)>(stackalloc (int, int)[Math.Min(this.Length, 128)]);
            Split(ref splits, separator, count, options);

            return ConsolidateSplitArray(ref splits);
        }

        /// <inheritdoc cref="Split(char, int?, StringSplitOptions)"/>
        public Substring[] Split(ReadOnlySpan<string> separators, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            var splits = new StackList<(int Offset, int Count)>(stackalloc (int, int)[Math.Min(this.Length, 128)]);
            Split(ref splits, separators, count, options);

            return ConsolidateSplitArray(ref splits);
        }

        /// <inheritdoc cref="Split_DocumentationStub"/>
        /// <param name="output">
        /// Output list to receive substrings from this instance that are delimited by the specified separator(s).
        /// If <paramref name="count"/> is specified, the list will receive this number of elements at most.
        /// </param>
        public void Split(List<Substring> output, char separator, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            _ = output ?? throw new ArgumentNullException(nameof(output));

            var splits = new StackList<(int Offset, int Count)>(stackalloc (int, int)[Math.Min(this.Length, 128)]);
            Split(ref splits, separator, count, options);

            ConsolidateSplitList(ref splits, output);
        }

        /// <inheritdoc cref="Split(List{Substring}, char, int?, StringSplitOptions)"/>
        public void Split(List<Substring> output, ReadOnlySpan<char> separators, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            _ = output ?? throw new ArgumentNullException(nameof(output));

            var splits = new StackList<(int Offset, int Count)>(stackalloc (int, int)[Math.Min(this.Length, 128)]);
            Split(ref splits, separators, count, options);

            ConsolidateSplitList(ref splits, output);
        }

        /// <inheritdoc cref="Split(List{Substring}, char, int?, StringSplitOptions)"/>
        public void Split(List<Substring> output, string separator, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            _ = output ?? throw new ArgumentNullException(nameof(output));

            var splits = new StackList<(int Offset, int Count)>(stackalloc (int, int)[Math.Min(this.Length, 128)]);
            Split(ref splits, separator, count, options);

            ConsolidateSplitList(ref splits, output);
        }

        /// <inheritdoc cref="Split(List{Substring}, char, int?, StringSplitOptions)"/>
        public void Split(List<Substring> output, ReadOnlySpan<string> separators, int? count = null, StringSplitOptions options = StringSplitOptions.None)
        {
            _ = output ?? throw new ArgumentNullException(nameof(output));

            var splits = new StackList<(int Offset, int Count)>(stackalloc (int, int)[Math.Min(this.Length, 128)]);
            Split(ref splits, separators, count, options);

            ConsolidateSplitList(ref splits, output);
        }

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
