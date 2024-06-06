using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace JakePerry
{
    /// <summary>
    /// This struct wraps a <see cref="MemberInfo"/> of type <see cref="FieldInfo"/> or <see cref="PropertyInfo"/>.
    /// Provides a convenient interface for accessing particular features common to each type that are not exposed
    /// by their common parent class.
    /// </summary>
    public readonly struct ValueMemberInfo : IEquatable<ValueMemberInfo>
    {
        /// <exception cref="InvalidOperationException">
        /// Thrown if this instance does not represent a member (ie. it is a default struct instance).
        /// </exception>
        private static Exception GetNullMemberException() => new InvalidOperationException("Member is null.");

        /// <summary>
        /// The underlying member. This can be a <see cref="FieldInfo"/> or a <see cref="PropertyInfo"/>.
        /// </summary>
        public readonly MemberInfo member;

        /// <summary>
        /// Indicates the type that declares this member.
        /// </summary>
        public Type DeclaringType => member?.DeclaringType;

        /// <summary>
        /// Indicates whether the underlying <see cref="MemberInfo"/> is null.
        /// This is true for default instances of this struct.
        /// </summary>
        public bool IsNull => member is null;

        /// <summary>
        /// Indicates whether the member is public.
        /// </summary>
        public bool IsPublic
        {
            get
            {
                if (member is FieldInfo f)
                {
                    return f.IsPublic;
                }
                else if (member is PropertyInfo p)
                {
                    return p.GetMethod.IsPublic && (p.SetMethod?.IsPublic ?? true);
                }

                throw GetNullMemberException();
            }
        }

        /// <summary>
        /// Indicates whether the member is static.
        /// </summary>
        public bool IsStatic
        {
            get
            {
                if (member is FieldInfo f)
                {
                    return f.IsStatic;
                }
                else if (member is PropertyInfo p)
                {
                    return p.GetMethod.IsStatic && (p.SetMethod?.IsStatic ?? true);
                }

                throw GetNullMemberException();
            }
        }

        /// <summary>
        /// The return type of the underlying field or property.
        /// </summary>
        /// <inheritdoc cref="GetNullMemberException"/>
        public Type MemberType
        {
            get
            {
                if (member is FieldInfo field)
                {
                    return field.FieldType;
                }
                else if (member is PropertyInfo prop)
                {
                    return prop.PropertyType;
                }

                throw GetNullMemberException();
            }
        }

        public string Name => member.Name;

        public ValueMemberInfo(FieldInfo member) { this.member = member; }

        public ValueMemberInfo(PropertyInfo member) { this.member = member; }

        public static implicit operator ValueMemberInfo(FieldInfo member) => new ValueMemberInfo(member);

        public static implicit operator ValueMemberInfo(PropertyInfo member) => new ValueMemberInfo(member);

        public static implicit operator MemberInfo(ValueMemberInfo member) => member.member;

        /// <summary>
        /// Returns the value of the underlying field or property reflected by a given object.
        /// </summary>
        /// <param name="obj">The object whose field or property value will be returned.</param>
        /// <inheritdoc cref="GetNullMemberException"/>
        public object GetValue(object obj)
        {
            if (member is FieldInfo field)
            {
                return field.GetValue(obj);
            }
            else if (member is PropertyInfo prop)
            {
                return prop.GetValue(obj);
            }

            throw GetNullMemberException();
        }

        /// <summary>
        /// Sets the value of the underlying field or property supported by the given object.
        /// </summary>
        /// <param name="obj">The object whose field or property value will be set.</param>
        /// <param name="value">The value to assign to the field.</param>
        /// <inheritdoc cref="GetNullMemberException"/>
        public void SetValue(object obj, object value)
        {
            if (member is FieldInfo field)
            {
                field.SetValue(obj, value);
            }
            else if (member is PropertyInfo prop)
            {
                prop.SetValue(obj, value);
            }
            else
            {
                throw GetNullMemberException();
            }
        }

        /// <summary>
        /// Shorthand method which sets the member value if the previous value is considered
        /// inequal according to the given equality comparer.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the value was modified; otherwise, <see langword="false"/>.
        /// </returns>
        public bool SetValue(object obj, object value, IEqualityComparer comparer, out object previousValue)
        {
            _ = comparer ?? throw new ArgumentNullException(nameof(comparer));

            if (member is FieldInfo field)
            {
                previousValue = field.GetValue(obj);
                if (comparer.Equals(value, previousValue)) return false;

                field.SetValue(obj, value);
                return true;
            }
            else if (member is PropertyInfo prop)
            {
                previousValue = prop.GetValue(obj);
                if (comparer.Equals(value, previousValue)) return false;

                prop.SetValue(obj, value);
                return true;
            }

            throw GetNullMemberException();
        }

        public bool Equals(ValueMemberInfo other)
        {
            return EqualityComparer<MemberInfo>.Default.Equals(member, other.member);
        }

        public override bool Equals(object obj)
        {
            return obj is ValueMemberInfo other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return -679090373 + EqualityComparer<MemberInfo>.Default.GetHashCode(member);
        }

        public static bool operator ==(ValueMemberInfo left, ValueMemberInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ValueMemberInfo left, ValueMemberInfo right)
        {
            return !left.Equals(right);
        }

        public static ValueMemberInfo FromMemberInfo(MemberInfo member)
        {
            _ = member ?? throw new ArgumentNullException(nameof(member));

            if (member is FieldInfo field)
                return new ValueMemberInfo(field);

            if (member is PropertyInfo property)
                return new ValueMemberInfo(property);

            throw new ArgumentException("Member must be a field or property.", nameof(member));
        }

        /// <summary>
        /// Attempt to cast the given member to a <see cref="ValueMemberInfo"/>. This operation succeeds
        /// if <paramref name="member"/> is a <see cref="FieldInfo"/> or <see cref="PropertyInfo"/>;
        /// Otherwise, it fails.
        /// </summary>
        /// <param name="member">The member to cast.</param>
        /// <param name="valueMember">The casted object, if the operation succeeds.</param>
        /// <returns>
        /// <see langword="true"/> if the member is successfully cast; Otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool TryCast(MemberInfo member, out ValueMemberInfo valueMember)
        {
            _ = member ?? throw new ArgumentNullException(nameof(member));

            valueMember = member switch
            {
                FieldInfo field => new ValueMemberInfo(field),
                PropertyInfo property => new ValueMemberInfo(property),
                _ => default
            };

            return !valueMember.IsNull;
        }
    }
}
