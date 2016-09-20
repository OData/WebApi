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
    public class DefaultODataSerializerProvider : IODataSerializerProvider
    {
        public ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType, HttpContext context)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            IServiceProvider provider = context.RequestServices;

            switch (edmType.TypeKind())
            {
                case EdmTypeKind.Enum:
                    return provider.GetRequiredService<ODataEnumSerializer>();

                case EdmTypeKind.Primitive:
                    return provider.GetRequiredService<ODataPrimitiveSerializer>();

                case EdmTypeKind.Collection:
                    IEdmCollectionTypeReference collectionType = edmType.AsCollection();
                    if (collectionType.Definition.IsDeltaFeed())
                    {
                        return provider.GetRequiredService<ODataDeltaFeedSerializer>();
                    }
                    else if (collectionType.ElementType().IsEntity() || collectionType.ElementType().IsComplex())
                    {
                        return provider.GetRequiredService<ODataResourceSetSerializer>();
                    }
                    else
                    {
                        return provider.GetRequiredService<ODataCollectionSerializer>();
                    }

                case EdmTypeKind.Complex:
                case EdmTypeKind.Entity:
                    return provider.GetRequiredService<ODataResourceSerializer>();

                default:
                    return null;
            }
        }

        public ODataSerializer GetODataPayloadSerializer(Type type, HttpContext context)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            IServiceProvider provider = context.RequestServices;

            // handle the special types.
            if (type == typeof(ODataServiceDocument))
            {
                return provider.GetRequiredService<ODataServiceDocumentSerializer>();
            }
            else if (type == typeof(Uri) || type == typeof(ODataEntityReferenceLink))
            {
                return provider.GetRequiredService<ODataEntityReferenceLinkSerializer>();
            }
            else if (typeof(IEnumerable<Uri>).IsAssignableFrom(type) || type == typeof(ODataEntityReferenceLinks))
            {
                return provider.GetRequiredService<ODataEntityReferenceLinksSerializer>();
            }
            else if (type == typeof(ODataError) || type == typeof(HttpError))
            {
                return provider.GetRequiredService<ODataErrorSerializer>();
            }
            else if (typeof(IEdmModel).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                return provider.GetRequiredService<ODataMetadataSerializer>();
            }

            HttpRequest request = context.Request;

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
                    return provider.GetRequiredService<ODataRawValueSerializer>();
                }
                else
                {
                    return GetEdmTypeSerializer(edmType, context);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
