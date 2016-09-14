// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        // private readonly IServiceProvider _rootContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataSerializerProvider"/> class.
        /// </summary>
        /// <param name="rootContainer">The root container.</param>
        public DefaultODataSerializerProvider(/*IServiceProvider rootContainer*/)
        {
            /*
            if (rootContainer == null)
            {
                throw Error.ArgumentNull("rootContainer");
            }

            _rootContainer = rootContainer;*/
        }

        public override IServiceProvider ServiceProvider { get; set; }

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
                    return ServiceProvider.GetRequiredService<ODataEnumSerializer>();

                case EdmTypeKind.Primitive:
                    return ServiceProvider.GetRequiredService<ODataPrimitiveSerializer>();

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.Definition.IsDeltaFeed())
                    {
                        return ServiceProvider.GetRequiredService<ODataDeltaFeedSerializer>();
                    }
                    else if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                    {
                        return ServiceProvider.GetRequiredService<ODataResourceSetSerializer>();
                    }
                    else
                    {
                        return ServiceProvider.GetRequiredService<ODataCollectionSerializer>();
                    }

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    return ServiceProvider.GetRequiredService<ODataResourceSerializer>();

                default:
                    return null;
            }
        }

        /// <inheritdoc />
        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            EnsureServiceProvider(request);

            // handle the special types.
            if (type == typeof(ODataServiceDocument))
            {
                return ServiceProvider.GetRequiredService<ODataServiceDocumentSerializer>();
            }
            else if (type == typeof(Uri) || type == typeof(ODataEntityReferenceLink))
            {
                return ServiceProvider.GetRequiredService<ODataEntityReferenceLinkSerializer>();
            }
            else if (typeof(IEnumerable<Uri>).IsAssignableFrom(type) || type == typeof(ODataEntityReferenceLinks))
            {
                return ServiceProvider.GetRequiredService<ODataEntityReferenceLinksSerializer>();
            }
            else if (type == typeof(ODataError) || type == typeof(HttpError))
            {
                return ServiceProvider.GetRequiredService<ODataErrorSerializer>();
            }
            else if (typeof(IEdmModel).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                return ServiceProvider.GetRequiredService<ODataMetadataSerializer>();
            }

            // if it is not a special type, assume it has a corresponding EdmType.
            IEdmModel model = request.ODataProperties().Model;
            ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);

            if (edmType != null)
            {
                if (((edmType.IsPrimitive() || edmType.IsEnum()) &&
                    ODataRawValueMediaTypeMapping.IsRawValueRequest(request)) ||
                    ODataCountMediaTypeMapping.IsCountRequest(request))
                {
                    return ServiceProvider.GetRequiredService<ODataRawValueSerializer>();
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

        private void EnsureServiceProvider(HttpRequest request)
        {
            if (ServiceProvider != null)
            {
                return;
            }

            ServiceProvider = request.HttpContext.RequestServices;
        }
    }
}
