// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.Builder.TestModels
{
    public class Address
    {
        public int HouseNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public ZipCode ZipCode { get; set; }
        public string IgnoreThis { get; set; }
    }
}
