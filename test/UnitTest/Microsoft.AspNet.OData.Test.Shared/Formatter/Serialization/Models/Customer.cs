﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Test.Common.Types;

namespace Microsoft.AspNet.OData.Test.Formatter.Serialization.Models
{
    public class Customer
    {
        public Customer()
        {
            this.Orders = new List<Order>();
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public IList<Order> Orders { get; set; }
        public SimpleEnum SimpleEnum { get; set; }
        public Address HomeAddress { get; set; }
    }

    public class SpecialCustomer : Customer
    {
        public int Level { get; set; }
        public DateTimeOffset Birthday { get; set; }
        public Decimal Bonus { get; set; }
    }
}
