using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace JakePerry
{
    public static class DisposeUtility
    {
        /// <summary>
        /// Convenience method for disposing multiple objects at once.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeAll(params IDisposable[] array)
        {
            if (array is not null)
                foreach (var o in array)
                {
                    o?.Dispose();
                }
        }

        /// <inheritdoc cref="DisposeAll(IDisposable[])"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeAll(IEnumerable<IDisposable> collection)
        {
            if (collection is not null)
                foreach (var o in collection)
                {
                    o?.Dispose();
                }
        }

        /// <inheritdoc cref="DisposeAll(IDisposable[])"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeAll<T0, T1>(T0 arg0, T1 arg1)
            where T0 : IDisposable
            where T1 : IDisposable
        {
            arg0?.Dispose();
            arg1?.Dispose();
        }

        /// <inheritdoc cref="DisposeAll(IDisposable[])"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeAll<T0, T1, T2>(T0 arg0, T1 arg1, T2 arg2)
            where T0 : IDisposable
            where T1 : IDisposable
            where T2 : IDisposable
        {
            arg0?.Dispose();
            arg1?.Dispose();
            arg2?.Dispose();
        }

        /// <summary>
        /// Dispose a non-generic <see cref="IEnumerator"/> object,
        /// if it implements the <see cref="IDisposable"/> interface.
        /// <para/>
        /// For more information, see the stackoverflow response
        /// <see href="https://stackoverflow.com/a/232616">here</see>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DisposeEnumerator<T>(T enumerator)
            where T : IEnumerator
        {
            if (enumerator is IDisposable d)
            {
                d.Dispose();
            }
        }

        /// <summary>
        /// Dispose the referenced instance <paramref name="o"/> if it is not <see langword="null"/>,
        /// then replaces it <paramref name="with"/> the second argument.
        /// </summary>
        /// <param name="o">The instance to be disposed and replaceed.</param>
        /// <param name="with">The new instance to replace the first with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Replace<T>(ref T o, T with)
            where T : IDisposable
        {
            o?.Dispose();
            o = with;
        }
    }
}
