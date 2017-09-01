// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
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
        public HttpRequestContext RequestContext { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to be used for generating links while serializing this
        /// feed instance.
        /// </summary>
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
        public IEdmModel EdmModel
        {
            get { return Request.GetModel(); }
        }

        /// <summary>
        /// Gets or sets the HTTP request that causes this instance to be generated.
        /// </summary>
        internal IWebApiRequestMessage InternalRequest { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to be used for generating links while serializing this
        /// feed instance.
        /// </summary>
        internal IWebApiUrlHelper InternalUrlHelper { get; private set; }
    }
}
