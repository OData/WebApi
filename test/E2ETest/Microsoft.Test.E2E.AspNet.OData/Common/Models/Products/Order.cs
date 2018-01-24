// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.Products
{
    public class Order
    {
        public int OrderId { get; set; }
        public IEnumerable<OrderLine> OrderLines { get; set; }
    }

    public class OrderLine
    {
        public int OrderLineId { get; set; }
        public Product Product { get; set; }
        public Decimal Cost { get; set; }
    }
}
