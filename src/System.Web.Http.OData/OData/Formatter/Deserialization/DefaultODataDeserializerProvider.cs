// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProvider"/>.
    /// </summary>
    public class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        private readonly ConcurrentDictionary<IEdmTypeReference, ODataEdmTypeDeserializer> _deserializerCache =
            new ConcurrentDictionary<IEdmTypeReference, ODataEdmTypeDeserializer>(new EdmTypeReferenceEqualityComparer());

        // cache the clrtype to edmtype mappings as we might have to crawl the inheritance hierarchy to find the mapping.
        private readonly ConcurrentDictionary<Tuple<IEdmModel, Type>, IEdmTypeReference> _clrTypeMappingCache =
            new ConcurrentDictionary<Tuple<IEdmModel, Type>, IEdmTypeReference>();

        private static readonly ODataEntityReferenceLinkDeserializer _entityReferenceLinkDeserializer = new ODataEntityReferenceLinkDeserializer();
        private readonly ODataActionPayloadDeserializer _actionPayloadDeserializer;

        private static readonly DefaultODataDeserializerProvider _instance = new DefaultODataDeserializerProvider();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataDeserializerProvider"/> class.
        /// </summary>
        public DefaultODataDeserializerProvider()
        {
            _actionPayloadDeserializer = new ODataActionPayloadDeserializer(this);
        }

        /// <summary>
        /// Gets the default instance of the <see cref="DefaultODataDeserializerProvider"/>.
        /// </summary>
        public static DefaultODataDeserializerProvider Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <inheritdoc />
        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            return _deserializerCache.GetOrAdd(edmType, CreateEdmTypeDeserializer);
        }

        /// <summary>
        /// Sets the <see cref="ODataEdmTypeDeserializer"/> for the given edmType in the deserializer cache.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="deserializer">The deserializer to use for the given EDM type.</param>
        public void SetEdmTypeDeserializer(IEdmTypeReference edmType, ODataEdmTypeDeserializer deserializer)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            _deserializerCache.AddOrUpdate(edmType, deserializer, (t, s) => deserializer);
        }

        /// <summary>
        /// Creates an <see cref="ODataEdmTypeDeserializer"/> that can deserialize payloads of the given <paramref name="edmType"/>.
        /// </summary>
        /// <param name="edmType">The EDM type that the created deserializer can handle.</param>
        /// <returns>The created deserializer.</returns>
        /// <remarks> Override this method if you want to use a custom deserializer. <see cref="GetEdmTypeDeserializer"/> calls into this method and caches the result.</remarks>
        public virtual ODataEdmTypeDeserializer CreateEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

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

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public override ODataDeserializer GetODataDeserializer(IEdmModel model, Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (type == typeof(Uri))
            {
                return _entityReferenceLinkDeserializer;
            }

            if (type == typeof(ODataActionParameters))
            {
                return _actionPayloadDeserializer;
            }

            Tuple<IEdmModel, Type> cacheKey = Tuple.Create(model, type);
            IEdmTypeReference edmType = _clrTypeMappingCache.GetOrAdd(cacheKey, (key) =>
            {
                IEdmModel m = key.Item1;
                Type t = key.Item2;
                return m.GetEdmTypeReference(t);
            });

            if (edmType == null)
            {
                return null;
            }
            else
            {
                return GetEdmTypeDeserializer(edmType);
            }
        }
    }
}
