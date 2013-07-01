// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    /// <summary>
    /// Defines a base class for specifying that an action supports particular HTTP methods.
    /// </summary>
    public abstract class HttpVerbAttribute : ActionMethodSelectorAttribute, IDirectRouteInfoProvider
    {
        private readonly string _routeTemplate;
        private readonly HttpVerbsValidator _httpVerbsValidator;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbAttribute" /> class.
        /// </summary>
        /// <param name="httpMethods">The HTTP methods the action supports.</param>
        protected HttpVerbAttribute(HttpVerbs httpMethods)
        {
            _httpVerbsValidator = new HttpVerbsValidator(httpMethods);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpVerbAttribute" /> class.
        /// </summary>
        /// <param name="httpMethods">The HTTP methods the action supports.</param>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        protected HttpVerbAttribute(HttpVerbs httpMethods, string routeTemplate)
        {
            _httpVerbsValidator = new HttpVerbsValidator(httpMethods);
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
            get { return _httpVerbsValidator.Verbs; }
        }

        /// <inheritdoc />
        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            return _httpVerbsValidator.IsValidForRequest(controllerContext);
        }
    }
}