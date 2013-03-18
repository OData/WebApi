// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a route parameter to match a regular expression.
    /// </summary>
    public class RegexHttpRouteConstraint : IHttpRouteConstraint
    {
        private readonly Regex _regex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexHttpRouteConstraint" /> class.
        /// </summary>
        /// <param name="pattern">The regular expression pattern to match.</param>
        public RegexHttpRouteConstraint(string pattern)
        {
            Pattern = pattern;
            _regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Gets the regular expression pattern to match.
        /// </summary>
        public string Pattern { get; private set; }

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
                return _regex.IsMatch(valueString);
            }
            return false;
        }
    }
}
