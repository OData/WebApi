// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IRouteConstraint"/> that only matches OData paths.
    /// </summary>
    public class ODataPathRouteConstraint : IRouteConstraint
    {
        // "%2F"
        private static readonly string _escapedSlash = Uri.EscapeDataString("/");

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="routeName">The name of the route this constraint is associated with.</param>
        public ODataPathRouteConstraint(string routeName)
        {
            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            RouteName = routeName;
        }

        /// <summary>
        /// Gets the name of the route this constraint is associated with.
        /// </summary>
        public string RouteName
        {
            get;
            private set;
        }

        /// <summary>
        /// Determines whether the URL parameter contains a valid value for this constraint.
        /// </summary>
        /// <param name="httpContext">The Http context.</param>
        /// <param name="route">The route to compare.</param>
        /// <param name="routeKey">The name of the parameter.</param>
        /// <param name="values">A list of parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns>
        /// True if this instance equals a specified route; otherwise, false.
        /// </returns>
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            throw new NotImplementedException();
        }
    }
}
