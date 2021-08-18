//-----------------------------------------------------------------------------
// <copyright file="Order.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
