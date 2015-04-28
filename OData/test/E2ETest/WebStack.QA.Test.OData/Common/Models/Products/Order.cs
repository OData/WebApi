using System;
using System.Collections.Generic;

namespace WebStack.QA.Test.OData.Common.Models.Products
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
