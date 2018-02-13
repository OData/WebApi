// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProviderProxy"/>.
    /// </summary>
    /// <remarks>
    /// This class is used to delay load the ODataDeserializerProvider from
    /// the service container. The proxy is used by the formatter, which is
    /// created outside of the container and therefore can’t use the 
    /// container directly. At run-time, the container from the active
    /// request is saved on a property on the proxy before the formatter
    /// asks for the ODataDeserializerProvider; once it does ask, the proxy
    /// loads the ODataDeserializerProvider from services container.
    /// </remarks>
    internal partial class ODataDeserializerProviderProxy : ODataDeserializerProvider
    {
        private IServiceProvider _requestContainer;

        public IServiceProvider RequestContainer
        {
            get { return _requestContainer; }
            set
            {
                Contract.Assert(_requestContainer == null, "Cannot set request container twice.");

                _requestContainer = value;
            }
        }

        /// <inheritdoc />
        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            return RequestContainer.GetRequiredService<ODataDeserializerProvider>()
                .GetEdmTypeDeserializer(edmType);
        }
    }
}
