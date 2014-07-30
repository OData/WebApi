// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProvider"/>.
    /// </summary>
    public class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        private static readonly ODataEntityReferenceLinkDeserializer _entityReferenceLinkDeserializer = new ODataEntityReferenceLinkDeserializer();
        private static readonly ODataPrimitiveDeserializer _primitiveDeserializer = new ODataPrimitiveDeserializer();
        private static readonly ODataEnumDeserializer _enumDeserializer = new ODataEnumDeserializer();

        private readonly ODataActionPayloadDeserializer _actionPayloadDeserializer;
        private readonly ODataEntityDeserializer _entityDeserializer;
        private readonly ODataFeedDeserializer _feedDeserializer;
        private readonly ODataCollectionDeserializer _collectionDeserializer;
        private readonly ODataComplexTypeDeserializer _complexDeserializer;

        private static readonly DefaultODataDeserializerProvider _instance = new DefaultODataDeserializerProvider();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataDeserializerProvider"/> class.
        /// </summary>
        public DefaultODataDeserializerProvider()
        {
            _actionPayloadDeserializer = new ODataActionPayloadDeserializer(this);
            _entityDeserializer = new ODataEntityDeserializer(this);
            _feedDeserializer = new ODataFeedDeserializer(this);
            _collectionDeserializer = new ODataCollectionDeserializer(this);
            _complexDeserializer = new ODataComplexTypeDeserializer(this);
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

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Entity:
                    return _entityDeserializer;

                case EdmTypeKind.Enum:
                    return _enumDeserializer;

                case EdmTypeKind.Primitive:
                    return _primitiveDeserializer;

                case EdmTypeKind.Complex:
                    return _complexDeserializer;

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.ElementType().IsEntity())
                    {
                        return _feedDeserializer;
                    }
                    else
                    {
                        return _collectionDeserializer;
                    }

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public override ODataDeserializer GetODataDeserializer(IEdmModel model, Type type, HttpRequestMessage request)
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

            if (type == typeof(ODataActionParameters) || type == typeof(ODataUntypedActionParameters))
            {
                return _actionPayloadDeserializer;
            }

            ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);

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
