// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// This class encapsulates the state and settings that get passed to <see cref="ODataDeserializer"/>
    /// from the <see cref="ODataInputFormatter"/>.
    /// </summary>
    public partial class ODataDeserializerContext
    {
        private HttpRequest _request;

        /// <summary>
        /// Gets or sets the HTTP Request that is being deserialized.
        /// </summary>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public HttpRequest Request
        {
            get { return _request; }
            set
            {
                _request = value;
                WebApiRequestMessage webApiRequestMessage = _request != null ? new WebApiRequestMessage(_request) : null;
                InternalRequest = webApiRequestMessage;
                InternalUrlHelper = _request != null ? new WebApiUrlHelper(_request.GetUrlHelper()) : null;

                // We add this setting via CompatibilityOptions
                CompatibilityOptions options = webApiRequestMessage != null ? webApiRequestMessage.RequestContainer.GetService<ODataOptions>().CompatibilityOptions : CompatibilityOptions.None;
                DisableCaseInsensitiveRequestPropertyBinding = options.HasFlag(CompatibilityOptions.DisableCaseInsensitiveRequestPropertyBinding) ? true : false;
            }
        }
    }
}
