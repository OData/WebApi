// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// Contains context information about the resource set currently being serialized.
    /// </summary>
    public class ResourceSetContext
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
        public object ResourceSetInstance { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belongs.
        /// </summary>
        public IEdmModel EdmModel
        {
            get
            {
                return Request.ODataProperties().Model;
            }
            set
            {
                Request.ODataProperties().Model = value;
            }
        }
    }
}
