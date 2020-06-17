// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace AspNetCoreODataSample.Web.Models
{
    public class Address
    {
        public string Street { get; set; }

        public string Region { get; set; }

        public IList<string> Emails { get; set; }

        public City RelatedCity { get; set; }
    }

    public class Customer
    {
        public int CustomerId { get; set; }

        public string Name { get; set; }

        public Address HomeAddress { get; set; }

        public IList<Address> Addresses { get; set; }

        public Order HomeOrder { get; set; }

        public IList<Order> Orders { get; set; }
    }

    public class VipCustomer : Customer
    {
        public int VipPrice { get; set; }

        public IList<int> VipTaxes { get; set; }

        public Address VipAddress { get; set; }

        public IList<Address> VipAddresses { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }

    public class City
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class UsAddress : Address
    {
        public string ZipCode { get; set; }
    }

    public class CnAddress : Address
    {
        public string PostCode { get; set; }
    }
}
