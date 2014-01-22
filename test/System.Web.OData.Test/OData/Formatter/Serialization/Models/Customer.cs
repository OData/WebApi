// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.TestCommon.Types;

namespace System.Web.OData.Formatter.Serialization.Models
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
        public SimpleEnum SimpleEnum { get; set; }
    }
}
