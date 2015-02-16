// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData
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
    }
}
