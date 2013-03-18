// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a route by an inner constraint that doesn't fail when an optional parameter is set to its default value.
    /// </summary>
    public class OptionalHttpRouteConstraint : IHttpRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionalHttpRouteConstraint" /> class.
        /// </summary>
        /// <param name="innerConstraint">The inner constraint to match if the parameter is not an optional parameter without a value</param>
        public OptionalHttpRouteConstraint(IHttpRouteConstraint innerConstraint)
        {
            if (innerConstraint == null)
            {
                throw Error.ArgumentNull("innerConstraint");
            }

            InnerConstraint = innerConstraint;
        }

        /// <summary>
        /// Gets the inner constraint to match if the parameter is not an optional parameter without a value.
        /// </summary>
        public IHttpRouteConstraint InnerConstraint { get; private set; }

        /// <inheritdoc />
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (route == null)
            {
                throw Error.ArgumentNull("route");
            }

            if (parameterName == null)
            {
                throw Error.ArgumentNull("parameterName");
            }

            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            // If the parameter is optional and has no value, then pass the constraint
            object defaultValue;
            if (route.Defaults.TryGetValue(parameterName, out defaultValue) && defaultValue == RouteParameter.Optional)
            {
                object value;
                if (values.TryGetValue(parameterName, out value) && value == RouteParameter.Optional)
                {
                    return true;
                }
            }

            return InnerConstraint.Match(request, route, parameterName, values, routeDirection);
        }
    }
}