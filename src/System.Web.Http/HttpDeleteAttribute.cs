// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Specifies that an action supports the DELETE HTTP method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpDeleteAttribute : Attribute, IHttpRouteInfoProvider
    {
        private static readonly Collection<HttpMethod> _supportedMethods = new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Delete });

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeleteAttribute" /> class.
        /// </summary>
        public HttpDeleteAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpDeleteAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public HttpDeleteAttribute(string routeTemplate)
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

        /// <summary>
        /// Gets or sets the name of the route to generate for this action.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Gets the route template describing the URI pattern to match against.
        /// </summary>
        public string RouteTemplate { get; private set; }
    }
}
