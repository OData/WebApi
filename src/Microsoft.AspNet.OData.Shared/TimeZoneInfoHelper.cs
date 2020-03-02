// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData
{
    internal class TimeZoneInfoHelper
    {
        private static TimeZoneInfo _defaultTimeZoneInfo;

        public static TimeZoneInfo TimeZone
        {
            get
            {
                if (_defaultTimeZoneInfo == null)
                {
                    return TimeZoneInfo.Local;
                }

                return _defaultTimeZoneInfo;
            }
            set { _defaultTimeZoneInfo = value; }
        }

        public static DateTimeOffset ConvertToDateTimeOffset(DateTime dateTime)
        {
            TimeZoneInfo timeZone = TimeZoneInfoHelper.TimeZone;
            TimeSpan utcOffset = timeZone.GetUtcOffset(dateTime);
            if (utcOffset >= TimeSpan.Zero)
            {
                if (dateTime <= DateTime.MinValue + utcOffset)
                {
                    return DateTimeOffset.MinValue;
                }
            }
            else
            {
                if (dateTime >= DateTime.MaxValue + utcOffset)
                {
                    return DateTimeOffset.MaxValue;
                }
            }

            if (dateTime.Kind == DateTimeKind.Local)
            {
                TimeZoneInfo localTimeZoneInfo = TimeZoneInfo.Local;
                TimeSpan localTimeSpan = localTimeZoneInfo.GetUtcOffset(dateTime);
                if (localTimeSpan < TimeSpan.Zero)
                {
                    if (dateTime >= DateTime.MaxValue + localTimeSpan)
                    {
                        return DateTimeOffset.MaxValue;
                    }
                }
                else
                {
                    if (dateTime <= DateTime.MinValue + localTimeSpan)
                    {
                        return DateTimeOffset.MinValue;
                    }
                }

                return TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), timeZone);
            }

            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTime(new DateTimeOffset(dateTime), timeZone);
            }

            return new DateTimeOffset(dateTime, timeZone.GetUtcOffset(dateTime));
        }
    }
}
