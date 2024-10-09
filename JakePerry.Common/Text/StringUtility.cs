using JakePerry.Collections;
using System;

namespace JakePerry.Text
{
    internal static class StringUtility
    {
        /// <summary>
        /// Find all whitespace characters in the source string and populates a stack list
        /// with their indexes.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="indexes">Stack list to receive output indexes.</param>
        internal static void FindWhitespaceChars(
            scoped ReadOnlySpan<char> str,
            scoped ref StackList<int> indexes)
        {
            for (int i = 0; i < str.Length; ++i)
                if (char.IsWhiteSpace(str[i]))
                {
                    indexes.Append(i);
                }
        }

        /// <summary>
        /// Find all instances of specified separator characters in the source string
        /// and populates a stack list with the separator indexes.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="separators">A span of separator characters.</param>
        /// <param name="indexes">Stack list to receive output indexes.</param>
        internal static void FindSeparatorChars(
            scoped ReadOnlySpan<char> str,
            scoped ReadOnlySpan<char> separators,
            scoped ref StackList<int> indexes)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                foreach (var c2 in separators)
                    if (c == c2)
                    {
                        indexes.Append(i);
                        break;
                    }
            }
        }

        /// <summary>
        /// Find all instances of a specified separator string in the source string
        /// and populates a stack list with the separator indexes.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="separator">A separator string.</param>
        /// <param name="indexes">Stack list to receive output indexes.</param>
        internal static void FindSeparatorString(
            scoped ReadOnlySpan<char> str,
            scoped ReadOnlySpan<char> separator,
            scoped ref StackList<int> indexes)
        {
            int i = 0;
            while (!str.IsEmpty)
            {
                int index = str.IndexOf(separator);
                if (index < 0) break;

                i += index;
                indexes.Append(i);

                i += separator.Length;
                str = str.Slice(index + separator.Length);
            }
        }

        /// <summary>
        /// Find all instances of specified separator strings in the source string
        /// and populates a stack list with the separator indexes and a corresponding
        /// stack list with the lengths of the separators that were found.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="separators">A span of separator characters.</param>
        /// <param name="indexes">Stack list to receive output indexes.</param>
        /// <param name="lengths">Stack list to receive output separator lenghts.</param>
        internal static void FindSeparatorStrings(
            scoped ReadOnlySpan<char> str,
            scoped ReadOnlySpan<string> separators,
            scoped ref StackList<int> indexes,
            scoped ref StackList<int> lengths)
        {
            int remaining = str.Length;
            for (int i = 0; i < str.Length; ++i, --remaining)
            {
                char c = str[i];

                for (int j = 0; j < separators.Length; ++j)
                {
                    string s = separators[j];

                    int sepLength = s?.Length ?? 0;
                    if (sepLength == 0 || sepLength > remaining) continue;

                    if (c == s[0])
                    {
                        if (sepLength == 1 || str.Slice(i, sepLength).SequenceEqual(s))
                        {
                            indexes.Append(i);
                            lengths.Append(sepLength);
                            i += sepLength - 1;
                            break;
                        }
                    }
                }
            }
        }
    }
}
