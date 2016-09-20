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
        /// <param name="edmType">The <see cref="IEdmTypeReference"/>.</param>
        /// <param name="context">.</param>
        /// <returns>The <see cref="ODataSerializer"/>.</returns>
        ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType, HttpContext context);

        /// <summary>
        /// Gets an <see cref="ODataSerializer"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which the serializer is being requested.</param>
        /// <param name="context">.</param>
        /// <returns>The <see cref="ODataSerializer"/> for the given type.</returns>
        ODataSerializer GetODataPayloadSerializer(Type type, HttpContext context);
    }
}
