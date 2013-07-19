// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Defines a base class for specifying that an action supports a particular HTTP method.
    /// </summary>
#pragma warning disable 3015 // This is an abstract base attribute that doesn't need an accessible constructor
    public abstract class HttpVerbAttribute : Attribute, IActionHttpMethodProvider, IHttpRouteInfoProvider
#pragma warning restore 3015
    {
        private readonly Collection<HttpMethod> _supportedMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbAttribute" /> class.
        /// </summary>
        /// <param name="httpMethod">The HTTP method the action supports.</param>
        protected HttpVerbAttribute(HttpMethod httpMethod)
        {
            _supportedMethods = new Collection<HttpMethod>() { httpMethod };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbAttribute" /> class.
        /// </summary>
        /// <param name="httpMethod">The HTTP method the action supports.</param>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        protected HttpVerbAttribute(HttpMethod httpMethod, string routeTemplate)
            : this(httpMethod)
        {
            RouteTemplate = routeTemplate;
        }

        /// <summary>
        /// Gets the HTTP methods the action supports.
        /// </summary>
        public Collection<HttpMethod> HttpMethods
        {
            get
            {
                return _supportedMethods;
            }
        }

        /// <inheritdoc />
        public string RouteName { get; set; }

        /// <inheritdoc />
        public int RouteOrder { get; set; }

        /// <inheritdoc />
        public string RouteTemplate { get; private set; }

        IEnumerable<HttpMethod> IHttpRouteInfoProvider.HttpMethods
        {
            get
            {
                return _supportedMethods;
            }
        }
    }
}
