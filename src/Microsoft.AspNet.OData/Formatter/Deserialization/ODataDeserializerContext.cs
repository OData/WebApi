//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// This class encapsulates the state and settings that get passed to <see cref="ODataDeserializer"/>
    /// from the <see cref="ODataMediaTypeFormatter"/>.
    /// </summary>
    public partial class ODataDeserializerContext
    {
        private HttpRequestMessage _request;

        /// <summary>
        /// Gets or sets the HTTP Request that is being deserialized.
        /// </summary>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public HttpRequestMessage Request
        {
            get { return _request; }
            set
            {
                _request = value;
                WebApiRequestMessage webApiRequestMessage = _request != null ? new WebApiRequestMessage(_request) : null;
                InternalRequest = webApiRequestMessage;
                InternalUrlHelper = _request != null ? new WebApiUrlHelper(_request.GetUrlHelper()) : null;

                // We add this setting via CompatibilityOptions
                CompatibilityOptions options = webApiRequestMessage != null ? webApiRequestMessage.Configuration.GetCompatibilityOptions() : CompatibilityOptions.None;
                DisableCaseInsensitiveRequestPropertyBinding = options.HasOption(CompatibilityOptions.DisableCaseInsensitiveRequestPropertyBinding);
            }
        }

        /// <summary>Gets or sets the request context.</summary>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public HttpRequestContext RequestContext { get; set; }
    }
}
