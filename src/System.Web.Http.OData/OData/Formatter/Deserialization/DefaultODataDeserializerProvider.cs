// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        // cache the clrtype to ODataDeserializer mappings as we might have to crawl the 
        // inheritance hierarchy to find the mapping.
        private readonly ConcurrentDictionary<Tuple<IEdmModel, Type>, ODataDeserializer> _clrTypeMappingCache =
            new ConcurrentDictionary<Tuple<IEdmModel, Type>, ODataDeserializer>();

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

        public override ODataDeserializer GetODataDeserializer(IEdmModel model, Type type)
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

            Tuple<IEdmModel, Type> cacheKey = Tuple.Create(model, type);
            return _clrTypeMappingCache.GetOrAdd(cacheKey, (key) =>
            {
                IEdmModel m = key.Item1;
                Type t = key.Item2;
                IEdmTypeReference edmType = m.GetEdmTypeReference(t);
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
