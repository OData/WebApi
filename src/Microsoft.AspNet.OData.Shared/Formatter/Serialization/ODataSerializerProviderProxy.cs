// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    /// <remarks>
    /// This class is used to delay load the ODataSerializerProvider from
    /// the service container. The proxy is used by the formatter, which is
    /// created outside of the container and therefore can’t use the 
    /// container directly. At run-time, the container from the active
    /// request is saved on a property on the proxy before the formatter
    /// asks for the ODataSerializerProvider; once it does ask, the proxy
    /// loads the ODataSerializerProvider from services container.
    /// </remarks>
    internal partial class ODataSerializerProviderProxy : ODataSerializerProvider
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
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            return RequestContainer.GetRequiredService<ODataSerializerProvider>().GetEdmTypeSerializer(edmType);
        }
    }
}
