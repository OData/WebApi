// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        // cache the clrtype to ODataDeserializer mappings as we might have to crawl the 
        // inheritance hirerachy to find the mapping.
        private ConcurrentDictionary<Type, ODataDeserializer> _clrTypeMappingCache = new ConcurrentDictionary<Type, ODataDeserializer>();

        public DefaultODataDeserializerProvider(IEdmModel edmModel)
            : base(edmModel)
        {
        }

        protected override ODataEntryDeserializer CreateDeserializer(IEdmTypeReference edmType)
        {
            if (edmType != null)
            {
                switch (edmType.TypeKind())
                {
                    case EdmTypeKind.Entity:
                        return new ODataEntityDeserializer(edmType.AsEntity(), this);

                    case EdmTypeKind.Primitive:
                        return new ODataPrimitiveDeserializer(edmType.AsPrimitive());

                    case EdmTypeKind.Complex:
                        return new ODataComplexTypeDeserializer(edmType.AsComplex(), this);

                    case EdmTypeKind.Collection:
                        IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                        if (collectionType.ElementType().IsEntity())
                        {
                            return new ODataFeedDeserializer(collectionType, this);
                        }
                        else
                        {
                            return new ODataCollectionDeserializer(collectionType, this);
                        }
                }
            }

            return null;
        }

        public override ODataDeserializer GetODataDeserializer(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (type == typeof(Uri))
            {
                return new ODataEntityReferenceLinkDeserializer();
            }

            if (typeof(ODataActionParameters).IsAssignableFrom(type))
            {
                return new ODataActionPayloadDeserializer(type, this);
            }

            return _clrTypeMappingCache.GetOrAdd(type, (t) =>
            {
                IEdmTypeReference edmType = EdmModel.GetEdmTypeReference(t);
                if (edmType == null)
                {
                    return null;
                }
                else
                {
                    return GetODataDeserializer(edmType);
                }
            });
        }
    }
}
