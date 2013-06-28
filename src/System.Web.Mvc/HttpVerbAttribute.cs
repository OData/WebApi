// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    /// <summary>
    /// Defines a base class for specifying that an action supports particular HTTP methods.
    /// </summary>
    public abstract class HttpVerbAttribute : HttpVerbsRoutingAttribute, IDirectRouteInfoProvider
    {
        private readonly string _routeTemplate;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbAttribute" /> class.
        /// </summary>
        /// <param name="httpMethods">The HTTP methods the action supports.</param>
        protected HttpVerbAttribute(HttpVerbs httpMethods)
            : base(httpMethods)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbAttribute" /> class.
        /// </summary>
        /// <param name="httpMethods">The HTTP methods the action supports.</param>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        protected HttpVerbAttribute(HttpVerbs httpMethods, string routeTemplate)
            : base(httpMethods)
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