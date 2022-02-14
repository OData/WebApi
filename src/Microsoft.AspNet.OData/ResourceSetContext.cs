//-----------------------------------------------------------------------------
// <copyright file="ResourceSetContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Contains context information about the resource set currently being serialized.
    /// </summary>
    public partial class ResourceSetContext
    {
        private HttpRequestMessage _request;
        private UrlHelper _urlHelper;

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
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

        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public HttpRequestContext RequestContext { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to be used for generating links while serializing this
        /// feed instance.
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
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belongs.
        /// </summary>
        /// <remarks>This function uses types that are AspNet-specific.</remarks>
        public IEdmModel EdmModel
        {
            get { return Request.GetModel(); }
        }

        /// <summary>
        /// Create a <see cref="ResourceSetContext"/> from an <see cref="ODataSerializerContext"/> and <see cref="IEnumerable"/>.
        /// </summary>
        /// <param name="resourceSetInstance">The instance representing the resource set being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>A new <see cref="ResourceSetContext"/>.</returns>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        internal static ResourceSetContext Create(ODataSerializerContext writeContext, IEnumerable resourceSetInstance)
        {
            ResourceSetContext resourceSetContext = new ResourceSetContext
            {
                Request = writeContext.Request,
                EntitySetBase = writeContext.NavigationSource as IEdmEntitySetBase,
                Url = writeContext.Url,
                ResourceSetInstance = resourceSetInstance
            };

            return resourceSetContext;
        }
    }
}
