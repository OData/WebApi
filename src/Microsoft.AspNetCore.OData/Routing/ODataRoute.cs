// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// A route implementation for OData routes. It supports passing in a route prefix for the route as well
    /// as a path constraint that parses the request path as OData.
    /// </summary>
    public class ODataRoute : Route
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute"/> class.
        /// </summary>
        /// <param name="target">The target router.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="routeConstraint">The OData route constraint.</param>
        /// <param name="resolver">The inline constraint resolver.</param>
        public ODataRoute(IRouter target, string routeName, string routePrefix, ODataPathRouteConstraint routeConstraint, IInlineConstraintResolver resolver)
            : this(target, routeName, routePrefix, (IRouteConstraint)routeConstraint, resolver)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRoute"/> class.
        /// </summary>
        /// <param name="target">The target router.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="routeConstraint">The OData route constraint.</param>
        /// <param name="resolver">The inline constraint resolver.</param>
        public ODataRoute(IRouter target, string routeName, string routePrefix, IRouteConstraint routeConstraint, IInlineConstraintResolver resolver)
            : base(target, routeName, GetRouteTemplate(routePrefix), defaults: null, constraints: null, dataTokens: null, inlineConstraintResolver: resolver)
        {
            throw new NotImplementedException();
        }

        private static string GetRouteTemplate(string prefix)
        {
            return String.IsNullOrEmpty(prefix) ?
                ODataRouteConstants.ODataPathTemplate :
                prefix + '/' + ODataRouteConstants.ODataPathTemplate;
        }
    }
}