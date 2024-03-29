using System;
using System.Collections.Generic;

namespace JakePerry
{
    public static class ListEx
    {
        /// <summary>
        /// Helper class that captures a predicate delegate and state object
        /// for use with the <see cref="List{T}.RemoveAll(Predicate{T})"/> method.
        /// </summary>
        private sealed class RemoveAllCapture<T, S>
        {
            private readonly Func<T, S, bool> m_match;
            private readonly S m_state;

            public RemoveAllCapture(Func<T, S, bool> match, S state)
            {
                m_match = match;
                m_state = state;
            }

            public bool Invoke(T arg)
            {
                return m_match.Invoke(arg, m_state);
            }
        }

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="source">The list to operate on.</param>
        /// <param name="match">The predicate delegate that defines the conditions of the elements to remove, based on a given state object.</param>
        /// <param name="state">The state object to be passed to the predicate.</param>
        public static int RemoveAll<T, S>(this List<T> source, Func<T, S, bool> match, S state)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = match ?? throw new ArgumentNullException(nameof(match));

            // Allocate an object to capture our parameters
            var capture = new RemoveAllCapture<T, S>(match, state);

            // Allocate a Predicate delegate using the capture's Invoke method
            var predicate = new Predicate<T>(capture.Invoke);

            return source.RemoveAll(predicate);
        }

        /// <summary>
        /// Removes the first element that matches the condition defined by the specified predicate.
        /// </summary>
        /// <param name="source">The list to operate on.</param>
        /// <param name="match">The predicate delegate that defines the conditions of the element to remove.</param>
        /// <returns>True if an element was removed; otherwise, false.</returns>
        public static bool RemoveFirst<T>(this List<T> source, Predicate<T> match)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = match ?? throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < source.Count; i++)
                if (match.Invoke(source[i]))
                {
                    source.RemoveAt(i);
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Removes the first element that matches the condition defined by the specified predicate.
        /// </summary>
        /// <param name="source">The list to operate on.</param>
        /// <param name="match">The predicate delegate that defines the conditions of the element to remove, based on a given state object.</param>
        /// <param name="state">The state object to be passed to the predicate.</param>
        /// <returns>True if an element was removed; otherwise, false.</returns>
        public static bool RemoveFirst<T, S>(this List<T> source, Func<T, S, bool> match, S state)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = match ?? throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < source.Count; i++)
                if (match.Invoke(source[i], state))
                {
                    source.RemoveAt(i);
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Removes the last element that matches the condition defined by the specified predicate.
        /// </summary>
        /// <param name="source">The list to operate on.</param>
        /// <param name="match">The predicate delegate that defines the conditions of the element to remove.</param>
        /// <returns>True if an element was removed; otherwise, false.</returns>
        public static bool RemoveLast<T>(this List<T> source, Predicate<T> match)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = match ?? throw new ArgumentNullException(nameof(match));

            for (int i = source.Count - 1; i >= 0; i--)
                if (match.Invoke(source[i]))
                {
                    source.RemoveAt(i);
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Removes the last element that matches the condition defined by the specified predicate.
        /// </summary>
        /// <param name="source">The list to operate on.</param>
        /// <param name="match">The predicate delegate that defines the conditions of the element to remove, based on a given state object.</param>
        /// <param name="state">The state object to be passed to the predicate.</param>
        /// <returns>True if an element was removed; otherwise, false.</returns>
        public static bool RemoveLast<T, S>(this List<T> source, Func<T, S, bool> match, S state)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = match ?? throw new ArgumentNullException(nameof(match));

            for (int i = source.Count - 1; i >= 0; i--)
                if (match.Invoke(source[i], state))
                {
                    source.RemoveAt(i);
                    return true;
                }

            return false;
        }
    }
}
