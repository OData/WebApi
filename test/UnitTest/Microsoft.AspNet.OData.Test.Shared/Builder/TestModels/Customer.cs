// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public Address Address { get; set; }
        public Address WorkAddress { get; set; }
        public string Website { get; set; }
        public string ShareSymbol { get; set; }
        public Decimal? SharePrice { get; set; }
        public Company Company { get; set; }
        public List<Order> Orders { get; set; }
        public List<string> Aliases { get; set; }
        public List<Address> Addresses { get; set; }
        public Dictionary<string, object> DynamicProperties { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public IDictionary<string, IDictionary<string, object>> InstanceAnnotations { get; set; }
    }     
}
