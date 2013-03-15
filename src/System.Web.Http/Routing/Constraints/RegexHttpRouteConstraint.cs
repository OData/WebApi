// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace System.Web.Http.Routing.Constraints
{
    public class RegexHttpRouteConstraint : IHttpRouteConstraint, IInlineRouteConstraint
    {
        private readonly Regex _regex;

        public RegexHttpRouteConstraint(string pattern)
        {
            _regex = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            
            Pattern = pattern;
            Options = _regex.Options;
        }

        public RegexOptions Options { get; private set; }

        public string Pattern { get; private set; }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
            {
                return true;
            }

            return _regex.IsMatch(value.ToString());
        }
    }
}
