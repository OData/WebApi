// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        private readonly ConcurrentDictionary<IEdmTypeReference, ODataEdmTypeSerializer> _serializerCache =
            new ConcurrentDictionary<IEdmTypeReference, ODataEdmTypeSerializer>(new EdmTypeReferenceEqualityComparer());

        // cache the clrtype to edm type mappings as we might have to crawl the inheritance hierarchy to find the mapping.
        private readonly ConcurrentDictionary<Tuple<IEdmModel, Type>, IEdmTypeReference> _clrTypeMappingCache =
            new ConcurrentDictionary<Tuple<IEdmModel, Type>, IEdmTypeReference>();

        private static readonly ODataWorkspaceSerializer _workspaceSerializer = new ODataWorkspaceSerializer();
        private static readonly ODataEntityReferenceLinkSerializer _entityReferenceLinkSerializer = new ODataEntityReferenceLinkSerializer();
        private static readonly ODataEntityReferenceLinksSerializer _entityReferenceLinksSerializer = new ODataEntityReferenceLinksSerializer();
        private static readonly ODataErrorSerializer _errorSerializer = new ODataErrorSerializer();
        private static readonly ODataMetadataSerializer _metadataSerializer = new ODataMetadataSerializer();

        private static readonly DefaultODataSerializerProvider _instance = new DefaultODataSerializerProvider();

        /// <summary>
        /// Gets the default instance of the <see cref="DefaultODataSerializerProvider"/>.
        /// </summary>
        public static DefaultODataSerializerProvider Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <inheritdoc />
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }
            return _serializerCache.GetOrAdd(edmType, CreateEdmTypeSerializer);
        }

        /// <summary>
        /// Sets the <see cref="ODataEdmTypeSerializer"/> for the given <paramref name="edmType"/> in the serializer cache.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="serializer">The serializer to use for the given EDM type.</param>
        public void SetEdmTypeSerializer(IEdmTypeReference edmType, ODataEdmTypeSerializer serializer)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            _serializerCache.AddOrUpdate(edmType, serializer, (t, s) => serializer);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ODataEdmTypeSerializer"/> for the given edm type.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmTypeReference"/>.</param>
        /// <returns>The constructed <see cref="ODataEdmTypeSerializer"/>.</returns>
        public virtual ODataEdmTypeSerializer CreateEdmTypeSerializer(IEdmTypeReference edmType)
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
                    return null;
            }
        }

        /// <inheritdoc />
        public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            // handle the special types.
            if (type == typeof(ODataWorkspace))
            {
                return _workspaceSerializer;
            }
            else if (type == typeof(Uri) || type == typeof(ODataEntityReferenceLink))
            {
                return _entityReferenceLinkSerializer;
            }
            else if (typeof(IEnumerable<Uri>).IsAssignableFrom(type) || type == typeof(ODataEntityReferenceLinks))
            {
                return _entityReferenceLinksSerializer;
            }
            else if (type == typeof(ODataError) || type == typeof(HttpError))
            {
                return _errorSerializer;
            }
            else if (typeof(IEdmModel).IsAssignableFrom(type))
            {
                return _metadataSerializer;
            }

            // TODO: Feature #694 - support Uri[] => EntityReferenceLinks

            // if it is not a special type, assume it has a corresponding EdmType.
            Tuple<IEdmModel, Type> cacheKey = Tuple.Create(model, type);
            IEdmTypeReference edmType = _clrTypeMappingCache.GetOrAdd(cacheKey, (key) =>
            {
                IEdmModel m = key.Item1;
                Type t = key.Item2;
                return m.GetEdmTypeReference(t);
            });

            if (edmType != null)
            {
                return GetEdmTypeSerializer(edmType);
            }
            else
            {
                return null;
            }
        }
    }
}
