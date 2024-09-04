using System;
using System.Text;

namespace JakePerry
{
    public static class SubstringBuilderEx
    {
        public static StringBuilder Append(this StringBuilder stringBuilder, Substring substring)
        {
            _ = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
            return stringBuilder.Append(substring.AsSpan());
        }

        public static StringBuilder AppendLine(this StringBuilder stringBuilder, Substring substring)
        {
            _ = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
            return stringBuilder.Append(substring.AsSpan()).AppendLine();
        }

        public static StringBuilder Insert(this StringBuilder stringBuilder, int index, Substring substring)
        {
            _ = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
            return stringBuilder.Insert(index, substring.AsSpan());
        }
    }
}
