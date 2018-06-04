// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProvider"/>.
    /// </summary>
    public partial class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequestMessage request)
        {
            // Using a Func<IEdmModel> to delay evaluation of the model.
            return GetODataDeserializerImpl(type, () => request.GetModel());
        }
    }
}
