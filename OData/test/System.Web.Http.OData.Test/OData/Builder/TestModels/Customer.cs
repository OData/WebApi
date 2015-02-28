// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.OData.Builder.TestModels
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
