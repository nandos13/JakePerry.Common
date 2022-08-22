namespace JakePerry
{
    public static class MathEx
    {
        /// <summary>
        /// Custom modulo operation that ensures a positive value is returned.
        /// </summary>
        public static int PosMod(int x, int m)
        {
            x %= m;
            return x < 0 ? x + m : x;
        }

        /// <inheritdoc cref="PosMod(int, int)"/>
        public static void PosMod(ref int x, int m)
        {
            x = PosMod(x, m);
        }
    }
}
