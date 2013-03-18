// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to be a string with a maximum length.
    /// </summary>
    public class MinLengthHttpRouteConstraint : IHttpRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MinLengthHttpRouteConstraint" /> class.
        /// </summary>
        /// <param name="minLength">The minimum length of the route parameter.</param>
        public MinLengthHttpRouteConstraint(int minLength)
        {
            if (minLength < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("minLength", minLength, 0);
            }

            MinLength = minLength;
        }

        /// <summary>
        /// Gets the minimum length of the route parameter.
        /// </summary>
        public int MinLength { get; private set; }

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
                string valueString = Convert.ToString(value, CultureInfo.InvariantCulture);
                return valueString.Length >= MinLength;
            }
            return false;
        }
    }
}