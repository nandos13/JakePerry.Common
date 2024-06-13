using System;
using System.Collections.Generic;
using System.Reflection;

namespace JakePerry
{
    /// <summary>
    /// A collection of helpful methods related to Reflection.
    /// </summary>
    public static class ReflectionEx
    {
        /// <summary>
        /// Unique key for caching type lookup results.
        /// </summary>
        private readonly struct TypeKey
        {
            public readonly Assembly assembly;
            public readonly string typeName;

            public TypeKey(Assembly a, string n) { assembly = a; typeName = n; }
        }

        /// <summary>
        /// Unique key for caching field &amp; property lookups.
        /// </summary>
        private readonly struct FieldPropertyKey
        {
            public readonly Type type;
            public readonly string name;

            public FieldPropertyKey(Type t, string n) { type = t; name = n; }
        }

        /// <summary>
        /// Unique key for caching method lookups.
        /// </summary>
        private readonly struct MethodKey
        {
            public readonly Type type;
            public readonly string methodName;
            public readonly ParamsArray<Type> types;

            public MethodKey(
                Type type,
                string methodName,
                ParamsArray<Type> types = default)
            {
                this.type = type;
                this.methodName = methodName;
                this.types = types;
            }
        }

        /// <summary>
        /// An array of rental buffers, where each buffer in the array contains
        /// instances of <see cref="object"/>[] with a length equal to one more than
        /// the index of the buffer in the array.
        /// <para/>
        /// ie. <c>Print(_rentalArrays[3].Pop().Length)</c> prints 4.
        /// </summary>
        private static readonly Stack<object[]>[] _rentalArrays;

        private static readonly Dictionary<TypeKey, Type> _typeLookup = new();
        private static readonly Dictionary<FieldPropertyKey, FieldInfo> _fieldLookup = new();
        private static readonly Dictionary<FieldPropertyKey, PropertyInfo> _propertyLookup = new();
        private static readonly Dictionary<MethodKey, MethodInfo> _methodLookup = new();

        static ReflectionEx()
        {
            _rentalArrays = new Stack<object[]>[4];
            for (int i = 0; i < 4; ++i)
            {
                _rentalArrays[i] = new Stack<object[]>(capacity: 8);
            }
        }

        /// <summary>
        /// Get the <see cref="Type"/> object with the specified name in
        /// <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">
        /// The assembly which defines the type.
        /// </param>
        /// <param name="typeName">
        /// The full name of the type.
        /// </param>
        /// <param name="throwOnError">
        /// <see langword="true"/> to throw an exception if the type is not found;
        /// <see langword="false"/> to return null.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        internal static Type GetType(
            Assembly assembly,
            string typeName,
            bool throwOnError = true)
        {
            _ = assembly ?? throw new ArgumentNullException(nameof(assembly));
            _ = typeName ?? throw new ArgumentNullException(nameof(typeName));

            if (typeName.Length == 0) throw new ArgumentException("Empty string.", nameof(typeName));

            var key = new TypeKey(assembly, typeName);
            if (!_typeLookup.TryGetValue(key, out Type type))
            {
                type = assembly.GetType(typeName, throwOnError: throwOnError, ignoreCase: false);
                _typeLookup[key] = type;
            }
            else if (throwOnError && type is null)
            {
                // This line will throw, since we know the input parameters are the same as a
                // previous call that failed with 'throwOnError = false'.
                return assembly.GetType(typeName, throwOnError: true, ignoreCase: false);
            }

            return type;
        }

        /// <summary>
        /// Searches for the specified field, using the specified binding constraints.
        /// </summary>
        /// <param name="type">
        /// The type which defines the field.
        /// </param>
        /// <param name="name">
        /// The name of the data field to get.
        /// </param>
        /// <param name="flags">
        /// Binding flags that specify how the search is conducted.
        /// </param>
        /// <param name="throwOnError">
        /// <see langword="true"/> to throw an exception if the field is not found;
        /// <see langword="false"/> to return null.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ReflectionException"/>
        internal static FieldInfo GetField(
            Type type,
            string name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));
            _ = name ?? throw new ArgumentNullException(nameof(name));

            if (name.Length == 0) throw new ArgumentException("Empty string.", nameof(name));

            var key = new FieldPropertyKey(type, name);
            if (!_fieldLookup.TryGetValue(key, out FieldInfo field))
            {
                field = type.GetField(name, flags);
                _fieldLookup[key] = field;
            }

            if (throwOnError && field is null)
            {
                throw new ReflectionException($"Unable to find field {name} for declaring type {type}.");
            }

            return field;
        }

        /// <summary>
        /// Searches for the specified property, using the specified binding constraints.
        /// </summary>
        /// <param name="type">
        /// The type which defines the property.
        /// </param>
        /// <param name="name">
        /// The name of the property to get.
        /// </param>
        /// <param name="flags">
        /// Binding flags that specify how the search is conducted.
        /// </param>
        /// <param name="throwOnError">
        /// <see langword="true"/> to throw an exception if the property is not found;
        /// <see langword="false"/> to return null.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ReflectionException"/>
        internal static PropertyInfo GetProperty(
            Type type,
            string name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));
            _ = name ?? throw new ArgumentNullException(nameof(name));

            if (name.Length == 0) throw new ArgumentException("Empty string.", nameof(name));

            var key = new FieldPropertyKey(type, name);
            if (!_propertyLookup.TryGetValue(key, out PropertyInfo property))
            {
                property = type.GetProperty(name, flags);
                _propertyLookup[key] = property;
            }

            if (throwOnError && property is null)
            {
                throw new ReflectionException($"Unable to find field {name} for declaring type {type}.");
            }

            return property;
        }

        /// <summary>
        /// Searches for the specified field or property member, using the specified binding constraints.
        /// </summary>
        /// <param name="type">
        /// The type which defines the member.
        /// </param>
        /// <param name="name">
        /// The name of the member to get.
        /// </param>
        /// <param name="flags">
        /// Binding flags that specify how the search is conducted.
        /// </param>
        /// <param name="throwOnError">
        /// <see langword="true"/> to throw an exception if the member is not found;
        /// <see langword="false"/> to return null.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ReflectionException"/>
        internal static ValueMemberInfo GetFieldOrProperty(
            Type type,
            string name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));
            _ = name ?? throw new ArgumentNullException(nameof(name));

            if (name.Length == 0) throw new ArgumentException("Empty string.", nameof(name));

            var key = new FieldPropertyKey(type, name);

            if (_fieldLookup.TryGetValue(key, out FieldInfo field) && field is not null)
            {
                return field;
            }

            if (_propertyLookup.TryGetValue(key, out PropertyInfo property) && property is not null)
            {
                return property;
            }

            _fieldLookup[key] = field = type.GetField(name, flags);
            if (field is not null) return field;

            _propertyLookup[key] = property = type.GetProperty(name, flags);
            if (property is not null) return property;

            if (throwOnError)
            {
                throw new ReflectionException($"Unable to find field {name} for declaring type {type}.");
            }

            return default;
        }

        /// <summary>
        /// Searches for the specified method, using the specified binding constraints.
        /// </summary>
        /// <param name="type">
        /// The type which defines the method.
        /// </param>
        /// <param name="name">
        /// The name of the method to get.
        /// </param>
        /// <param name="flags">
        /// Binding flags that specify how the search is conducted.
        /// </param>
        /// <param name="types">
        /// An array of <see cref="Type"/> objects representing the number, order, and type of the
        /// parameters for the method to get.
        /// </param>
        /// <param name="throwOnError">
        /// <see langword="true"/> to throw an exception if the method is not found;
        /// <see langword="false"/> to return null.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ReflectionException"/>
        internal static MethodInfo GetMethod(
            Type type,
            string name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            ParamsArray<Type> types = default,
            bool throwOnError = true)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));
            _ = name ?? throw new ArgumentNullException(nameof(name));

            if (name.Length == 0) throw new ArgumentException("Empty string.", nameof(name));

            var key = new MethodKey(type, name, types: types);
            if (!_methodLookup.TryGetValue(key, out MethodInfo method))
            {
                if (types.Length == 0)
                {
                    method = type.GetMethod(name: name, bindingAttr: flags);
                }
                else
                {
                    var typesArray = types.ToArray();

                    method = type.GetMethod(
                        name: name,
                        bindingAttr: flags,
                        binder: null,
                        callConvention: default,
                        types: typesArray,
                        modifiers: null);
                }
                _methodLookup[key] = method;
            }

            if (throwOnError && method is null)
            {
                throw new ReflectionException($"Unable to find method {name} for declaring type {type}.");
            }

            return method;
        }

        /// <summary>
        /// Rent an array with the given length in range [1..4].
        /// </summary>
        /// <remarks>
        /// Intended for use as an arguments array in a reflection call.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static object[] RentArray(int length)
        {
            if (length < 1 || length > 4) throw new ArgumentOutOfRangeException(nameof(length));

            var stack = _rentalArrays[length - 1];
            var array = stack.Count > 0 ? stack.Pop() : new object[length];

            return array;
        }

        /// <summary>
        /// Rent an array containing the given argument(s).
        /// </summary>
        /// <remarks>
        /// Intended for use as an arguments array in a reflection call.
        /// </remarks>
        public static object[] RentArrayWithArguments(object arg0)
        {
            var array = RentArray(1);
            array[0] = arg0;

            return array;
        }

        /// <inheritdoc cref="RentArrayWithArguments(object)"/>
        public static object[] RentArrayWithArguments(object arg0, object arg1)
        {
            var array = RentArray(2);
            array[0] = arg0;
            array[1] = arg1;

            return array;
        }

        /// <inheritdoc cref="RentArrayWithArguments(object)"/>
        public static object[] RentArrayWithArguments(object arg0, object arg1, object arg2)
        {
            var array = RentArray(3);
            array[0] = arg0;
            array[1] = arg1;
            array[2] = arg2;

            return array;
        }

        /// <inheritdoc cref="RentArrayWithArguments(object)"/>
        public static object[] RentArrayWithArguments(object arg0, object arg1, object arg2, object arg3)
        {
            var array = RentArray(4);
            array[0] = arg0;
            array[1] = arg1;
            array[2] = arg2;
            array[3] = arg3;

            return array;
        }

        /// <summary>
        /// Clear and return a rented array to the pool.
        /// </summary>
        public static void ReturnArray(object[] array)
        {
            if (array is null) return;

            int length = array.Length;
            if (length < 1 || length > 4) return;

            Array.Clear(array, 0, length);
            _rentalArrays[length - 1].Push(array);
        }
    }
}
