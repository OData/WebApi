// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to be a string with a maximum length.
    /// </summary>
    public class MaxLengthHttpRouteConstraint : IHttpRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaxLengthHttpRouteConstraint" /> class.
        /// </summary>
        /// <param name="maxLength">The maximum length of the route parameter</param>
        public MaxLengthHttpRouteConstraint(int maxLength)
        {
            if (maxLength < 0)
            {
                throw Error.ArgumentMustBeGreaterThanOrEqualTo("maxLength", maxLength, 0);
            }

            MaxLength = maxLength;
        }

        /// <summary>
        /// Gets the maximum length of the route parameter.
        /// </summary>
        public int MaxLength { get; private set; }

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
                return valueString.Length <= MaxLength;
            }
            return false;
        }
    }
}
