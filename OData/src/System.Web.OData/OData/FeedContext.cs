// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// Contains context information about the feed currently being serialized.
    /// </summary>
    public class FeedContext
    {
        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        public HttpRequestContext RequestContext { get; set; }

        /// <summary>
        /// Gets the <see cref="IEdmEntitySetBase"/> this instance belongs to.
        /// </summary>
        public IEdmEntitySetBase EntitySetBase { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UrlHelper"/> to be used for generating links while serializing this
        /// feed instance.
        /// </summary>
        public UrlHelper Url { get; set; }

        /// <summary>
        /// Gets the value of this feed instance.
        /// </summary>
        public object FeedInstance { get; set; }
    }
}
