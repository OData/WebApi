// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constraints a url parameter to be a long within a given range of values.
    /// </summary>
    public class RangeHttpRouteConstraint : IHttpRouteConstraint, IInlineRouteConstraint
    {
        // These must be set from the ctor of implementors.
        private readonly MinHttpRouteConstraint _minConstraint;
        private readonly MaxHttpRouteConstraint _maxConstraint;

        /// <summary>
        /// Constraints a url parameter to be a long within a given range of values.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public RangeHttpRouteConstraint(string min, string max)
        {
            _minConstraint = new MinHttpRouteConstraint(min);
            _maxConstraint = new MaxHttpRouteConstraint(max);
        }

        /// <summary>
        /// Minimum value.
        /// </summary>
        public long Min 
        {
            get { return _minConstraint.Min; }
        }

        /// <summary>
        /// Minimum valut.
        /// </summary>
        public long Max
        {
            get { return _maxConstraint.Max; }
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            return _minConstraint.Match(request, route, parameterName, values, routeDirection) &&
                   _maxConstraint.Match(request, route, parameterName, values, routeDirection);
        }
    }
}