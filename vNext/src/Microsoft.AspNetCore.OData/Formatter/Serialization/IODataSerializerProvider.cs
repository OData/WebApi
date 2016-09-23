// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// An serializer factory for creating <see cref="ODataSerializer"/>s.
    /// </summary>
    public interface IODataSerializerProvider
    {
        /// <summary>
        /// Gets an <see cref="ODataEdmTypeSerializer"/> for the given edmType.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="edmType">The <see cref="IEdmTypeReference"/>.</param>
        /// <returns>The <see cref="ODataSerializer"/>.</returns>
        ODataEdmTypeSerializer GetEdmTypeSerializer(HttpContext context, IEdmTypeReference edmType);

        /// <summary>
        /// Gets an <see cref="ODataSerializer"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="type">The <see cref="Type"/> for which the serializer is being requested.</param>
        /// <returns>The <see cref="ODataSerializer"/> for the given type.</returns>
        ODataSerializer GetODataPayloadSerializer(HttpContext context, Type type);
    }
}
