// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider" />
    /// </summary>
    internal class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        // cache the clrtype to ODataSerializer mappings as we might have to crawl the 
        // inheritance hirerachy to find the mapping.
        private ConcurrentDictionary<Type, ODataSerializer> _clrTypeMappingCache = new ConcurrentDictionary<Type, ODataSerializer>();

        public DefaultODataSerializerProvider(IEdmModel edmModel)
            : base(edmModel)
        {
        }

        public override ODataSerializer CreateEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Primitive:
                    return new ODataPrimitiveSerializer(edmType.AsPrimitive());

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.ElementType().IsEntity())
                    {
                        return new ODataFeedSerializer(collectionType, this);
                    }
                    else
                    {
                        return new ODataCollectionSerializer(collectionType, this);
                    }

                case EdmTypeKind.Complex:
                    return new ODataComplexTypeSerializer(edmType.AsComplex(), this);

                case EdmTypeKind.Entity:
                    return new ODataEntityTypeSerializer(edmType.AsEntity(), this);

                default:
                    throw Error.InvalidOperation(SRResources.TypeCannotBeSerialized, edmType.ToTraceString(), typeof(ODataMediaTypeFormatter).Name);
            }
        }

        public override ODataSerializer GetODataPayloadSerializer(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            // handle the special types.
            if (type == typeof(ODataWorkspace))
            {
                return new ODataWorkspaceSerializer();
            }
            else if (type == typeof(Uri))
            {
                return new ODataEntityReferenceLinkSerializer();
            }
            else if (type == typeof(ODataError) || type == typeof(HttpError))
            {
                return new ODataErrorSerializer();
            }
            else if (typeof(IEdmModel).IsAssignableFrom(type))
            {
                return new ODataMetadataSerializer();
            }

            // TODO: Feature #694 - support Uri[] => EntityReferenceLinks

            // if it is not a special type, assume it has a corresponding EdmType.
            return _clrTypeMappingCache.GetOrAdd(type, (t) =>
            {
                IEdmTypeReference edmType = EdmModel.GetEdmTypeReference(t);
                if (edmType != null)
                {
                    return GetEdmTypeSerializer(edmType);
                }
                else
                {
                    return null;
                }
            });
        }
    }
}
