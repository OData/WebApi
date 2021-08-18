//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Adapters;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the <see cref="ODataSerializer"/> when serializing objects in OData message format.
    /// </summary>
    public partial class ODataSerializerContext
    {
        private HttpRequestMessage _request;
        private UrlHelper _urlHelper;

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
        /// </summary>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public HttpRequestMessage Request
        {
            get { return _request; }

            set
            {
                _request = value;
                InternalRequest = _request != null ? new WebApiRequestMessage(_request) : null;
            }
        }

        /// <summary>Gets or sets the request context.</summary>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public HttpRequestContext RequestContext { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UrlHelper"/> to use for generating OData links.
        /// </summary>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public UrlHelper Url
        {
            get { return _urlHelper; }

            set
            {
                _urlHelper = value;
                InternalUrlHelper = _urlHelper != null ? new WebApiUrlHelper(_urlHelper) : null;
            }
        }

        /// <summary>
        /// Copy the properties this instance of <see cref="ODataSerializerContext"/> from an existing instance.
        /// </summary>
        /// <param name="context"></param>
        /// <remarks>This function uses types that are AspNet-specific.</remarks>
        private void CopyPlatformSpecificProperties(ODataSerializerContext context)
        {
            Request = context.Request;
            Url = context.Url;
            // TODO: This property is not copied in the Aspnet version of the product.
            //RequestContext = context.RequestContext;
        }
    }
}
