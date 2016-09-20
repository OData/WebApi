using System.Collections.Generic;
using System.Linq;

namespace ODataSample.Web.Models
{
    using System;

    public class SampleContext : ISampleService
    {
        #region In-memory data
        private readonly List<Product> _products = new List<Product>
        {
            new Product { ProductId = 1, Name = "Apple",  Price = 10 },
            new Product { ProductId = 2, Name = "Orange", Price = 20 },
            new Product { ProductId = 3, Name = "Banana", Price = 30 },
            new Product { ProductId = 4, Name = "Cherry", Price = 40 },

        };

        private readonly List<Customer> _customers = new List<Customer>
        {
            new Customer { CustomerId = 1, FirstName = "Mark", LastName = "Stand" },
            new Customer { CustomerId = 2, FirstName = "Peter", LastName = "Huward" },
            new Customer { CustomerId = 3, FirstName = "Sam", LastName = "Xu" }
        };

        public SampleContext()
        {
            _customers[0].Products = new List<Product>
            {
                _products[0],
                _products[1]
            };
        }
        #endregion

        #region ISampleService
        public IEnumerable<Product> Products => _products;

        public IEnumerable<Customer> Customers => _customers;

        public IEnumerable<Customer> FindCustomersWithProduct(int productId)
        {
            return _customers.Where(c => c.Products.FirstOrDefault(p => p.ProductId == productId) != null);
        }

        public Customer AddCustomerProduct(int customerId, int productId)
        {
            var customer = _customers.SingleOrDefault(p => p.CustomerId == customerId);
            var product = _products.SingleOrDefault(p => p.ProductId == productId);

            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            if (customer != null)
            {
                customer.Products.Add(product);
            }

            return customer;
        }


        public Customer AddCustomerProducts(int customerId, IEnumerable<int> products)
        {
            var customer = _customers.SingleOrDefault(p => p.CustomerId == customerId);
            if (customer == null)
            {
                throw new ArgumentNullException(nameof(customer));
            }

            foreach (var productId in products)
            {
                var product = _products.SingleOrDefault(p => p.ProductId == productId);

                if (product == null)
                {
                    throw new ArgumentNullException(nameof(product));
                }
                customer.Products.Add(product);
            }

            return customer;
        }

        #endregion

        #region Products business logic

        public Product FindProduct(int id)
        {
            return _products.SingleOrDefault(p => p.ProductId == id);
        }

        public Product AddProduct(Product product)
        {
            var existingProduct = FindProduct(product.ProductId);
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

        public Customer FindCustomer(int id)
        {
            return _customers.SingleOrDefault(p => p.CustomerId == id);
        }

        public Customer AddCustomer(Customer customer)
        {
            var existingCustomer = FindCustomer(customer.CustomerId);
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

            customer.Products = _customers[index].Products;
            _customers[index] = customer;
            return true;
        }

        public bool DeleteCustomer(int id)
        {
            int count = _customers.RemoveAll(p => p.CustomerId == id);
            return count > 0;
        }

        public bool TestPrimitiveReturnType()
        {
            return true;
        }

        #endregion
    }
}
