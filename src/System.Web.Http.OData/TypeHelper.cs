// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace System.Web.Http
{
    internal static class TypeHelper
    {
        /// <summary>
        /// Returns type of T if the type implements IEnumerable of T, otherwise, return null.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Type GetImplementedIEnumerableType(Type type)
        {
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
