// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    public class OptionalHttpRouteConstraint : IHttpRouteConstraint
    {
        public OptionalHttpRouteConstraint(IHttpRouteConstraint innerConstraint)
        {
            InnerConstraint = innerConstraint;
        }

        public IHttpRouteConstraint InnerConstraint { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            // If the param is optional and has no value, then pass the constraint
            if (route.Defaults.ContainsKey(parameterName) && route.Defaults[parameterName] == RouteParameter.Optional)
            {
                if (values[parameterName] == RouteParameter.Optional)
                {
                    return true;
                }
            }

            return InnerConstraint.Match(request, route, parameterName, values, routeDirection);
        }
    }
}