﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace JakePerry.Reflection
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
            public readonly Substring typeName;

            public TypeKey(Assembly a, Substring n) { assembly = a; typeName = n; }
        }

        /// <summary>
        /// Unique key for caching field &amp; property lookups.
        /// </summary>
        private readonly struct FieldPropertyKey
        {
            public readonly Type type;
            public readonly Substring name;

            public FieldPropertyKey(Type t, Substring n) { type = t; name = n; }
        }

        /// <summary>
        /// Unique key for caching method lookups.
        /// </summary>
        private readonly struct MethodKey
        {
            public readonly Type type;
            public readonly Substring methodName;
            public readonly ParamsArray<Type> types;

            public MethodKey(
                Type type,
                Substring methodName,
                ParamsArray<Type> types = default)
            {
                this.type = type;
                this.methodName = methodName;
                this.types = types;
            }
        }

        private readonly struct GenericTypeKey
        {
            public readonly Type type;
            public readonly ParamsArray<Type> typeArguments;

            public GenericTypeKey(Type type, ParamsArray<Type> typeArguments)
            {
                this.type = type;
                this.typeArguments = typeArguments;
            }
        }

        /// <summary>
        /// Unique key for caching generic method lookups.
        /// </summary>
        private readonly struct GenericMethodKey
        {
            public readonly MethodInfo method;
            public readonly ParamsArray<Type> typeArguments;

            public GenericMethodKey(MethodInfo method, ParamsArray<Type> typeArguments)
            {
                this.method = method;
                this.typeArguments = typeArguments;
            }
        }

        /// <summary>
        /// An array of pools, where each pool in the array contains
        /// instances of <see cref="object"/>[] with a length equal to one more than
        /// the index of the buffer in the array.
        /// <para/>
        /// ie. <c>Print(_arrayPools[3].Rent().Length)</c> prints 4.
        /// </summary>
        private static readonly ObjectPool<object[]>[] _arrayPools;

        private static readonly Dictionary<TypeKey, Type> _typeLookup = new();
        private static readonly Dictionary<FieldPropertyKey, FieldInfo> _fieldLookup = new();
        private static readonly Dictionary<FieldPropertyKey, PropertyInfo> _propertyLookup = new();
        private static readonly Dictionary<MethodKey, ConstructorInfo> _ctorLookup = new();
        private static readonly Dictionary<MethodKey, MethodInfo> _methodLookup = new();
        private static readonly Dictionary<GenericTypeKey, Type> _genericTypeLookup = new();
        private static readonly Dictionary<GenericMethodKey, MethodInfo> _genericMethodLookup = new();

        static ReflectionEx()
        {
            static void Teardown(object[] o) => Array.Clear(o, 0, o.Length);

            _arrayPools = new ObjectPool<object[]>[4];
            for (int i = 0; i < 4; ++i)
            {
                int length = i + 1;
                _arrayPools[i] = new ObjectPool<object[]>(() => new object[length], Teardown)
                {
                    Capacity = 4
                };
            }
        }

        /// <summary>
        /// Get the <see cref="Type"/> object with the specified name.
        /// </summary>
        /// <param name="typeName">
        /// The full name of the type.
        /// </param>
        /// <param name="throwOnError">
        /// <see langword="true"/> to throw an exception if the type is not found;
        /// <see langword="false"/> to return null.
        /// </param>
        /// <exception cref="ArgumentException"/>
        internal static Type GetType(
            Substring typeName,
            bool throwOnError = true)
        {
            Enforce.Argument(typeName, nameof(typeName)).IsNotEmpty();

            TypeKey key = new(null, typeName);
            if (!_typeLookup.TryGetValue(key, out Type type))
            {
                // Prevent storing keys with substring of a larger string
                string typeNameStr = typeName.CopyString();
                key = new(null, typeNameStr);

                type = Type.GetType(typeNameStr, throwOnError: throwOnError, ignoreCase: false);
                _typeLookup[key] = type;
            }
            else if (throwOnError && type is null)
            {
                // This line will throw, since we know the input parameters are the same as a
                // previous call that failed with 'throwOnError = false'.
                type = Type.GetType(typeName.CopyString(), throwOnError: true, ignoreCase: false);
            }

            return type;
        }

        /// <inheritdoc cref="GetType(Substring, bool)"/>
        /// <exception cref="ArgumentNullException"/>
        internal static Type GetType(
            string typeName,
            bool throwOnError = true)
        {
            Enforce.Argument(typeName, nameof(typeName)).IsNotNull();

            return GetType((Substring)typeName, throwOnError);
        }

        /// <summary>
        /// Get the <see cref="Type"/> object with the specified name in
        /// <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">
        /// The assembly which defines the type.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <inheritdoc cref="GetType(Substring, bool)"/>
        internal static Type GetType(
            Assembly assembly,
            Substring typeName,
            bool throwOnError = true)
        {
            Enforce.Argument(assembly, nameof(assembly)).IsNotNull();
            Enforce.Argument(typeName, nameof(typeName)).IsNotEmpty();

            TypeKey key = new(assembly, typeName);
            if (!_typeLookup.TryGetValue(key, out Type type))
            {
                // Prevent storing keys with substring of a larger string
                string typeNameStr = typeName.CopyString();
                key = new(assembly, typeNameStr);

                type = assembly.GetType(typeNameStr, throwOnError: throwOnError, ignoreCase: false);
                _typeLookup[key] = type;
            }
            else if (throwOnError && type is null)
            {
                // This line will throw, since we know the input parameters are the same as a
                // previous call that failed with 'throwOnError = false'.
                return assembly.GetType(typeName.CopyString(), throwOnError: true, ignoreCase: false);
            }

            return type;
        }

        /// <inheritdoc cref="GetType(Assembly, Substring, bool)"/>
        internal static Type GetType(
            Assembly assembly,
            string typeName,
            bool throwOnError = true)
        {
            Enforce.Argument(typeName, nameof(typeName)).IsNotNull();

            return GetType(assembly, (Substring)typeName, throwOnError);
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
            Substring name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            Enforce.Argument(type, nameof(type)).IsNotNull();
            Enforce.Argument(name, nameof(name)).IsNotEmpty();

            FieldPropertyKey key = new(type, name);
            if (!_fieldLookup.TryGetValue(key, out FieldInfo field))
            {
                // Prevent storing keys with substring of a larger string
                string nameStr = name.CopyString();
                key = new(type, nameStr);

                field = type.GetField(nameStr, flags);
                _fieldLookup[key] = field;
            }

            if (throwOnError && field is null)
            {
                throw new ReflectionException($"Unable to find field {name} for declaring type {type}.");
            }

            return field;
        }

        /// <inheritdoc cref="GetField(Type, Substring, BindingFlags, bool)"/>
        internal static FieldInfo GetField(
            Type type,
            string name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            Enforce.Argument(name, nameof(name)).IsNotNull();

            return GetField(type, (Substring)name, flags, throwOnError);
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
            Substring name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            Enforce.Argument(type, nameof(type)).IsNotNull();
            Enforce.Argument(name, nameof(name)).IsNotEmpty();

            FieldPropertyKey key = new(type, name);
            if (!_propertyLookup.TryGetValue(key, out PropertyInfo property))
            {
                // Prevent storing keys with substring of a larger string
                string nameStr = name.CopyString();
                key = new(type, nameStr);

                property = type.GetProperty(nameStr, flags);
                _propertyLookup[key] = property;
            }

            if (throwOnError && property is null)
            {
                throw new ReflectionException($"Unable to find field {name} for declaring type {type}.");
            }

            return property;
        }

        /// <inheritdoc cref="GetProperty(Type, Substring, BindingFlags, bool)"/>
        internal static PropertyInfo GetProperty(
            Type type,
            string name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            Enforce.Argument(name, nameof(name)).IsNotNull();

            return GetProperty(type, (Substring)name, flags, throwOnError);
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
            Substring name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            Enforce.Argument(type, nameof(type)).IsNotNull();
            Enforce.Argument(name, nameof(name)).IsNotEmpty();

            FieldPropertyKey key = new(type, name);

            bool fieldWasCached = _fieldLookup.TryGetValue(key, out FieldInfo field);
            if (field is not null) return field;

            bool propertyWasCached = _propertyLookup.TryGetValue(key, out PropertyInfo property);
            if (property is not null) return property;

            string nameStr = null;
            if (!fieldWasCached || !propertyWasCached)
            {
                nameStr = name.CopyString();

                if (!fieldWasCached)
                {
                    _fieldLookup[key] = field = type.GetField(nameStr, flags);
                    if (field is not null) return field;
                }

                if (!propertyWasCached)
                {
                    _propertyLookup[key] = property = type.GetProperty(nameStr, flags);
                    if (property is not null) return property;
                }
            }

            if (throwOnError)
            {
                throw new ReflectionException($"Unable to find field or property {nameStr ?? name.CopyString()} for declaring type {type}.");
            }

            return default;
        }

        /// <inheritdoc cref="GetFieldOrProperty(Type, Substring, BindingFlags, bool)"/>
        internal static ValueMemberInfo GetFieldOrProperty(
            Type type,
            string name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            bool throwOnError = true)
        {
            Enforce.Argument(name, nameof(name)).IsNotNull();

            return GetFieldOrProperty(type, (Substring)name, flags, throwOnError);
        }

        /// <summary>
        /// Searches for a constructor whose parameters match the specified argument types.
        /// </summary>
        /// <param name="type">
        /// The type which defines the method.
        /// </param>
        /// <param name="flags">
        /// Binding flags that specify how the search is conducted.
        /// </param>
        /// <param name="types">
        /// An array of <see cref="Type"/> objects representing the number, order, and type of the
        /// parameters for the constructor to get.
        /// </param>
        /// <param name="throwOnError">
        /// <see langword="true"/> to throw an exception if the constructor is not found;
        /// <see langword="false"/> to return null.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ReflectionException"/>
        internal static ConstructorInfo GetConstructor(
            Type type,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            ParamsArray<Type> types = default,
            bool throwOnError = true)
        {
            Enforce.Argument(type, nameof(type)).IsNotNull();

            MethodKey key = new(type, string.Empty, types: types);
            if (!_ctorLookup.TryGetValue(key, out ConstructorInfo ctor))
            {
                if (types.Length == 0)
                {
                    ctor = type.GetConstructor(flags, null, Array.Empty<Type>(), null);
                }
                else
                {
                    Type[] typesArray = types.ToArray();

                    ctor = type.GetConstructor(
                        bindingAttr: flags,
                        binder: null,
                        callConvention: default,
                        types: typesArray,
                        modifiers: null);
                }
                _ctorLookup[key] = ctor;
            }

            if (throwOnError && ctor is null)
            {
                throw new ReflectionException($"Unable to find matching constructor for declaring type {type}.");
            }

            return ctor;
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
            Substring name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            ParamsArray<Type> types = default,
            bool throwOnError = true)
        {
            Enforce.Argument(type, nameof(type)).IsNotNull();
            Enforce.Argument(name, nameof(name)).IsNotEmpty();

            MethodKey key = new(type, name, types: types);
            if (!_methodLookup.TryGetValue(key, out MethodInfo method))
            {
                // Prevent storing keys with substring of a larger string
                string nameStr = name.CopyString();
                key = new(type, nameStr, types: types);

                if (types.Length == 0)
                {
                    method = type.GetMethod(name: nameStr, bindingAttr: flags);
                }
                else
                {
                    Type[] typesArray = types.ToArray();

                    method = type.GetMethod(
                        name: nameStr,
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

        /// <inheritdoc cref="GetMethod(Type, Substring, BindingFlags, ParamsArray{Type}, bool)"/>
        internal static MethodInfo GetMethod(
            Type type,
            string name,
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public,
            ParamsArray<Type> types = default,
            bool throwOnError = true)
        {
            Enforce.Argument(name, nameof(name)).IsNotNull();

            return GetMethod(type, (Substring)name, flags, types, throwOnError);
        }

        /// <summary>
        /// Substitutes the elements of an array of types for the type parameters of the
        /// generic type definition <paramref name="type"/> and returns a <see cref="Type"/> object
        /// representing the resulting constructed type.
        /// </summary>
        /// <param name="type">
        /// A generic type definition.
        /// </param>
        /// <param name="typeArguments">
        /// An array of types to be substituted for the type parameters of the generic type.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <seealso cref="Type.MakeGenericType(Type[])"/>
        internal static Type MakeGenericType(
            Type type,
            ParamsArray<Type> typeArguments)
        {
            Enforce.Argument(type, nameof(type)).IsNotNull();

            GenericTypeKey key = new(type, typeArguments);
            if (!_genericTypeLookup.TryGetValue(key, out Type genericType))
            {
                genericType = type.MakeGenericType(typeArguments.ToArray());
                _genericTypeLookup[key] = genericType;
            }

            return genericType;
        }

        /// <summary>
        /// Substitutes the elements of an array of type parameters of the generic method definition
        /// <paramref name="method"/>, and returns a <see cref="MethodInfo"/> object representing
        /// the resulting constructed method.
        /// </summary>
        /// <param name="method">
        /// A generic method definition.
        /// </param>
        /// <param name="typeArguments">
        /// An array of types to be substituted for the type parameters of the generic method definition.
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        /// <seealso cref="MethodInfo.MakeGenericMethod(Type[])"/>
        internal static MethodInfo MakeGenericMethod(
            MethodInfo method,
            ParamsArray<Type> typeArguments)
        {
            Enforce.Argument(method, nameof(method)).IsNotNull();

            GenericMethodKey key = new(method, typeArguments);
            if (!_genericMethodLookup.TryGetValue(key, out MethodInfo genericMethod))
            {
                genericMethod = method.MakeGenericMethod(typeArguments.ToArray());
                _genericMethodLookup[key] = genericMethod;
            }

            return genericMethod;
        }

        /// <summary>
        /// Get the default value for <paramref name="type"/>.
        /// </summary>
        /// <returns>
        /// Returns <see langword="null"/> if <paramref name="type"/> is a reference type;
        /// Otherwise, returns the default constructed value if it is a value type.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public static object GetDefaultValue(Type type)
        {
            Enforce.Argument(type, nameof(type)).IsNotNull();

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        /// <summary>
        /// Get a pool containing arrays of the specified length in range [1..4].
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static ObjectPool<object[]> GetArrayPool(int length)
        {
            Enforce.Argument(length, nameof(length)).IsBetween(1, 4);

            return _arrayPools[length - 1];
        }

        /// <summary>
        /// Rent an array with the given length in range [1..4].
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public static object[] RentArray(int length)
        {
            ObjectPool<object[]> pool = GetArrayPool(length);
            return pool.Rent();
        }

        /// <summary>
        /// Rent an array with the given length in range [1..4] and obtain a scope
        /// object that will handle returning said array when it is disposed.
        /// </summary>
        /// <remarks>
        /// See <see cref="ObjectPool{T}.RentInScope(out T)"/> for usage guidelines.
        /// </remarks>
        public static ObjectPool<object[]>.RentalScope RentArrayInScope(int length, out object[] obj)
        {
            ObjectPool<object[]> pool = GetArrayPool(length);
            return pool.RentInScope(out obj);
        }

        /// <summary>
        /// Rent an array contaning the given argument(s) and obtain a scope
        /// object that will handle returning said array when it is disposed.
        /// </summary>
        /// <inheritdoc cref="RentArrayInScope(int, out object[])"/>
        public static ObjectPool<object[]>.RentalScope RentArrayWithArgsInScope(out object[] obj, object arg0)
        {
            ObjectPool<object[]>.RentalScope scope = RentArrayInScope(1, out obj);
            obj[0] = arg0;

            return scope;
        }

        /// <inheritdoc cref="RentArrayWithArgsInScope(out object[], object)"/>
        public static ObjectPool<object[]>.RentalScope RentArrayWithArgsInScope(out object[] obj, object arg0, object arg1)
        {
            ObjectPool<object[]>.RentalScope scope = RentArrayInScope(2, out obj);
            obj[0] = arg0;
            obj[1] = arg1;

            return scope;
        }

        /// <inheritdoc cref="RentArrayWithArgsInScope(out object[], object)"/>
        public static ObjectPool<object[]>.RentalScope RentArrayWithArgsInScope(out object[] obj, object arg0, object arg1, object arg2)
        {
            ObjectPool<object[]>.RentalScope scope = RentArrayInScope(3, out obj);
            obj[0] = arg0;
            obj[1] = arg1;
            obj[2] = arg2;

            return scope;
        }

        /// <inheritdoc cref="RentArrayWithArgsInScope(out object[], object)"/>
        public static ObjectPool<object[]>.RentalScope RentArrayWithArgsInScope(out object[] obj, object arg0, object arg1, object arg2, object arg3)
        {
            ObjectPool<object[]>.RentalScope scope = RentArrayInScope(3, out obj);
            obj[0] = arg0;
            obj[1] = arg1;
            obj[2] = arg2;
            obj[3] = arg3;

            return scope;
        }

        /// <summary>
        /// Clear and return a rented array to the pool.
        /// </summary>
        public static void ReturnArray(object[] array)
        {
            if (array is null) return;

            ObjectPool<object[]> pool = GetArrayPool(array.Length);
            pool.Return(array);
        }
    }
}
