// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents a factory that creates an <see cref="ODataDeserializer"/>.
    /// </summary>
    public abstract partial class ODataDeserializerProvider
    {
        /// <summary>
        /// Gets an <see cref="ODataDeserializer"/> for the given type.
        /// </summary>
        /// <param name="type">The CLR type.</param>
        /// <param name="request">The request being deserialized.</param>
        /// <returns>An <see cref="ODataDeserializer"/> that can deserialize the given type.</returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public abstract ODataDeserializer GetODataDeserializer(Type type, HttpRequest request);
    }
}
