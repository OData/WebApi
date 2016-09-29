// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.OData.Routing
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
        /// <param name="routePrefix">The route prefix.</param>
        /// <param name="constraint">The OData route constraint.</param>
        /// <param name="resolver">The inline constraint resolver.</param>
        public ODataRoute(IRouter target, string routePrefix, ODataRouteConstraint constraint, IInlineConstraintResolver resolver)
            : base(target, GetRouteTemplate(routePrefix), inlineConstraintResolver: resolver)
        {
            if (constraint == null)
            {
                throw Error.ArgumentNull("constraint");
            }

            RoutePrefix = routePrefix;
            RouteConstraint = constraint;
            Constraints.Add(ODataRouteConstants.ConstraintName, constraint);
        }

        /// <summary>
        /// Gets the route prefix.
        /// </summary>
        public string RoutePrefix { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataRouteConstraint"/> on this route.
        /// </summary>
        public ODataRouteConstraint RouteConstraint { get; private set; }

        private static string GetRouteTemplate(string prefix)
        {
            return String.IsNullOrEmpty(prefix) ?
                ODataRouteConstants.ODataPathTemplate :
                prefix + '/' + ODataRouteConstants.ODataPathTemplate;
        }
    }
}
