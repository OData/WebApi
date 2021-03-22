// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData
{
    internal static class TypeHelper
    {
        /// <summary>
        /// Return the memberInfo from a type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The memberInfo from a type.</returns>
        public static MemberInfo AsMemberInfo(Type clrType)
        {
            return clrType as MemberInfo;
        }

        /// <summary>
        /// Return the type from a MemberInfo.
        /// </summary>
        /// <param name="memberInfo">The MemberInfo to convert.</param>
        /// <returns>The type from a MemberInfo.</returns>
        public static Type AsType(MemberInfo memberInfo)
        {
            return memberInfo as Type;
        }

        /// <summary>
        /// Return the assembly from a type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The assembly from a type.</returns>
        public static Assembly GetAssembly(Type clrType)
        {
            return clrType.Assembly;
        }

        /// <summary>
        /// Return the base type from a type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The base type from a type.</returns>
        public static Type GetBaseType(Type clrType)
        {
            return clrType.BaseType;
        }

        /// <summary>
        /// Return the qualified name from a member info.
        /// </summary>
        /// <param name="memberInfo">The member info to convert.</param>
        /// <returns>The qualified name from a member info.</returns>
        public static string GetQualifiedName(MemberInfo memberInfo)
        {
            Contract.Assert(memberInfo != null);
            Type type = memberInfo as Type;
            return type != null ? (type.Namespace + "." + type.Name) : memberInfo.Name;
        }

        /// <summary>
        /// Return the reflected type from a member info.
        /// </summary>
        /// <param name="memberInfo">The member info to convert.</param>
        /// <returns>The reflected type from a member info.</returns>
        public static Type GetReflectedType(MemberInfo memberInfo)
        {
            return memberInfo.ReflectedType;
        }

        /// <summary>
        /// Determine if a type is abstract.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is abstract; false otherwise.</returns>
        public static bool IsAbstract(Type clrType)
        {
            return clrType.IsAbstract;
        }

        /// <summary>
        /// Determine if a type is a class.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a class; false otherwise.</returns>
        public static bool IsClass(Type clrType)
        {
            return clrType.IsClass;
        }

        /// <summary>
        /// Determine if a type is a generic type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a generic type; false otherwise.</returns>
        public static bool IsGenericType(this Type clrType)
        {
            return clrType.IsGenericType;
        }

        /// <summary>
        /// Determine if a type is a generic type definition.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a generic type definition; false otherwise.</returns>
        public static bool IsGenericTypeDefinition(this Type clrType)
        {
            return clrType.IsGenericTypeDefinition;
        }

        /// <summary>
        /// Determine if a type is an interface.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an interface; false otherwise.</returns>
        public static bool IsInterface(Type clrType)
        {
            return clrType.IsInterface;
        }

        /// <summary>
        /// Determine if a type is null-able.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is null-able; false otherwise.</returns>
        public static bool IsNullable(Type clrType)
        {
            if (TypeHelper.IsValueType(clrType))
            {
                // value types are only nullable if they are Nullable<T>
                return TypeHelper.IsGenericType(clrType) && clrType.GetGenericTypeDefinition() == typeof(Nullable<>);
            }
            else
            {
                // reference types are always nullable
                return true;
            }
        }

        /// <summary>
        /// Determine if a type is public.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is public; false otherwise.</returns>
        public static bool IsPublic(Type clrType)
        {
            return clrType.IsPublic;
        }

        /// <summary>
        /// Determine if a type is a primitive.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a primitive; false otherwise.</returns>
        public static bool IsPrimitive(Type clrType)
        {
            return clrType.IsPrimitive;
        }

        /// <summary>
        /// Determine if a type is assignable from another type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <param name="fromType">The type to assign from.</param>
        /// <returns>True if the type is assignable; false otherwise.</returns>
        public static bool IsTypeAssignableFrom(Type clrType, Type fromType)
        {
            return clrType.IsAssignableFrom(fromType);
        }

        /// <summary>
        /// Determine if a type is a value type.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a value type; false otherwise.</returns>
        public static bool IsValueType(Type clrType)
        {
            return clrType.IsValueType;
        }

        /// <summary>
        /// Determine if a type is visible.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is visible; false otherwise.</returns>
        public static bool IsVisible(Type clrType)
        {
            return clrType.IsVisible;
        }

        /// <summary>
        /// Return the type from a nullable type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The type from a nullable type.</returns>
        public static Type ToNullable(Type clrType)
        {
            if (TypeHelper.IsNullable(clrType))
            {
                return clrType;
            }
            else
            {
                return typeof(Nullable<>).MakeGenericType(clrType);
            }
        }

        /// <summary>
        /// Return the collection element type.
        /// </summary>
        /// <param name="clrType">The type to convert.</param>
        /// <returns>The collection element type from a type.</returns>
        public static Type GetInnerElementType(Type clrType)
        {
            Type elementType;
            TypeHelper.IsCollection(clrType, out elementType);
            Contract.Assert(elementType != null);

            return elementType;
        }

        /// <summary>
        /// Determine if a type is a collection.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsCollection(Type clrType)
        {
            Type elementType;
            return TypeHelper.IsCollection(clrType, out elementType);
        }

        /// <summary>
        /// Determine if a type is a collection.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <param name="elementType">out: the element type of the collection.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsCollection(Type clrType, out Type elementType)
        {
            if (clrType == null)
            {
                throw Error.ArgumentNull("clrType");
            }

            elementType = clrType;

            // see if this type should be ignored.
            if (clrType == typeof(string))
            {
                return false;
            }

            Type collectionInterface
                = clrType.GetInterfaces()
                    .Union(new[] { clrType })
                    .FirstOrDefault(
                        t => TypeHelper.IsGenericType(t)
                             && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();
                return true;
            }

            return false;
        }

        public static Type GetUnderlyingTypeOrSelf(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        /// <summary>
        /// Determine if a type is an enumeration.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is an enumeration; false otherwise.</returns>
        public static bool IsEnum(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return underlyingTypeOrSelf.IsEnum;
        }

        /// <summary>
        /// Determine if a type is a DateTime.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a DateTime; false otherwise.</returns>
        public static bool IsDateTime(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return Type.GetTypeCode(underlyingTypeOrSelf) == TypeCode.DateTime;
        }

        /// <summary>
        /// Determine if a type is a TimeSpan.
        /// </summary>
        /// <param name="clrType">The type to test.</param>
        /// <returns>True if the type is a TimeSpan; false otherwise.</returns>
        public static bool IsTimeSpan(Type clrType)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(clrType);
            return underlyingTypeOrSelf == typeof(TimeSpan);
        }

        /// <summary>
        /// Determines whether the given type is IQueryable.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns><c>true</c> if the type is IQueryable.</returns>
        internal static bool IsIQueryable(Type type)
        {
            return type == typeof(IQueryable) ||
                (type != null && TypeHelper.IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(IQueryable<>));
        }

        /// <summary>
        /// Determines whether the given type is a primitive type or
        /// is a <see cref="string"/>, <see cref="DateTime"/>, <see cref="Decimal"/>,
        /// <see cref="Guid"/>, <see cref="DateTimeOffset"/> or <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns><c>true</c> if the type is a primitive type.</returns>
        internal static bool IsQueryPrimitiveType(Type type)
        {
            Contract.Assert(type != null);

            type = GetInnerMostElementType(type);

            return TypeHelper.IsEnum(type) ||
                   TypeHelper.IsPrimitive(type) ||
                   type == typeof(Uri) ||
                   (EdmLibHelpers.GetEdmPrimitiveTypeOrNull(type) != null);
        }

        /// <summary>
        /// Returns the innermost element type for a given type, dealing with
        /// nullables, arrays, etc.
        /// </summary>
        /// <param name="type">The type from which to get the innermost type.</param>
        /// <returns>The innermost element type</returns>
        internal static Type GetInnerMostElementType(Type type)
        {
            Contract.Assert(type != null);

            while (true)
            {
                Type nullableUnderlyingType = Nullable.GetUnderlyingType(type);
                if (nullableUnderlyingType != null)
                {
                    type = nullableUnderlyingType;
                }
                else if (type.HasElementType)
                {
                    type = type.GetElementType();
                }
                else
                {
                    return type;
                }
            }
        }

        /// <summary>
        /// Returns type of T if the type implements IEnumerable of T, otherwise, return null.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Type GetImplementedIEnumerableType(Type type)
        {
            // get inner type from Task<T>
            if (TypeHelper.IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                type = type.GetGenericArguments().First();
            }

            if (TypeHelper.IsGenericType(type) && TypeHelper.IsInterface(type) &&
                (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                 type.GetGenericTypeDefinition() == typeof(IQueryable<>)))
            {
                // special case the IEnumerable<T>
                return GetInnerGenericType(type);
            }
            else
            {
                // for the rest of interfaces and strongly Type collections
                Type[] interfaces = type.GetInterfaces();
                foreach (Type interfaceType in interfaces)
                {
                    if (TypeHelper.IsGenericType(interfaceType) &&
                        (interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                         interfaceType.GetGenericTypeDefinition() == typeof(IQueryable<>)))
                    {
                        // special case the IEnumerable<T>
                        return GetInnerGenericType(interfaceType);
                    }
                }
            }

            return null;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catching all exceptions in this case is the right to do.")]
        // This code is copied from DefaultHttpControllerTypeResolver.GetControllerTypes.
        internal static IEnumerable<Type> GetLoadedTypes(IWebApiAssembliesResolver assembliesResolver)
        {
            if (assembliesResolver == null)
            {
                yield return null;
            }

            // Go through all assemblies referenced by the application and search for types matching a predicate
            IEnumerable<Assembly> assemblies = assembliesResolver.Assemblies;
            foreach (Assembly assembly in assemblies)
            {
                Type[] exportedTypes = null;
                if (assembly == null || assembly.IsDynamic)
                {
                    // can't call GetTypes on a null (or dynamic?) assembly
                    continue;
                }

                try
                {
                    exportedTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    continue;
                }

                if (exportedTypes != null)
                {
                    foreach (Type t in exportedTypes)
                    {
                        if ((t != null) && (TypeHelper.IsVisible(t)))
                        {
                            yield return t;
                        }
                    }
                }
            }
        }

        internal static Type GetTaskInnerTypeOrSelf(Type type)
        {
            if (IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return type.GetGenericArguments().First();
            }

            return type;
        }

        internal static void ValidateAssignableFromForArgument(Type expectedType, Type type, string customTypeDescription = null)
        {
            if (!expectedType.IsAssignableFrom(type))
            {
                throw Error.Argument("propertyInfo", SRResources.PropertyTypeShouldBeOfType,
                   customTypeDescription ?? expectedType.FullName);
            }
        }

        private static Type GetInnerGenericType(Type interfaceType)
        {
            // Getting the type T definition if the returning type implements IEnumerable<T>
            Type[] parameterTypes = interfaceType.GetGenericArguments();

            if (parameterTypes.Length == 1)
            {
                return parameterTypes[0];
            }

            return null;
        }
    }
}
