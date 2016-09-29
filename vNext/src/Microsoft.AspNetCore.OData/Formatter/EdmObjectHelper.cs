// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter
{
    internal static class EdmObjectHelper
    {
        public static IEdmObject ConvertToEdmObject(this IEnumerable enumerable, IEdmCollectionTypeReference collectionType)
        {
            Contract.Assert(enumerable != null);
            Contract.Assert(collectionType != null);

            IEdmTypeReference elementType = collectionType.ElementType();

            if (elementType.IsEntity())
            {
                EdmEntityObjectCollection entityCollection =
                                        new EdmEntityObjectCollection(collectionType);

                foreach (EdmEntityObject entityObject in enumerable)
                {
                    entityCollection.Add(entityObject);
                }

                return entityCollection;
            }
            else if (elementType.IsComplex())
            {
                EdmComplexObjectCollection complexCollection =
                                        new EdmComplexObjectCollection(collectionType);

                foreach (EdmComplexObject complexObject in enumerable)
                {
                    complexCollection.Add(complexObject);
                }

                return complexCollection;
            }
            else if (elementType.IsEnum())
            {
                EdmEnumObjectCollection enumCollection =
                                        new EdmEnumObjectCollection(collectionType);

                foreach (EdmEnumObject enumObject in enumerable)
                {
                    enumCollection.Add(enumObject);
                }

                return enumCollection;
            }

            return null;
        }
    }
}
