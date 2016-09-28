// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="IODataSerializerProvider"/>.
    /// </summary>
    public class DefaultODataSerializerProvider : IODataSerializerProvider
    {
        /// <inheritdoc />
        public ODataEdmTypeSerializer GetEdmTypeSerializer(HttpContext context, IEdmTypeReference edmType)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

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

        /// <inheritdoc />
        public ODataSerializer GetODataPayloadSerializer(HttpContext context, Type type)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (type == null)
            {
                throw Error.ArgumentNull("type");
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
            else if (type == typeof(ODataError) || type == typeof(SerializableError))
            {
                return provider.GetRequiredService<ODataErrorSerializer>();
            }
            else if (typeof(IEdmModel).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
            {
                return provider.GetRequiredService<ODataMetadataSerializer>();
            }

            // if it is not a special type, assume it has a corresponding EdmType.
            IEdmModel model = context.ODataFeature().Model;
            ClrTypeCache typeMappingCache = model.GetTypeMappingCache();
            IEdmTypeReference edmType = typeMappingCache.GetEdmType(type, model);

            if (edmType != null)
            {
                if (((edmType.IsPrimitive() || edmType.IsEnum()) &&
                    ODataRawValueMediaTypeMapping.IsRawValueRequest(context)) ||
                    ODataCountMediaTypeMapping.IsCountRequest(context))
                {
                    return provider.GetRequiredService<ODataRawValueSerializer>();
                }
                else
                {
                    return GetEdmTypeSerializer(context, edmType);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
