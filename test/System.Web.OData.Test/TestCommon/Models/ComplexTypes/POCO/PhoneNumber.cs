// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.TestCommon.Models
{
    public struct PhoneNumber
    {
        public int CountryCode { get; set; }

        public int AreaCode { get; set; }

        public int Number { get; set; }

        public PhoneType PhoneType { get; set; }
    }
}
