// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System.Web.OData
{
    public class SimpleOpenCustomer
    {
        [Key]
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public SimpleOpenAddress Address { get; set; }
        public string Website { get; set; }
        public List<SimpleOpenOrder> Orders { get; set; }
        public IDictionary<string, object> CustomerProperties { get; set; }
    }
}