using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// Represents a section of a <see cref="string"/>.
    /// <para/>
    /// Most use cases would be better off using the <see cref="MemoryExtensions.AsSpan(string)"/> method,
    /// however <see cref="ReadOnlySpan{T}"/> is a ref struct which imposes some limitations.
    /// This type is intended for use when said limitations are incompatible with desired code design.
    /// </summary>
    public readonly partial struct Substring : IComparable<string>, IComparable<Substring>, IEquatable<string>, IEquatable<Substring>
    {
        private readonly string m_value;
        private readonly int m_start;
        private readonly int m_length;

        /// <summary>
        /// Indicates whether the substring is empty.
        /// </summary>
        public bool IsEmpty => m_length == 0;

        /// <summary>
        /// The length of this substring.
        /// </summary>
        public int Length => m_length;

        /// <summary>
        /// The <see cref="string"/> which this is a substring of.
        /// </summary>
        public string SourceString => m_value;

        /// <summary>
        /// Index of <see cref="SourceString"/> at which this substring starts.
        /// </summary>
        public int StartIndex => m_start;

        public static Substring Empty => new(string.Empty, 0, 0);

        public Substring(string value, int startIndex, int length)
        {
            Enforce.Argument(value, nameof(value)).IsNotNull();

            m_value = value;

            if (startIndex < 0 || startIndex > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, null);
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, null);
            }

            if (startIndex + length > value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, null);
            }

            if (length == 0)
            {
                startIndex = 0;
                m_value = string.Empty;
            }

            m_start = startIndex;
            m_length = length;
        }

        public Substring(string value, int startIndex)
        {
            Enforce.Argument(value, nameof(value)).IsNotNull();

            this = new Substring(value, startIndex, value.Length - startIndex);
        }

        public Substring(string value) : this(value, 0) { }

        public Substring(string value, Index startIndex)
        {
            Enforce.Argument(value, nameof(value)).IsNotNull();

            int start = startIndex.GetOffset(value.Length);

            this = new Substring(value, start);
        }

        public Substring(string value, Range range)
        {
            Enforce.Argument(value, nameof(value)).IsNotNull();

            (int start, int length) = range.GetOffsetAndLength(value.Length);

            this = new Substring(value, start, length);
        }

        public Substring(Substring other, int startIndex, int length)
        {
            int len = other.m_length;

            if (startIndex < 0 || startIndex > len)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, null);
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, null);
            }

            if (startIndex + length > len)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, null);
            }

            startIndex += other.m_start;
            this = new Substring(other.m_value, startIndex, length);
        }

        public Substring(Substring other, int startIndex)
        {
            int length = other.m_length;

            if (startIndex < 0 || startIndex > length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, null);
            }

            startIndex += other.m_start;
            length -= startIndex;

            this = new Substring(other.m_value, startIndex, length);
        }

        public Substring(Substring other, Index startIndex)
        {
            int start = startIndex.GetOffset(other.Length);

            this = new Substring(other, start);
        }

        public Substring(Substring other, Range range)
        {
            (int start, int length) = range.GetOffsetAndLength(other.Length);

            this = new Substring(other, start, length);
        }

        /// <returns>
        /// The character at the given index in the substring.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public char this[int index]
        {
            get
            {
                if (index < 0 || index >= m_length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
                }

                return m_value[index + m_start];
            }
        }

        public Substring Slice(int start)
        {
            return new Substring(this, start);
        }

        public Substring Slice(int start, int length)
        {
            return new Substring(this, start, length);
        }

        /// <summary>
        /// Creates a new read-only span over a string.
        /// </summary>
        public ReadOnlySpan<char> AsSpan()
        {
            return m_value is null ? ReadOnlySpan<char>.Empty : m_value.AsSpan(m_start, m_length);
        }

        /// <inheritdoc cref="AsSpan()"/>
        /// <param name="start">
        /// The index at which to begin this slice.
        /// </param>
        public ReadOnlySpan<char> AsSpan(int start)
        {
            return Slice(start).AsSpan();
        }

        /// <inheritdoc cref="AsSpan(int)"/>
        /// <param name="length">
        /// The desired length for the slice.
        /// </param>
        public ReadOnlySpan<char> AsSpan(int start, int length)
        {
            return Slice(start, length).AsSpan();
        }

        /// <summary>
        /// Get a ReadOnlyMemory representation of the substring.
        /// </summary>
        public ReadOnlyMemory<char> AsMemory()
        {
            return m_value is null ? ReadOnlyMemory<char>.Empty : m_value.AsMemory(m_start, m_length);
        }

        /// <inheritdoc cref="AsMemory()"/>
        /// <param name="start">
        /// The index at which to begin this slice.
        /// </param>
        public ReadOnlyMemory<char> AsMemory(int start)
        {
            return Slice(start).AsMemory();
        }

        /// <inheritdoc cref="AsMemory(int)"/>
        /// <param name="length">
        /// The desired length for the slice.
        /// </param>
        public ReadOnlyMemory<char> AsMemory(int start, int length)
        {
            return Slice(start, length).AsMemory();
        }

        /// <summary>
        /// Get a <see cref="string"/> representation of the substring.
        /// </summary>
        /// <remarks>
        /// Note: This is an allocating call.
        /// </remarks>
        public string CopyString()
        {
            if (m_value is null) return null;
            if (m_start == 0 && m_length == m_value.Length) return m_value;

            return m_value.Substring(m_start, m_length);
        }

        public int CompareTo(string other, int start, int length, StringComparison comparisonType)
        {
            if (m_value is null) return other is null ? 0 : -1;
            if (other is null) return 1;

            return string.Compare(m_value, m_start, other, start, length, comparisonType);
        }

        public int CompareTo(string other, StringComparison comparisonType)
        {
            if (other is null) return m_value is null ? 0 : 1;
            return CompareTo(other, 0, other.Length, comparisonType);
        }

        public int CompareTo(string other)
        {
            return CompareTo(other, StringComparison.Ordinal);
        }

        public int CompareTo(Substring other, StringComparison comparisonType)
        {
            if (other.m_value is null) return m_value is null ? 0 : 1;
            return CompareTo(other.m_value, other.m_start, other.m_length, comparisonType);
        }

        public int CompareTo(Substring other)
        {
            return CompareTo(other, StringComparison.Ordinal);
        }

        public bool Equals(string other, int start, int length, StringComparison comparisonType)
        {
            if (m_value is null) return other is null;
            if (other is null) return false;

            if (m_length != length) return false;

            if (ReferenceEquals(m_value, other) && m_start == start)
            {
                return true;
            }

            return string.Compare(m_value, m_start, other, start, length, comparisonType) == 0;
        }

        public bool Equals(string other)
        {
            if (other is null) return m_value is null;
            return Equals(other, 0, other.Length, StringComparison.Ordinal);
        }

        public bool Equals(string other, StringComparison comparisonType)
        {
            if (other is null) return m_value is null;
            return Equals(other, 0, other.Length, comparisonType);
        }

        public bool Equals(Substring other)
        {
            return Equals(other.m_value, other.m_start, other.m_length, StringComparison.Ordinal);
        }

        public bool Equals(Substring other, StringComparison comparisonType)
        {
            return Equals(other.m_value, other.m_start, other.m_length, comparisonType);
        }

        public override bool Equals(object obj)
        {
            if (obj is string s) return this.Equals(s);
            return obj is Substring other && this.Equals(other);
        }

        public unsafe override int GetHashCode()
        {
            if (m_value is null) return StringComparer.Ordinal.GetHashCode((string)null);
            if (m_length == 0) return StringComparer.Ordinal.GetHashCode(string.Empty);

            HashCode hash = new();
            fixed (char* ptr = m_value)
            {
                int i = m_start;
                int end = m_start + m_length;
                while (i + 4 < end)
                {
                    hash.Add(m_value[i]);
                    hash.Add(m_value[i + 1]);
                    hash.Add(m_value[i + 2]);
                    hash.Add(m_value[i + 3]);
                    i += 4;
                }
                for (; i < end; ++i)
                {
                    hash.Add(m_value[i]);
                }
            }

            return hash.ToHashCode();
        }

        public Substring GetSubstring(int startIndex)
        {
            return new Substring(this, startIndex);
        }

        public Substring GetSubstring(int startIndex, int length)
        {
            return new Substring(this, startIndex, length);
        }

        /// <summary>
        /// Determines if the substring ends with the given string or character.
        /// </summary>
        public bool EndsWith(char c)
        {
            return m_value is not null && m_value[m_start + m_length - 1] == c;
        }

        /// <inheritdoc cref="EndsWith(char)"/>
        /// <param name="comparisonType">
        /// Determines how values are compared.
        /// </param>
        public bool EndsWith(string value, StringComparison comparisonType)
        {
            if (value is null) return false;

            int len = value.Length;
            if (len > m_length) return false;

            if (len == m_length)
            {
                return this.Equals(value, 0, len, comparisonType);
            }

            return new Substring(m_value, m_start + m_length - len, len).Equals(value, 0, len, comparisonType);
        }

        /// <inheritdoc cref="EndsWith(char)"/>
        public bool EndsWith(string value)
        {
            return EndsWith(value, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Determines if the substring starts with the given string or character.
        /// </summary>
        public bool StartsWith(char c)
        {
            return m_value is not null && m_value[m_start] == c;
        }

        /// <inheritdoc cref="StartsWith(char)"/>
        /// <param name="comparisonType">
        /// Determines how values are compared.
        /// </param>
        public bool StartsWith(string value, StringComparison comparisonType)
        {
            if (value is null) return false;

            int len = value.Length;
            if (len > m_length) return false;

            if (len == m_length)
            {
                return this.Equals(value, 0, len, comparisonType);
            }

            return new Substring(m_value, m_start, len).Equals(value, 0, len, comparisonType);
        }

        /// <inheritdoc cref="StartsWith(char)"/>
        public bool StartsWith(string value)
        {
            return StartsWith(value, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// Determines if the substring starts with the given span of characters, using the given comparison type.
        /// </summary>
        public bool StartsWith(ReadOnlySpan<char> value, StringComparison comparisonType)
        {
            return AsSpan().StartsWith(value, comparisonType);
        }

        public override string ToString()
        {
            return CopyString();
        }

        public static bool operator ==(Substring x, Substring y) => x.Equals(y);

        public static bool operator !=(Substring x, Substring y) => !(x == y);

        public static bool operator ==(Substring x, string y) => x.Equals(y);

        public static bool operator !=(Substring x, string y) => !(x == y);

        public static bool operator ==(string x, Substring y) => y.Equals(x);

        public static bool operator !=(string x, Substring y) => !(x == y);

        public static implicit operator Substring(string str)
        {
            return new Substring(str, 0);
        }

        public static explicit operator string(Substring substr)
        {
            return substr.CopyString();
        }
    }
}
