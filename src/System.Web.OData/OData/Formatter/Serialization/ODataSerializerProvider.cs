// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// An ODataSerializerProvider is a factory for creating <see cref="ODataSerializer"/>s.
    /// </summary>
    public abstract class ODataSerializerProvider
    {
        /// <summary>
        /// Gets an <see cref="ODataEdmTypeSerializer"/> for the given edmType.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmTypeReference"/>.</param>
        /// <returns>The <see cref="ODataSerializer"/>.</returns>
        public abstract ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType);

        /// <summary>
        /// Gets an <see cref="ODataSerializer"/> for the given <paramref name="model"/> and <paramref name="type"/>.
        /// </summary>
        /// <param name="model">The EDM model associated with the request.</param>
        /// <param name="type">The <see cref="Type"/> for which the serializer is being requested.</param>
        /// <param name="request">The request for which the response is being serialized.</param>
        /// <returns>The <see cref="ODataSerializer"/> for the given type.</returns>
        public abstract ODataSerializer GetODataPayloadSerializer(IEdmModel model, Type type, HttpRequestMessage request);
    }
}
