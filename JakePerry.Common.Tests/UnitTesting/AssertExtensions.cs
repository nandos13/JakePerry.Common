using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace JakePerry.UnitTesting
{
    public static class AssertExtensions
    {
        private static void BuildSequenceString<T>(StringBuilder sb, IEnumerable<T>? sequence)
        {
            if (sequence is null)
            {
                sb.Append("null");
                return;
            }

            sb.Append("Count:");
            int countInsert = sb.Length;

            sb.Append("|Sequence:");

            int count = 0;
            bool flag = false;
            foreach (var o in sequence)
            {
                if (flag) sb.Append(',');
                else flag = true;

                sb.Append(UnitTestingUtility.StringifyArgument(o));

                ++count;
            }

            sb.Insert(countInsert, count);
        }

        private static void FailSequenceEqual<T>(IEnumerable<T>? expected, IEnumerable<T>? actual)
        {
            var sb = new StringBuilder();

            sb.Append("Assert.SequenceEqual failed. Expected:<");
            BuildSequenceString(sb, expected);
            sb.Append(">. Actual:<");
            BuildSequenceString(sb, actual);
            sb.Append('>');

            Assert.Fail(sb.ToString());
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
        public static void SequenceEqual<T>(this Assert assert, IEnumerable<T> expected, IEnumerable<T> actual, IEqualityComparer<T>? comparer)
        {
            if (expected is null)
            {
                // Both sequences are null, so they are considered equal
                if (actual is null) return;
            }
            else if (actual is not null && expected.SequenceEqual(actual, comparer))
            {
                // Both sequences are non-null and are equal
                return;
            }

            FailSequenceEqual(expected, actual);
        }

        public static void SequenceEqual<T>(this Assert assert, IEnumerable<T> expected, IEnumerable<T> actual)
        {
            SequenceEqual(assert, expected, actual, null);
        }
    }
}
