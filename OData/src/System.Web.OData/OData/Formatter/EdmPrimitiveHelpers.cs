﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Xml.Linq;
using Microsoft.OData.Edm.Library;

namespace System.Web.OData.Formatter
{
    internal static class EdmPrimitiveHelpers
    {
        public static object ConvertPrimitiveValue(object value, Type type)
        {
            Contract.Assert(value != null);
            Contract.Assert(type != null);

            // if value is of the same type nothing to do here.
            if (value.GetType() == type || value.GetType() == Nullable.GetUnderlyingType(type))
            {
                return value;
            }

            if (type.IsInstanceOfType(value))
            {
                return value;
            }

            string str = value as string;

            if (type == typeof(char))
            {
                if (str == null || str.Length != 1)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeStringLengthOne));
                }

                return str[0];
            }
            else if (type == typeof(char?))
            {
                if (str == null || str.Length > 1)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeStringMaxLengthOne));
                }

                return str.Length > 0 ? str[0] : (char?)null;
            }
            else if (type == typeof(char[]))
            {
                if (str == null)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                }

                return str.ToCharArray();
            }
            else if (type == typeof(Binary))
            {
                return new Binary((byte[])value);
            }
            else if (type == typeof(XElement))
            {
                if (str == null)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                }

                return XElement.Parse(str);
            }
            else
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                if (type.IsEnum)
                {
                    if (str == null)
                    {
                        throw new ValidationException(Error.Format(SRResources.PropertyMustBeString));
                    }

                    return Enum.Parse(type, str);
                }
                else if (type == typeof(DateTime))
                {
                    if (value is DateTimeOffset)
                    {
                        DateTimeOffset dateTimeOffsetValue = (DateTimeOffset)value;
                        TimeZoneInfo timeZone = TimeZoneInfoHelper.TimeZone;
                        dateTimeOffsetValue = TimeZoneInfo.ConvertTime(dateTimeOffsetValue, timeZone);
                        return dateTimeOffsetValue.DateTime;
                    }

                    if (value is Date)
                    {
                        Date dt = (Date)value;
                        return (DateTime)dt;
                    }

                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeDateTimeOffsetOrDate));
                }
                else if (type == typeof(TimeSpan))
                {
                    if (value is TimeOfDay)
                    {
                        TimeOfDay tod = (TimeOfDay)value;
                        return (TimeSpan)tod;
                    }

                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeTimeOfDay));
                }
                else
                {
                    Contract.Assert(type == typeof(uint) || type == typeof(ushort) || type == typeof(ulong));

                    // Note that we are not casting the return value to nullable<T> as even if we do it
                    // CLR would unbox it back to T.
                    return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
