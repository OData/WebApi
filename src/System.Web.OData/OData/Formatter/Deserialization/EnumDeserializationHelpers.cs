// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.OData.Formatter.Deserialization
{
    internal static class EnumDeserializationHelpers
    {
        public static object ConvertEnumValue(object value, Type type)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            Type enumType = TypeHelper.GetUnderlyingTypeOrSelf(type);

            // if value is of the requested type nothing to do here.
            if (value.GetType() == enumType)
            {
                return value;
            }

            ODataEnumValue enumValue = value as ODataEnumValue;

            if (enumValue == null)
            {
                throw new ValidationException(Error.Format(SRResources.PropertyMustBeEnum, value.GetType().Name, "ODataEnumValue"));
            }

            if (!enumType.IsEnum)
            {
                throw Error.InvalidOperation(Error.Format(SRResources.TypeMustBeEnumOrNullableEnum, type.Name));
            }

            return Enum.Parse(enumType, enumValue.Value);
        }
    }
}