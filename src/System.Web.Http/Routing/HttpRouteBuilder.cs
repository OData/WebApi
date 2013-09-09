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
    internal class HttpRouteBuilder
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
        /// <param name="order">The subroute order.</param>
        /// <param name="actions">The actions to invoke for the route.</param>
        /// <returns>The generated <see cref="IHttpRoute"/>.</returns>
        public virtual IHttpRoute BuildParsingRoute(
            string routeTemplate,
            int order,
            IEnumerable<ReflectedHttpActionDescriptor> actions)
        {
            if (routeTemplate == null)
            {
                throw Error.ArgumentNull("routeTemplate");
            }

            HttpRouteValueDictionary defaults = new HttpRouteValueDictionary();
            HttpRouteValueDictionary constraints = new HttpRouteValueDictionary();

            string detokenizedRouteTemplate = InlineRouteTemplateParser.ParseRouteTemplate(routeTemplate, defaults, constraints, ConstraintResolver);

            return BuildParsingRoute(detokenizedRouteTemplate, order, defaults, constraints, actions);
        }

        /// <summary>
        /// Builds an <see cref="IHttpRoute"/>.
        /// </summary>
        /// <param name="routeTemplate">The detokenized route template.</param>
        /// <param name="order">The subroute order.</param>
        /// <param name="defaults">The route defaults.</param>
        /// <param name="constraints">The route constraints.</param>
        /// <param name="actions">The actions to invoke for the route.</param>
        /// <returns>The generated <see cref="IHttpRoute"/>.</returns>
        public virtual IHttpRoute BuildParsingRoute(
            string routeTemplate,
            int order,
            HttpRouteValueDictionary defaults,
            HttpRouteValueDictionary constraints,
            IEnumerable<ReflectedHttpActionDescriptor> actions)
        {
            return BuildDirectRoute(routeTemplate, order, defaults, constraints, actions);
        }

        public virtual IHttpRoute BuildGenerationRoute(IHttpRoute parsingRoute)
        {
            return new GenerateRoute(parsingRoute);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRoute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template.</param>
        /// <param name="order">The subroute order.</param>
        /// <param name="actions">The actions that are reachable via this route.</param>
        public static HttpRoute BuildDirectRoute(string routeTemplate, int order, IEnumerable<ReflectedHttpActionDescriptor> actions)            
        {
            return BuildDirectRoute(routeTemplate, order, defaults: null, constraints: null, actions: actions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRoute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template.</param>
        /// <param name="order">The subroute order.</param>
        /// <param name="defaults">The default values.</param>
        /// <param name="constraints">The route constraints.</param>
        /// <param name="actions">The actions that are reachable via this route.</param>
        public static HttpRoute BuildDirectRoute(
            string routeTemplate,
            int order,
            HttpRouteValueDictionary defaults,
            HttpRouteValueDictionary constraints,
            IEnumerable<ReflectedHttpActionDescriptor> actions)
        {
            HttpRoute route = new HttpRoute(routeTemplate, defaults: defaults, constraints: constraints, dataTokens: null, handler: null);

            if (actions != null)
            {
                route.DataTokens[RouteKeys.OrderDataTokenKey] = order;
                route.DataTokens[RouteKeys.PrecedenceDataTokenKey] = route.ParsedRoute.GetPrecedence(constraints);
                route.DataTokens[RouteKeys.ActionsDataTokenKey] = actions.AsArray();
            }
            
            return route;
        }
    }
}
