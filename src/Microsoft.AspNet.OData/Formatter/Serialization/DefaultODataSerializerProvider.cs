// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public partial class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        /// <inheritdoc />
        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequestMessage request)
        {
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
            IEdmModel model = request.GetModel();
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
