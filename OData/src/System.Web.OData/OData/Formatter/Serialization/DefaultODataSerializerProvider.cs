// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        private readonly IServiceProvider _rootContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataSerializerProvider"/> class.
        /// </summary>
        /// <param name="rootContainer">The root container.</param>
        public DefaultODataSerializerProvider(IServiceProvider rootContainer)
        {
            if (rootContainer == null)
            {
                throw Error.ArgumentNull("rootContainer");
            }

            _rootContainer = rootContainer;
        }

        /// <inheritdoc />
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Enum:
                    return _rootContainer.GetRequiredService<ODataEnumSerializer>();

                case EdmTypeKind.Primitive:
                    return _rootContainer.GetRequiredService<ODataPrimitiveSerializer>();

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.Definition.IsDeltaFeed())
                    {
                        return _rootContainer.GetRequiredService<ODataDeltaFeedSerializer>();
                    }
                    else if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                    {
                        return _rootContainer.GetRequiredService<ODataResourceSetSerializer>();
                    }
                    else
                    {
                        return _rootContainer.GetRequiredService<ODataCollectionSerializer>();
                    }

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    return _rootContainer.GetRequiredService<ODataResourceSerializer>();

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public override ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type, HttpRequestMessage request)
        {
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            // handle the special types.
            if (type == typeof(ODataServiceDocument))
            {
                return _rootContainer.GetRequiredService<ODataServiceDocumentSerializer>();
            }
            else if (type == typeof(Uri) || type == typeof(ODataEntityReferenceLink))
            {
                return _rootContainer.GetRequiredService<ODataEntityReferenceLinkSerializer>();
            }
            else if (typeof(IEnumerable<Uri>).IsAssignableFrom(type) || type == typeof(ODataEntityReferenceLinks))
            {
                return _rootContainer.GetRequiredService<ODataEntityReferenceLinksSerializer>();
            }
            else if (type == typeof(ODataError) || type == typeof(HttpError))
            {
                return _rootContainer.GetRequiredService<ODataErrorSerializer>();
            }
            else if (typeof(IEdmModel).IsAssignableFrom(type))
            {
                return _rootContainer.GetRequiredService<ODataMetadataSerializer>();
            }

            // if it is not a special type, assume it has a corresponding EdmType.
            ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);

            if (edmType != null)
            {
                if (((edmType.IsPrimitive() || edmType.IsEnum()) &&
                    ODataRawValueMediaTypeMapping.IsRawValueRequest(request)) ||
                    ODataCountMediaTypeMapping.IsCountRequest(request))
                {
                    return _rootContainer.GetRequiredService<ODataRawValueSerializer>();
                }
                else
                {
                    return GetEdmTypeSerializer(edmType);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
