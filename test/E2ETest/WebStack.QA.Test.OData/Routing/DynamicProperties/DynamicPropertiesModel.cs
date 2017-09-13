// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace WebStack.QA.Test.OData.Routing.DynamicProperties
{
    public class DynamicCustomer
    {
        public int Id { get; set; }

        public Account Account { get; set; }

        public Order Order { get; set; }

        public Account SecondAccount { get; set; }

        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public class Order
    {
        public string Name { get; set; }

        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public class Account
    {
        public string Name { get; set; }

        public string Number { get; set; }

        public Dictionary<string, object> DynamicProperties { get; set; }
    }

    public class DynamicVipCustomer : DynamicCustomer
    {
        public string VipCode { get; set; }
    }

    public class DynamicSingleCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Account Account { get; set; }

        public Order Order { get; set; }

        public Account SecondAccount { get; set; }

        public Dictionary<string, object> DynamicProperties { get; set; }
    }
}
