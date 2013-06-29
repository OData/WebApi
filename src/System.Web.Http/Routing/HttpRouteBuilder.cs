// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace System.Web.Http.Routing
{
    /// <summary>
    /// Builds <see cref="IHttpRoute"/> instances based on route information.
    /// </summary>
    public class HttpRouteBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRouteBuilder" /> class using the default inline constraint resolver.
        /// </summary>
        public HttpRouteBuilder()
            : this(new DefaultInlineConstraintResolver())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRouteBuilder" /> class.
        /// </summary>
        /// <param name="constraintResolver">The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints.</param>
        public HttpRouteBuilder(IInlineConstraintResolver constraintResolver)
        {
            if (constraintResolver == null)
            {
                throw Error.ArgumentNull("constraintResolver");
            }

            ConstraintResolver = constraintResolver;
        }

        public IInlineConstraintResolver ConstraintResolver { get; private set; }

        /// <summary>
        /// Builds an <see cref="IHttpRoute"/> for a particular action.
        /// </summary>
        /// <param name="routeTemplate">The tokenized route template for the route.</param>
        /// <param name="httpMethods">The HTTP methods supported by the route.</param>
        /// <param name="controllerName">The name of the associated controller.</param>
        /// <param name="actionName">The name of the associated action.</param>
        /// <returns>The generated <see cref="IHttpRoute"/>.</returns>
        public virtual IHttpRoute BuildHttpRoute(string routeTemplate, IEnumerable<HttpMethod> httpMethods, string controllerName, string actionName)
        {
            if (routeTemplate == null)
            {
                throw Error.ArgumentNull("routeTemplate");
            }

            if (controllerName == null)
            {
                throw Error.ArgumentNull("controllerName");
            }

            if (actionName == null)
            {
                throw Error.ArgumentNull("actionName");
            }

            HttpRouteValueDictionary defaults = new HttpRouteValueDictionary
            {
                { RouteKeys.ControllerKey, controllerName },
                { RouteKeys.ActionKey, actionName }
            };

            HttpRouteValueDictionary constraints = new HttpRouteValueDictionary();
            if (httpMethods != null)
            {
                // Current method constraint implementation is inefficient since it matches before running the constraint.
                // Consider checking the HTTP method first in a custom route as a performance optimization.
                constraints.Add("httpMethod", new HttpMethodConstraint(httpMethods.ToArray()));
            }

            string detokenizedRouteTemplate = InlineRouteTemplateParser.ParseRouteTemplate(routeTemplate, defaults, constraints, ConstraintResolver);

            return BuildHttpRoute(defaults, constraints, detokenizedRouteTemplate);
        }

        /// <summary>
        /// Builds an <see cref="IHttpRoute"/>.
        /// </summary>
        /// <param name="defaults">The route defaults.</param>
        /// <param name="constraints">The route constraints.</param>
        /// <param name="routeTemplate">The detokenized route template.</param>
        /// <returns>The generated <see cref="IHttpRoute"/>.</returns>
        public virtual IHttpRoute BuildHttpRoute(HttpRouteValueDictionary defaults, HttpRouteValueDictionary constraints, string routeTemplate)
        {
            return new HttpRoute(routeTemplate, defaults, constraints);
        }
    }
}
