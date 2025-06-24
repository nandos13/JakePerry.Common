using System.Runtime.CompilerServices;

namespace JakePerry
{
    public static class TryGet
    {
        /// <summary>
        /// Helper for failing some TryGetX method. Assigns the <see langword="default"/> value
        /// - or the <paramref name="defaultValue"/> specified by the user - to the
        /// <see langword="out"/> parameter, then returns <see langword="false"/>.
        /// <para/>
        /// <b>Usage:</b>
        /// <code>
        /// public bool TryGetSomething(out object o)
        /// {
        ///     // ...
        ///     
        ///     return TryGet.Fail(out o);
        /// }
        /// </code>
        /// </summary>
        /// <param name="defaultValue">
        /// The value to be assigned to the <see langword="out"/> parameter.
        /// Usually this does not need to be specified, but in some cases it may be preferable
        /// to default to another value, ie. -1 instead of 0 for <see cref="int"/> parameters.
        /// </param>
        /// <returns>
        /// Always returns <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Fail<T>(out T o, T defaultValue = default)
        {
            o = defaultValue;
            return false;
        }

        /// <summary>
        /// Helper for succeeding some TryGetX method. Assigns the given <paramref name="value"/>
        /// to the <see langword="out"/> parameter <paramref name="recipient"/>,
        /// then returns <see langword="true"/>.
        /// <para/>
        /// <b>Usage:</b>
        /// <code>
        /// public bool TryGetSomething(out object o)
        /// {
        ///     if (...)
        ///     {
        ///         return TryGet.Pass(out o, "result");
        ///     }
        ///     
        ///     // ...
        /// }
        /// </code>
        /// </summary>
        /// <returns>
        /// Always returns <see langword="true"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Pass<T>(out T recipient, T value)
        {
            recipient = value;
            return true;
        }
    }
}
