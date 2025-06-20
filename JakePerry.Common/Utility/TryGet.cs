using System.Runtime.CompilerServices;

namespace JakePerry
{
    public static class TryGet
    {
        /// <summary>
        /// Helper for failing some TryGetX method. Assigns the <see langword="default"/> value
        /// to the <see langword="out"/> parameter, then returns <see langword="false"/>.
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
        /// <returns>
        /// Always returns <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Fail<T>(out T o)
        {
            o = default;
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
