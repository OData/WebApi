// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.UriParserExtension
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public IList<Order> Orders { get; set; }
    }

    public class VipCustomer : Customer
    {
        public string VipProperty { get; set; }
    }

    public enum Gender
    {
        Male = 1,
        Female = 2
    }

    public class Order
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }
}
