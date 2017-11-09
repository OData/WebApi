// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    internal partial class ODataSerializerProviderProxy : ODataSerializerProvider
    {
        /// <inheritdoc />
        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
        {
            return RequestContainer.GetRequiredService<ODataSerializerProvider>()
                .GetODataPayloadSerializer(type, request);
        }
    }
}
