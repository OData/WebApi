// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    /// <summary>
    /// Represents a route for an action method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpRouteAttribute : HttpVerbsRoutingAttribute, IDirectRouteInfoProvider
    {
        private readonly string _routeTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRouteAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public HttpRouteAttribute(string routeTemplate)
        {
            ValidateRouteTemplateArgument(routeTemplate);
            _routeTemplate = routeTemplate;
        }

        /// <inheritdoc />
        public string RouteTemplate
        {
            get { return _routeTemplate; }
        }
    }
}