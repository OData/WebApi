// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.OData
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
