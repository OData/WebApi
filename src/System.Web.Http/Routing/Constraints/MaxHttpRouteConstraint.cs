// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter to be a long with a maximum value.
    /// </summary>
    public class MaxHttpRouteConstraint : IHttpRouteConstraint
    {
        public MaxHttpRouteConstraint(long max)
        {
            Max = max;
        }

        /// <summary>
        /// Maximum value of the parameter.
        /// </summary>
        public long Max { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            object value = values[parameterName];
            if (value == null)
            {
                return true;
            }

            long longValue;
            if (!Int64.TryParse(value.ToString(), out longValue))
            {
                return false;
            }

            return longValue <= Max;
        }
    }
}