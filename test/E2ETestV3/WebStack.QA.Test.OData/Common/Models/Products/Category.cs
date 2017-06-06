using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStack.QA.Test.OData.Common.Models.Products
{
    public class Category
    {
        public Category()
        {
            Products = new List<Product>();
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
