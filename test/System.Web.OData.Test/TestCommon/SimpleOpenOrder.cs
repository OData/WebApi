// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System.Web.OData
{
    public class SimpleOpenOrder
    {
        [Key]
        public int OrderId { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public SimpleOpenCustomer Customer { get; set; }
        public DateTimeOffset OrderDate { get; set; }
        public SimpleOpenAddress Address { get; set; }
        public IDictionary<string, object> OrderProperties { get; set; }
    }
}