// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter to be parsable as the given type.
    /// </summary>
    public class TypeOfHttpRouteConstraint<T> : IHttpRouteConstraint
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

    public class BoolHttpRouteConstraint : TypeOfHttpRouteConstraint<bool> { }
    public class IntHttpRouteConstraint : TypeOfHttpRouteConstraint<int> { }
    public class LongHttpRouteConstraint : TypeOfHttpRouteConstraint<long> { }
    public class FloatHttpRouteConstraint : TypeOfHttpRouteConstraint<float> { }
    public class DoubleHttpRouteConstraint : TypeOfHttpRouteConstraint<double> { }
    public class DecimalHttpRouteConstraint : TypeOfHttpRouteConstraint<decimal> { }
    public class GuidHttpRouteConstraint : TypeOfHttpRouteConstraint<Guid> { }
    public class DateTimeHttpRouteConstraint : TypeOfHttpRouteConstraint<DateTime> { }
}