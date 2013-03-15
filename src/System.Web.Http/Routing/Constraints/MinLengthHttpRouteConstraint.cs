// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter to be a string with a maximum length.
    /// </summary>
    public class MinLengthHttpRouteConstraint : IHttpRouteConstraint
    {
        public MinLengthHttpRouteConstraint(string minLength)
        {
            var parsedMinLength = minLength.ParseInt();
            if (!parsedMinLength.HasValue)
            {
                throw new ArgumentOutOfRangeException("minLength", minLength);
            }

            MinLength = parsedMinLength.Value;
        }

        /// <summary>
        /// Minimum length of the string.
        /// </summary>
        public int MinLength { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
            {
                return true;
            }

            return value.ToString().Length >= MinLength;
        }
    }
}