// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    /// <summary>
    /// Constrains a url parameter by the values in the given enum type.
    /// </summary>
    public class EnumValueHttpRouteConstraint<TEnum> : IHttpRouteConstraint 
        where TEnum : struct
    {
        private readonly TypeConverter _converter;

        public EnumValueHttpRouteConstraint()
        {
            var valueType = Enum.GetUnderlyingType(typeof(TEnum));
            _converter = TypeDescriptor.GetConverter(valueType);
        }

        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            var value = values[parameterName];
            if (value == null)
            {
                return true;
            }

            if (!_converter.IsValid(value))
            {
                return false;
            }

            var convertedValue = _converter.ConvertFrom(value);
            if (convertedValue == null)
            {
                return false;
            }

            return Enum.IsDefined(typeof(TEnum), convertedValue);
        }
    }
}