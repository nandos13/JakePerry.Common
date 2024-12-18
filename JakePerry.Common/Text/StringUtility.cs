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
        /// Find all instances of specified characters in the source string
        /// and populates a stack list with the indexes.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="chars">A span of characters to find.</param>
        /// <param name="indexes">Stack list to receive output indexes.</param>
        internal static void FindChars(
            scoped ReadOnlySpan<char> str,
            scoped ReadOnlySpan<char> chars,
            scoped ref StackList<int> indexes)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                foreach (var c2 in chars)
                    if (c == c2)
                    {
                        indexes.Append(i);
                        break;
                    }
            }
        }

        /// <summary>
        /// Find all instances of a specified substring in the source string
        /// and populates a stack list with the indexes.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="substring">A string to find.</param>
        /// <param name="indexes">Stack list to receive output indexes.</param>
        internal static void FindString(
            scoped ReadOnlySpan<char> str,
            scoped ReadOnlySpan<char> substring,
            scoped ref StackList<int> indexes)
        {
            int i = 0;
            while (!str.IsEmpty)
            {
                int index = str.IndexOf(substring);
                if (index < 0) break;

                i += index;
                indexes.Append(i);

                i += substring.Length;
                str = str.Slice(index + substring.Length);
            }
        }

        /// <summary>
        /// Find all instances of specified substrings in the source string
        /// and populates a stack list with the indexes and a corresponding
        /// stack list with the lengths of the strings that were found.
        /// </summary>
        /// <param name="str">The source string.</param>
        /// <param name="substrings">A span of strings to find.</param>
        /// <param name="indexes">Stack list to receive output indexes.</param>
        /// <param name="lengths">Stack list to receive output matching string lenghts.</param>
        internal static void FindStrings(
            scoped ReadOnlySpan<char> str,
            scoped ReadOnlySpan<string> substrings,
            scoped ref StackList<int> indexes,
            scoped ref StackList<int> lengths)
        {
            int remaining = str.Length;
            for (int i = 0; i < str.Length; ++i, --remaining)
            {
                char c = str[i];

                for (int j = 0; j < substrings.Length; ++j)
                {
                    string s = substrings[j];

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
