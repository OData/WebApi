// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter by the names in the given enum type.
    /// </summary>
    public class EnumNameHttpRouteConstraint<TEnum> : IHttpRouteConstraint 
        where TEnum : struct
    {
        private readonly HashSet<string> _enumNames;

        public EnumNameHttpRouteConstraint()
        {
            _enumNames = new HashSet<string>(Enum.GetNames(typeof(TEnum)));
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
            {
                return true;
            }

            return _enumNames.Any(n => n.Equals(value.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
