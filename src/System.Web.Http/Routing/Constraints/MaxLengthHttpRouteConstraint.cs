// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter to be a string with a maximum length.
    /// </summary>
    public class MaxLengthHttpRouteConstraint : IHttpRouteConstraint
    {
        public MaxLengthHttpRouteConstraint(int maxLength)
        {
            MaxLength = maxLength;
        }

        /// <summary>
        /// Maximum length of the string.
        /// </summary>
        public int MaxLength { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            object value = values[parameterName];
            if (value == null)
            {
                return true;
            }

            return value.ToString().Length <= MaxLength;
        }
    }
}
