using System;

namespace JakePerry
{
    /// <summary>
    /// Represents a section of a <see cref="string"/>.
    /// </summary>
    public readonly struct Substring : IComparable<string>, IComparable<Substring>, IEquatable<string>, IEquatable<Substring>
    {
        public readonly string value;
        public readonly int start;
        public readonly int length;

        public Substring(string value, int startIndex, int length)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));

            if (startIndex < 0 || startIndex >= value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (startIndex + length > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            this.start = startIndex;
            this.length = length;
        }

        public Substring(string value, int startIndex)
        {
            this.value = value ?? throw new ArgumentNullException(nameof(value));

            this = new Substring(value, startIndex, value.Length - startIndex);
        }

        public char this[int index]
        {
            get
            {
                if (index < 0 || index >= length)
                {
                    throw new ArgumentNullException(nameof(index));
                }

                return value[index + start];
            }
        }

        public ReadOnlySpan<char> AsSpan() => value.AsSpan(start, length);

        public ReadOnlyMemory<char> AsMemory() => value.AsMemory(start, length);

        /// <summary>
        /// Get a <see cref="string"/> representation of the substring.
        /// </summary>
        /// <remarks>
        /// Note: This is an allocating call.
        /// </remarks>
        public string CopyString()
        {
            if (value is null) return null;
            if (start == 0 && length == value.Length) return value;

            return value.Substring(start, length);
        }

        public int CompareTo(string other, int start, int length, StringComparison comparisonType)
        {
            if (value is null) return other is null ? 0 : -1;
            if (other is null) return 1;

            return string.Compare(value, this.start, other, start, length, comparisonType);
        }

        public int CompareTo(string other, StringComparison comparisonType)
        {
            if (other is null) return value is null ? 0 : 1;
            return CompareTo(other, 0, other.Length, comparisonType);
        }

        public int CompareTo(string other)
        {
            return CompareTo(other, StringComparison.Ordinal);
        }

        public int CompareTo(Substring other, StringComparison comparisonType)
        {
            if (other.value is null) return value is null ? 0 : 1;
            return CompareTo(other.value, other.start, other.length, comparisonType);
        }

        public int CompareTo(Substring other)
        {
            return CompareTo(other, StringComparison.Ordinal);
        }

        public bool Equals(string other, int start, int length, StringComparison comparisonType)
        {
            if (value is null) return other is null;
            if (other is null) return false;

            if (this.length != length) return false;

            if (ReferenceEquals(value, other) && this.start == start)
            {
                return true;
            }

            return string.Compare(value, this.start, other, start, length, comparisonType) != 0;
        }

        public bool Equals(string other)
        {
            if (other is null) return value is null;
            return Equals(other, 0, other.Length, StringComparison.Ordinal);
        }

        public bool Equals(string other, StringComparison comparisonType)
        {
            if (other is null) return value is null;
            return Equals(other, 0, other.Length, comparisonType);
        }

        public bool Equals(Substring other)
        {
            return Equals(other.value, other.start, other.length, StringComparison.Ordinal);
        }

        public bool Equals(Substring other, StringComparison comparisonType)
        {
            return Equals(other.value, other.start, other.length, comparisonType);
        }

        public override bool Equals(object obj)
        {
            if (obj is string s) return this.Equals(s);
            return obj is Substring other && this.Equals(other);
        }

        public unsafe override int GetHashCode()
        {
            if (value is null) return StringComparer.Ordinal.GetHashCode((string)null);
            if (length == 0) return StringComparer.Ordinal.GetHashCode(string.Empty);

            //int hash = 5381;
            var hash = new HashCode();
            fixed (char* ptr = value)
            {
                int i = start;
                int end = start + length;
                while (i + 4 < end)
                {
                    hash.Add(value[i]);
                    hash.Add(value[i + 1]);
                    hash.Add(value[i + 2]);
                    hash.Add(value[i + 3]);
                    i += 4;
                }
                for (; i < end; ++i)
                {
                    hash.Add(value[i]);
                }
            }

            return hash.ToHashCode();
        }

        public static bool operator ==(Substring x, Substring y) => x.Equals(y);

        public static bool operator !=(Substring x, Substring y) => !(x == y);

        public static bool operator ==(Substring x, string y) => x.Equals(y);

        public static bool operator !=(Substring x, string y) => !(x == y);

        public static bool operator ==(string x, Substring y) => y.Equals(x);

        public static bool operator !=(string x, Substring y) => !(x == y);
    }
}
