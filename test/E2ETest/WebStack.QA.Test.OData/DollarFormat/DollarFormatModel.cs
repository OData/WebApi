using System;
using System.Collections.Generic;

namespace WebStack.QA.Test.OData.DollarFormat
{
    public class DollarFormatCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<DollarFormatOrder> Orders { get; set; }
        public DollarFormatOrder SpecialOrder { get; set; }
    }

    public class DollarFormatOrder
    {
        public int Id { get; set; }
        public DateTimeOffset PurchaseDate { get; set; }
        public string Detail { get; set; }
    }
}
