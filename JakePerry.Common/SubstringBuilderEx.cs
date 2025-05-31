using System.Text;

namespace JakePerry
{
    public static class SubstringBuilderEx
    {
        public static StringBuilder Append(this StringBuilder stringBuilder, Substring substring)
        {
            Enforce.Argument(stringBuilder, nameof(stringBuilder)).IsNotNull();

            return stringBuilder.Append(substring.AsSpan());
        }

        public static StringBuilder AppendLine(this StringBuilder stringBuilder, Substring substring)
        {
            Enforce.Argument(stringBuilder, nameof(stringBuilder)).IsNotNull();

            return stringBuilder.Append(substring.AsSpan()).AppendLine();
        }

        public static StringBuilder Insert(this StringBuilder stringBuilder, int index, Substring substring)
        {
            Enforce.Argument(stringBuilder, nameof(stringBuilder)).IsNotNull();

            return stringBuilder.Insert(index, substring.AsSpan());
        }
    }
}
