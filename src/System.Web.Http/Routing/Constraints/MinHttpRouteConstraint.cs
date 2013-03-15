// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter to be a long with a minimum value.
    /// </summary>
    public class MinHttpRouteConstraint : IHttpRouteConstraint, IInlineRouteConstraint
    {
        public MinHttpRouteConstraint(string min)
        {
            var parsedMin = min.ParseLong();
            if (!parsedMin.HasValue)
            {
                throw new ArgumentOutOfRangeException("min", min);
            }

            Min = parsedMin.Value;
        }

        /// <summary>
        /// Minimum value of the parameter.
        /// </summary>
        public long Min { get; private set; }

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

            return parsedValue.Value >= Min;
        }
    }
}