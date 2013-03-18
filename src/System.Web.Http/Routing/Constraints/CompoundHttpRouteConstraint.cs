// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a route by several child constraints.
    /// </summary>
    public class CompoundHttpRouteConstraint : IHttpRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundHttpRouteConstraint" /> class.
        /// </summary>
        /// <param name="constraints">The child constraints that must match for this constraint to match.</param>
        public CompoundHttpRouteConstraint(IList<IHttpRouteConstraint> constraints)
        {
            if (constraints == null)
            {
                throw Error.ArgumentNull("constraints");
            }

            Constraints = constraints;
        }

        /// <summary>
        /// Gets the child constraints that must match for this constraint to match.
        /// </summary>
        public IEnumerable<IHttpRouteConstraint> Constraints { get; private set; }

        /// <inheritdoc />
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            foreach (IHttpRouteConstraint constraint in Constraints)
            {
                if (!constraint.Match(request, route, parameterName, values, routeDirection))
                {
                    return false;
                }
            }
            return true;
        }
    }
}