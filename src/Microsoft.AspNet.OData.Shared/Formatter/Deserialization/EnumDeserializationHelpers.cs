//-----------------------------------------------------------------------------
// <copyright file="EnumDeserializationHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
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

            if (!TypeHelper.IsEnum(enumType))
            {
                throw Error.InvalidOperation(Error.Format(SRResources.TypeMustBeEnumOrNullableEnum, type.Name));
            }

            return Enum.Parse(enumType, enumValue.Value);
        }
    }
}
