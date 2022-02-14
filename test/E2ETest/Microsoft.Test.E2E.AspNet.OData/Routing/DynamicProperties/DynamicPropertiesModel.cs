//-----------------------------------------------------------------------------
// <copyright file="DynamicPropertiesModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Routing.DynamicProperties
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
