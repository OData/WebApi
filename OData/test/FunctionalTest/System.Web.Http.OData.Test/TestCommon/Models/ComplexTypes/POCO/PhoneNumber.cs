// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.Http.OData.TestCommon.Models
{
    public struct PhoneNumber
    {
        public int CountryCode { get; set; }

        public int AreaCode { get; set; }

        public int Number { get; set; }

        public PhoneType PhoneType { get; set; }
    }
}
