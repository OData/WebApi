// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    internal class ODataSerializerProviderProxy : ODataSerializerProvider
    {
        private static readonly ODataSerializerProviderProxy _instance = new ODataSerializerProviderProxy();

        private IServiceProvider _requestContainer;

        /// <summary>
        /// Gets the default instance of the <see cref="ODataSerializerProviderProxy"/>.
        /// </summary>
        public static ODataSerializerProviderProxy Instance
        {
            get
            {
                return _instance;
            }
        }

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

        /// <inheritdoc />
        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequestMessage request)
        {
            return RequestContainer.GetRequiredService<ODataSerializerProvider>()
                .GetODataPayloadSerializer(type, request);
        }
    }
}
