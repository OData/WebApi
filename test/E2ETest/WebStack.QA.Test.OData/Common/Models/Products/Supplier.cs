using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebStack.QA.Test.OData.Common.Models.Products
{
    public class Supplier
    {
        public Supplier()
        {
            Products = new List<Product>();
            Address = new Address();
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public Address Address { get; set; }

        public int Concurrency { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public virtual ICollection<Product> Products { get; set; }
    }
}
