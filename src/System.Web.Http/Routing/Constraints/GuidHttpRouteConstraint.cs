// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    public class GuidHttpRouteConstraint : IHttpRouteConstraint
    {
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            object value = values[parameterName];
            if (value == null)
            {
                return false;
            }

            if (value is Guid)
            {
                return true;
            }

            Guid result;
            return Guid.TryParse(value.ToString(), out result);
        }
    }
}