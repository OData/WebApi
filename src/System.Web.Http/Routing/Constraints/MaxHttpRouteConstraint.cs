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
        public MaxHttpRouteConstraint(string max)
        {
            var parsedMax = max.ParseLong();
            if (!parsedMax.HasValue)
            {
                throw new ArgumentOutOfRangeException("max", max);
            }

            Max = parsedMax.Value;
        }

        /// <summary>
        /// Maximum value of the parameter.
        /// </summary>
        public long Max { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
            {
                return true;
            }

            var parsedValue = value.ParseLong();
            if (!parsedValue.HasValue)
            {
                return false;
            }

            return parsedValue.Value <= Max;
        }
    }
}