// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter to be a string of a given length 
    /// or within a given range of lengths if two params are given.
    /// </summary>
    public class LengthHttpRouteConstraint : IHttpRouteConstraint, IInlineRouteConstraint
    {
        private readonly MinLengthHttpRouteConstraint _minLengthHttpConstraint;
        private readonly MaxLengthHttpRouteConstraint _maxLengthHttpConstraint;

        /// <summary>
        /// Constrains a url parameter to be a string of a given length.
        /// </summary>
        /// <param name="length">The length of the string</param>
        public LengthHttpRouteConstraint(string length)
        {
            var parsedLength = length.ParseInt();
            if (!parsedLength.HasValue)
            {
                throw new ArgumentOutOfRangeException("length", length);
            }

            Length = parsedLength;
        }

        /// <summary>
        /// Constrains a url parameter to be a string with a length in the given range.
        /// </summary>
        /// <param name="minLength">The minimum length of the string.</param>
        /// <param name="maxLength">The maximum length of the string.</param>
        public LengthHttpRouteConstraint(string minLength, string maxLength)
        {
            _minLengthHttpConstraint = new MinLengthHttpRouteConstraint(minLength);
            _maxLengthHttpConstraint = new MaxLengthHttpRouteConstraint(maxLength);
        }

        /// <summary>
        /// Length of the string.
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Minimum length of the string.
        /// </summary>
        public int? MinLength 
        {
            get { return _minLengthHttpConstraint != null ? _minLengthHttpConstraint.MinLength : (int?)null; }
        }

        /// <summary>
        /// Minimum length of the string.
        /// </summary>
        public int? MaxLength
        {
            get { return _maxLengthHttpConstraint.MaxLength; }
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (Length.HasValue)
            {
                var value = values[parameterName];
                if (value == null)
                {
                    return true;
                }

                return value.ToString().Length == Length.Value;
            }

            return _minLengthHttpConstraint.Match(request, route, parameterName, values, routeDirection) &&
                   _maxLengthHttpConstraint.Match(request, route, parameterName, values, routeDirection);
        }
    }
}