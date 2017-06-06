using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebStack.QA.Test.OData.Common.Models.Products
{
    public class Product
    {
        public Product()
        {
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public DateTime? DiscontinuedDate { get; set; }

        public DateTimeOffset? DateTimeOffset { get; set; }

        public int Rating { get; set; }

        public decimal? Price { get; set; }

        public double? Amount { get; set; }

        public Guid Guid { get; set; }

        public byte[] Binary { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public virtual Category Category { get; set; }

        public virtual Supplier Supplier { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public virtual Product RelatedProduct { get; set; }

        public bool? Taxable { get; set; }
    }
}
