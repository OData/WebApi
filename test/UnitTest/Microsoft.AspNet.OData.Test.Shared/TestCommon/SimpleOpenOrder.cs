// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNet.OData.Test.Common
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