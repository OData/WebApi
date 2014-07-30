// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.OData.Builder.TestModels
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public string Website { get; set; }
        public string ShareSymbol { get; set; }
        public Decimal? SharePrice { get; set; }
        public List<Order> Orders { get; set; }
        public List<string> Aliases { get; set; }
        public List<Address> Addresses { get; set; }
    }
}
