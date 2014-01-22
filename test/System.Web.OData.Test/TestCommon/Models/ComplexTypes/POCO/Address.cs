// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.OData.TestCommon.Models
{
    public class Address
    {
        public string StreetAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public Address()
        {
        }

        public Address(int index, ReferenceDepthContext context)
        {
            Address sourceAddress = DataSource.Address[index];
            this.StreetAddress = sourceAddress.StreetAddress;
            this.City = sourceAddress.City;
            this.State = sourceAddress.State;
            this.ZipCode = sourceAddress.ZipCode;
        }

        public int ZipCode { get; set; }
    }
}
