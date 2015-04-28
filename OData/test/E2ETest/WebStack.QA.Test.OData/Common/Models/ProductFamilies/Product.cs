using System;

namespace WebStack.QA.Test.OData.Common.Models.ProductFamilies
{
    public partial class Product
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateTimeOffset? ReleaseDate { get; set; }

        public DateTimeOffset? SupportedUntil { get; set; }

        public virtual ProductFamily Family { get; set; }
    }
}
