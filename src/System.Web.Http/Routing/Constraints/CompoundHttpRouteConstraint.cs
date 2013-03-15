// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    public class CompoundHttpRouteConstraint : IHttpRouteConstraint
    {
        public CompoundHttpRouteConstraint(IEnumerable<IHttpRouteConstraint> constraints)
        {
            Constraints = constraints;
        }

        public IEnumerable<IHttpRouteConstraint> Constraints { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            return Constraints.All(c => c.Match(request, route, parameterName, values, routeDirection));
        }
    }
}