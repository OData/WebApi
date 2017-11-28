// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProviderProxy"/>.
    /// </summary>
    internal partial class ODataDeserializerProviderProxy
    {
        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequestMessage request)
        {
            return RequestContainer.GetRequiredService<ODataDeserializerProvider>()
                .GetODataDeserializer(type, request);
        }
    }
}
