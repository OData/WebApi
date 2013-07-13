// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;

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
        /// <param name="actions">The actions to invoke for the route.</param>
        /// <returns>The generated <see cref="IHttpRoute"/>.</returns>
        public virtual IHttpRoute BuildHttpRoute(
            string routeTemplate,
            IEnumerable<HttpMethod> httpMethods,
            IEnumerable<ReflectedHttpActionDescriptor> actions)
        {
            if (routeTemplate == null)
            {
                throw Error.ArgumentNull("routeTemplate");
            }

            HttpRouteValueDictionary defaults = new HttpRouteValueDictionary();
            HttpRouteValueDictionary constraints = new HttpRouteValueDictionary();
            if (httpMethods != null)
            {
                // Current method constraint implementation is inefficient since it matches before running the constraint.
                // Consider checking the HTTP method first in a custom route as a performance optimization.
                constraints["httpMethod"] = new HttpMethodConstraint(httpMethods.AsArray());
            }

            string detokenizedRouteTemplate = InlineRouteTemplateParser.ParseRouteTemplate(routeTemplate, defaults, constraints, ConstraintResolver);

            return BuildHttpRoute(detokenizedRouteTemplate, defaults, constraints, actions);
        }

        /// <summary>
        /// Builds an <see cref="IHttpRoute"/>.
        /// </summary>
        /// <param name="routeTemplate">The detokenized route template.</param>
        /// <param name="defaults">The route defaults.</param>
        /// <param name="constraints">The route constraints.</param>
        /// <param name="actions">The actions to invoke for the route.</param>
        /// <returns>The generated <see cref="IHttpRoute"/>.</returns>
        public virtual IHttpRoute BuildHttpRoute(
            string routeTemplate,
            HttpRouteValueDictionary defaults,
            HttpRouteValueDictionary constraints,
            IEnumerable<ReflectedHttpActionDescriptor> actions)
        {
            return new HttpDirectRoute(routeTemplate, defaults, constraints, actions);
        }
    }
}
