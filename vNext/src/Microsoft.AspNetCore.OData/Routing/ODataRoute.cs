// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
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
        /// <param name="target"></param>
        /// <param name="routePrefix"></param>
        /// <param name="constraint"></param>
        /// <param name="resolver"></param>
        public ODataRoute(IRouter target, string routePrefix, ODataRouteConstraint constraint, IInlineConstraintResolver resolver)
            : base(target, GetRouteTemplate(routePrefix), inlineConstraintResolver: resolver)
        {
            Constraints.Add(ODataRouteConstants.ConstraintName, constraint);
        }

        private static string GetRouteTemplate(string prefix)
        {
            return String.IsNullOrEmpty(prefix) ?
                ODataRouteConstants.ODataPathTemplate :
                prefix + '/' + ODataRouteConstants.ODataPathTemplate;
        }
    }
}
