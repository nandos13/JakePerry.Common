using System.Runtime.CompilerServices;

namespace JakePerry.PySharp
{
    internal static partial class BuiltIns
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Int32RangeSequence range(int stop)
        {
            return new Int32RangeSequence(stop);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Int32RangeSequence range(int start, int stop)
        {
            return new Int32RangeSequence(start, stop);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Int32RangeSequence range(int start, int stop, int step)
        {
            return new Int32RangeSequence(start, stop, step);
        }
    }
}
