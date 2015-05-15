using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataSample.Web.Models
{
    public class SampleContext : ISampleService
    {
        #region In-memory data
        private readonly List<Product> _products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Apple",  Price = 10 },
            new Product { ProductId = 2, Name = "Orange", Price = 20 },
            new Product { ProductId = 3, Name = "Banana", Price = 30 }
        };

        private readonly List<Customer> _customers = new List<Customer>
        {
            new Customer { CustomerId = 1, FirstName = "Lewis", LastName = "Cheng" }
        };

        public SampleContext()
        {
            _customers[0].Products = new List<Product>
            {
                _products[0],
                _products[2]
            };
        }
        #endregion

        #region ISampleService
        public IEnumerable<Product> Products => _products;

        public IEnumerable<Customer> Customers => _customers;
        #endregion

        #region Products business logic
        public Product AddProduct(Product product)
        {
            var existingProduct = _products.SingleOrDefault(p => p.ProductId == product.ProductId);
            if (existingProduct != null)
            {
                return existingProduct;
            }
            
            _products.Add(product);
            return product;
        }

        public bool UpdateProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return false;
            }

            var index = _products.FindIndex(p => p.ProductId == id);
            if (index == -1)
            {
                return false;
            }

            _products[index] = product;
            return true;
        }

        public bool DeleteProduct(int id)
        {
            int count = _products.RemoveAll(p => p.ProductId == id);
            return count > 0;
        }
        #endregion

        #region Customers business logic
        public Customer AddCustomer(Customer customer)
        {
            var existingCustomer = _customers.SingleOrDefault(p => p.CustomerId == customer.CustomerId);
            if (existingCustomer != null)
            {
                return existingCustomer;
            }

            _customers.Add(customer);
            return customer;
        }

        public bool UpdateCustomer(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return false;
            }

            var index = _customers.FindIndex(p => p.CustomerId == id);
            if (index == -1)
            {
                return false;
            }

            _customers[index] = customer;
            return true;
        }

        public bool DeleteCustomer(int id)
        {
            int count = _customers.RemoveAll(p => p.CustomerId == id);
            return count > 0;
        }
        #endregion
    }
}
