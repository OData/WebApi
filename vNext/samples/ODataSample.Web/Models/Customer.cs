using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataSample.Web.Models
{
    using Microsoft.AspNetCore.OData.Builder;
    using Microsoft.OData.Edm;

    public class Customer
    {
        private List<Product> _products;

        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Contained]
        public List<Product> Products
        {
            get
            {
                return this._products ?? (this._products = new List<Product>());
            }
            set
            {
                _products = value;
            }
        }
    }
}
