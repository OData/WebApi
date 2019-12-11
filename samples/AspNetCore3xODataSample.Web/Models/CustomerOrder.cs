// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore3xODataSample.Web.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Address HomeAddress { get; set; }

        public Order Order { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }

        public string Title { get; set; }
    }

    [Owned, ComplexType]
    public class Address
    {
        public string City { get; set; }

        public string Street { get; set; }
    }
}
