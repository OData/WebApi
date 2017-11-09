// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProviderProxy"/>.
    /// </summary>
    internal class ODataDeserializerProviderProxy : ODataDeserializerProvider
    {
        public IServiceProvider RequestContainer
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
