//-----------------------------------------------------------------------------
// <copyright file="EdmPrimitiveHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.DataAnnotations;
#if NETFX // System.Data.Linq.Binary is only supported in the AspNet version.
using System.Data.Linq;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter
{
    internal static class EdmPrimitiveHelpers
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "These are simple conversion function and cannot be split up.")]
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
#if NETFX // System.Data.Linq.Binary is only supported in the AspNet version.
            else if (type == typeof(Binary))
            {
                return new Binary((byte[])value);
            }
#endif
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
                if (TypeHelper.IsEnum(type))
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
                else if (type == typeof(bool))
                {
                    bool result;
                    if (str != null && Boolean.TryParse(str, out result))
                    {
                        return result;
                    }

                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeBoolean));
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
