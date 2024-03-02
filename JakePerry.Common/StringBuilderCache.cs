using System;
using System.Text;

namespace JakePerry
{
    /// <summary>
    /// Provides a cached reusable <see cref="StringBuilder"/> instance per thread.
    /// This is an optimization that should be used for small internal operations
    /// that only operate on small strings.
    /// </summary>
    /// <remarks>
    /// This class is a copy of the internal class by the same name used in mscorlib,
    /// with some small modifications.
    /// Source code for the internal mscorlib class can be found at
    /// https://referencesource.microsoft.com/#mscorlib/system/text/stringbuildercache.cs
    /// </remarks>
    public static class StringBuilderCache
    {
        /// <summary>
        /// Maximum capacity allowed for a StringBuilder to be cached. For more info
        /// see the comment in the original class, linked above.
        /// </summary>
        private const int kMaxBuilderSize = 360;

        /// <summary>
        /// Default capacity of a StringBuilder object - this field duplicates
        /// StringBuilder.DefaultCapacity which isn't publicly accessible.
        /// </summary>
        private const int kDefaultCapacity = 16;

        [ThreadStatic]
        private static StringBuilder _cachedInstance;

        /// <summary>
        /// Gets a cached StringBuilder with a minimum given capacity.
        /// A new StringBuilder object will be allocated in the following cases:
        /// <para>
        /// * <paramref name="capacity"/> is larger than the maximum allowed value.
        /// </para>
        /// <para>
        /// * The cache is empty (first call on a thread, or resulting from nested calls to this method).
        /// </para>
        /// <para>
        /// * The cached StringBuilder's capacity is less than <paramref name="capacity"/> (prevents fragmentation).
        /// </para>
        /// </summary>
        public static StringBuilder Acquire(int capacity = kDefaultCapacity)
        {
            if (capacity <= kMaxBuilderSize)
            {
                var sb = _cachedInstance;
                if (sb != null)
                {
                    // Avoid stringbuilder block fragmentation by getting a new StringBuilder
                    // when the requested size is larger than the current capacity
                    if (capacity <= sb.Capacity)
                    {
                        _cachedInstance = null;
                        sb.Clear();
                        return sb;
                    }
                }
            }

            return new StringBuilder(capacity);
        }

        /// <summary>
        /// Place the StringBuilder into the cache to be reused again if its capacity
        /// does not exceed the maximum allowed value.
        /// </summary>
        public static void Release(StringBuilder sb)
        {
            if (sb is null || sb.Capacity > kMaxBuilderSize) return;

            if (_cachedInstance is null || _cachedInstance.Capacity < sb.Capacity)
            {
                _cachedInstance = sb;
            }
        }

        /// <summary>
        /// Shorthand method to get the StringBuilder's ToString() value and release it
        /// in one line of code.
        /// </summary>
        /// <seealso cref="Release(StringBuilder)"/>
        public static string GetStringAndRelease(StringBuilder sb)
        {
            var result = sb?.ToString();
            Release(sb);

            return result;
        }
    }
}
