using System;
using System.Runtime.InteropServices;

namespace JakePerry
{
    /// <summary>
    /// Packs a <see cref="Guid"/> into two <see cref="ulong"/> values.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct PackedGuid :
        IComparable,
        IComparable<Guid>,
        IComparable<PackedGuid>,
        IEquatable<Guid>,
        IEquatable<PackedGuid>,
        IFormattable,
        IStruct,
        IStructWithDefaultCheck
    {
        [FieldOffset(0)]
        [NonSerialized]
        private Guid m_guid;

        /// <summary>
        /// First 8-byte segment of the Guid.
        /// </summary>
        [FieldOffset(0)]
        public ulong a;

        /// <summary>
        /// Second 8-byte segment of the Guid.
        /// </summary>
        [FieldOffset(8)]
        public ulong b;

        public readonly bool IsDefaultValue => a == 0UL && b == 0UL;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedGuid"/> structure.
        /// </summary>
        /// <param name="guid">
        /// The <see cref="Guid"/> to encapsulate.
        /// </param>
        public PackedGuid(Guid guid)
        {
            a = b = 0;
            m_guid = guid;
        }

        /// <summary>
        /// <inheritdoc cref="PackedGuid(Guid)" path="/summary"/>
        /// </summary>
        /// <param name="guid">
        /// <inheritdoc cref="Guid(string)" path="/param[@name='g']"/>
        /// </param>
        public PackedGuid(string guid) : this(new Guid(guid)) { }

        /// <summary>
        /// <inheritdoc cref="PackedGuid(Guid)" path="/summary"/>
        /// </summary>
        /// <param name="a">
        /// The first 8 bytes of the Guid.
        /// </param>
        /// <param name="b">
        /// The second 8 bytes of the Guid.
        /// </param>
        public PackedGuid(ulong a, ulong b)
        {
            m_guid = default;

            this.a = a;
            this.b = b;
        }

        public readonly int CompareTo(Guid other)
        {
            return m_guid.CompareTo(other);
        }

        public readonly int CompareTo(PackedGuid other)
        {
            return m_guid.CompareTo(other.m_guid);
        }

        public readonly int CompareTo(object obj)
        {
            if (obj is Guid other1)
                return this.CompareTo(other1);

            if (obj is PackedGuid other2)
                return this.CompareTo(other2);

            return -1;
        }

        public readonly bool Equals(Guid other)
        {
            return m_guid.Equals(other);
        }

        public readonly bool Equals(PackedGuid other)
        {
            return m_guid.Equals(other.m_guid);
        }

        public readonly override bool Equals(object obj)
        {
            if (obj is Guid other1)
                return this.Equals(other1);

            if (obj is PackedGuid other2)
                return this.Equals(other2);

            return false;
        }

        public readonly override int GetHashCode()
        {
            return m_guid.GetHashCode();
        }

        public readonly Guid ToGuid()
        {
            return m_guid;
        }

        public readonly override string ToString()
        {
            return m_guid.ToString("N");
        }

        public readonly string ToString(string format)
        {
            return m_guid.ToString(format);
        }

        public readonly string ToString(string format, IFormatProvider provider)
        {
            return m_guid.ToString(format, provider);
        }

        public static PackedGuid NewGuid()
        {
            return new PackedGuid(Guid.NewGuid());
        }

        public static implicit operator Guid(PackedGuid guid)
        {
            return guid.m_guid;
        }

        public static implicit operator PackedGuid(Guid guid)
        {
            return new PackedGuid(guid);
        }

        public static explicit operator string(PackedGuid guid)
        {
            return guid.ToString();
        }

        public static bool operator ==(PackedGuid left, PackedGuid right) => left.Equals(right);
        public static bool operator !=(PackedGuid left, PackedGuid right) => !(left == right);

        public static bool operator <(PackedGuid left, PackedGuid right) => left.CompareTo(right) < 0;
        public static bool operator <=(PackedGuid left, PackedGuid right) => left.CompareTo(right) <= 0;
        public static bool operator >(PackedGuid left, PackedGuid right) => left.CompareTo(right) > 0;
        public static bool operator >=(PackedGuid left, PackedGuid right) => left.CompareTo(right) >= 0;
    }
}
