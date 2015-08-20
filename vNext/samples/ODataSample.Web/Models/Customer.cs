using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataSample.Web.Models
{
    public class Customer
    {
        private List<Product> _products;

        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
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
