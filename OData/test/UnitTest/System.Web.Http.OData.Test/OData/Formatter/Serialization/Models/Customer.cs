﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.OData.Formatter.Serialization.Models
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
        public IList<Order> Orders { get; private set; }
    }
}
