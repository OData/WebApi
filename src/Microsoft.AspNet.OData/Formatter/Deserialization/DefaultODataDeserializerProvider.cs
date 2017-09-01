// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProvider"/>.
    /// </summary>
    public partial class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        /// <inheritdoc />
        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequestMessage request)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            if (type == typeof(Uri))
            {
                return _rootContainer.GetRequiredService<ODataEntityReferenceLinkDeserializer>();
            }

            if (type == typeof(ODataActionParameters) || type == typeof(ODataUntypedActionParameters))
            {
                return _rootContainer.GetRequiredService<ODataActionPayloadDeserializer>();
            }

            IEdmModel model = request.GetModel();
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
