// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    /// <summary>
    /// Represents a route for an action method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpRouteAttribute : Attribute, IDirectRouteInfoProvider
    {
        private readonly string _routeTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRouteAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public HttpRouteAttribute(string routeTemplate)
        {
            _routeTemplate = routeTemplate;
        }

        /// <inheritdoc />
        public string RouteTemplate
        {
            get { return _routeTemplate; }
        }

        /// <summary>
        /// Gets or sets the name of the route to generate for this action.
        /// </summary>
        public string RouteName { get; set; }

        /// <summary>
        /// Gets or sets the order of the route relative to other routes. The default order is 0.
        /// </summary>
        public int RouteOrder { get; set; }

        /// <inheritdoc />
        public ICollection<string> Verbs
        {
            get { return null; }
        }
    }
}