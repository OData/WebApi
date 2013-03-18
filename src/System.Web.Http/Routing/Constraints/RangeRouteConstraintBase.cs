// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constraints a route parameter to be an integer within a given range of values.
    /// </summary>
    public class RangeHttpRouteConstraint : IHttpRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RangeHttpRouteConstraint" /> class.
        /// </summary>
        /// <param name="min">The minimum value of the route parameter.</param>
        /// <param name="max">The maximum value of the route parameter.</param>
        public RangeHttpRouteConstraint(long min, long max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Gets the minimum value of the route parameter.
        /// </summary>
        public long Min { get; private set; }

        /// <summary>
        /// Gets the maximum value of the route parameter.
        /// </summary>
        public long Max { get; private set; }

        /// <inheritdoc />
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (parameterName == null)
            {
                throw Error.ArgumentNull("parameterName");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            object value;
            if (values.TryGetValue(parameterName, out value) && value != null)
            {
                long longValue;
                if (value is long)
                {
                    longValue = (long)value;
                    return longValue >= Min && longValue <= Max;
                }

                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (Int64.TryParse(valueString, NumberStyles.Integer, CultureInfo.InvariantCulture, out longValue))
                {
                    return longValue >= Min && longValue <= Max;
                }
            }
            return false;
        }
    }
}