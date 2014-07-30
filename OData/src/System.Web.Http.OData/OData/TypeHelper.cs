// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Formatter;

namespace System.Web.Http.OData
{
    internal static class TypeHelper
    {
        public static Type ToNullable(this Type t)
        {
            if (t.IsNullable())
            {
                return t;
            }
            else
            {
                return typeof(Nullable<>).MakeGenericType(t);
            }
        }

        // Gets the collection element type.
        public static Type GetInnerElementType(this Type type)
        {
            Type elementType;
            type.IsCollection(out elementType);
            Contract.Assert(elementType != null);

            return elementType;
        }

        public static bool IsCollection(this Type type)
        {
            Type elementType;
            return type.IsCollection(out elementType);
        }

        public static bool IsCollection(this Type type, out Type elementType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            elementType = type;

            // see if this type should be ignored.
            if (type == typeof(string))
            {
                return false;
            }

            Type collectionInterface
                = type.GetInterfaces()
                    .Union(new[] { type })
                    .FirstOrDefault(
                        t => t.IsGenericType
                             && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (collectionInterface != null)
            {
                elementType = collectionInterface.GetGenericArguments().Single();
                return true;
            }

            return false;
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

            return type.IsEnum ||
                   type.IsPrimitive ||
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
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                type = type.GetGenericArguments().First();
            }

            if (type.IsGenericType && type.IsInterface &&
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
                    if (interfaceType.IsGenericType &&
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
        internal static IEnumerable<Type> GetLoadedTypes(IAssembliesResolver assembliesResolver)
        {
            List<Type> result = new List<Type>();

            // Go through all assemblies referenced by the application and search for types matching a predicate
            ICollection<Assembly> assemblies = assembliesResolver.GetAssemblies();
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
                    result.AddRange(exportedTypes.Where(t => t != null && t.IsVisible));
                }
            }

            return result;
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
