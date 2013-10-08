// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal static class CollectionDeserializationHelpers
    {
        private static readonly Type[] _emptyTypeArray = new Type[0];
        private static readonly object[] _emptyObjectArray = new object[0];
        private static readonly MethodInfo _toArrayMethodInfo = typeof(Enumerable).GetMethod("ToArray");

        public static void AddToCollection(this IEnumerable items, IEnumerable collection, Type elementType, Type resourceType, string propertyName, Type propertyType)
        {
            Contract.Assert(items != null);
            Contract.Assert(collection != null);
            Contract.Assert(elementType != null);
            Contract.Assert(resourceType != null);
            Contract.Assert(propertyName != null);
            Contract.Assert(propertyType != null);

            MethodInfo addMethod = null;
            IList list = collection as IList;

            if (list == null)
            {
                addMethod = collection.GetType().GetMethod("Add", new Type[] { elementType });
                if (addMethod == null)
                {
                    string message = Error.Format(SRResources.CollectionShouldHaveAddMethod, propertyType.FullName, propertyName, resourceType.FullName);
                    throw new SerializationException(message);
                }
            }
            else if (list.GetType().IsArray)
            {
                string message = Error.Format(SRResources.GetOnlyCollectionCannotBeArray, propertyName, resourceType.FullName);
                throw new SerializationException(message);
            }

            bool isNonstandardEdmPrimitiveCollection;
            EdmLibHelpers.IsNonstandardEdmPrimitive(elementType, out isNonstandardEdmPrimitiveCollection);

            foreach (object item in items)
            {
                object element = item;

                if (isNonstandardEdmPrimitiveCollection && element != null)
                {
                    // convert non-standard edm primitives if required.
                    element = EdmPrimitiveHelpers.ConvertPrimitiveValue(element, elementType);
                }

                if (list != null)
                {
                    list.Add(element);
                }
                else
                {
                    Contract.Assert(addMethod != null);
                    addMethod.Invoke(collection, new object[] { element });
                }
            }
        }

        public static void Clear(this IEnumerable collection, string propertyName, Type resourceType)
        {
            Contract.Assert(collection != null);

            MethodInfo clearMethod = collection.GetType().GetMethod("Clear", _emptyTypeArray);
            if (clearMethod == null)
            {
                string message = Error.Format(SRResources.CollectionShouldHaveClearMethod, collection.GetType().FullName,
                    propertyName, resourceType.FullName);
                throw new SerializationException(message);
            }

            clearMethod.Invoke(collection, _emptyObjectArray);
        }

        public static bool TryCreateInstance(Type collectionType, IEdmCollectionTypeReference edmCollectionType, Type elementType, out IEnumerable instance)
        {
            Contract.Assert(collectionType != null);

            if (collectionType == typeof(EdmComplexObjectCollection))
            {
                instance = new EdmComplexObjectCollection(edmCollectionType);
                return true;
            }
            else if (collectionType == typeof(EdmEntityObjectCollection))
            {
                instance = new EdmEntityObjectCollection(edmCollectionType);
                return true;
            }
            else if (collectionType.IsGenericType)
            {
                Type genericDefinition = collectionType.GetGenericTypeDefinition();
                if (genericDefinition == typeof(IEnumerable<>) ||
                    genericDefinition == typeof(ICollection<>) ||
                    genericDefinition == typeof(IList<>))
                {
                    instance = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IEnumerable;
                    return true;
                }
            }

            if (collectionType.IsArray)
            {
                // We dont know the size of the collection in advance. So, create a list and later call ToArray. 
                instance = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IEnumerable;
                return true;
            }

            if (collectionType.GetConstructor(Type.EmptyTypes) != null && !collectionType.IsAbstract)
            {
                instance = Activator.CreateInstance(collectionType) as IEnumerable;
                return true;
            }

            instance = null;
            return false;
        }

        public static IEnumerable ToArray(IEnumerable value, Type elementType)
        {
            return _toArrayMethodInfo.MakeGenericMethod(elementType).Invoke(null, new object[] { value }) as IEnumerable;
        }
    }
}
