//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// An ODataSerializerProvider is a factory for creating <see cref="ODataSerializer"/>s.
    /// </summary>
    public abstract partial class ODataSerializerProvider
    {
        /// <summary>
        /// Gets an <see cref="ODataSerializer"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> for which the serializer is being requested.</param>
        /// <param name="request">The request for which the response is being serialized.</param>
        /// <returns>The <see cref="ODataSerializer"/> for the given type.</returns>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public abstract ODataSerializer GetODataPayloadSerializer(Type type, HttpRequestMessage request);
    }
}
