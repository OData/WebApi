// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter to be parsable as the given type.
    /// </summary>
    public abstract class TypeOfHttpRouteConstraint<T> : IHttpRouteConstraint, IInlineRouteConstraint
        where T : struct
    {
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
            {
                return true;
            }

            return value.Parse<T>().HasValue;
        }
    }
}