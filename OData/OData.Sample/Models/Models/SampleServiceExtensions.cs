//using System.Collections.Generic;
//using System.Linq;

using System.Linq;

namespace ODataSample.Web.Models
{
	public static class SampleServiceExtensions
	{
		public static Product FindProduct(this ISampleService service, int id)
		{
			return service.Products.SingleOrDefault(p => p.ProductId == id);
		}

		public static Customer FindCustomer(this ISampleService service, int id)
		{
			return service.Customers.SingleOrDefault(p => p.CustomerId == id);
		}
	}
}

//    public class SampleContext : ISampleService
//    {
//	    private static int _productId;
//        #region In-memory data
//        private readonly List<Product> _products = new List<Product>
//        {
//            new Product { ProductId = ++_productId, Name = "Apple number1",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "Orange number1", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "Peanut butter number1", Price = 25 },
//            new Product { ProductId = ++_productId, Name = "xApple number2",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "xOrange number2", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "xPeanut butter number2", Price = 25 },
//            new Product { ProductId = ++_productId, Name = "xApple number2",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "xOrange number2", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "xPeanut butter number2", Price = 25 },
//            new Product { ProductId = ++_productId, Name = "xApple number2",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "xOrange number2", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "xPeanut butter number2", Price = 25 },
//            new Product { ProductId = ++_productId, Name = "xApple number2",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "xOrange number2", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "xPeanut butter number2", Price = 25 },
//            new Product { ProductId = ++_productId, Name = "Apple number3",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "Orange number3", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "Peanut butter number3", Price = 25 },
//            new Product { ProductId = ++_productId, Name = "Apple number4",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "Orange number4", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "Peanut butter number4", Price = 25 },
//            new Product { ProductId = ++_productId, Name = "Apple number5",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "Orange number5", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "Peanut butter number5", Price = 25 },
//            new Product { ProductId = ++_productId, Name = "Apple number6",  Price = 10 },
//            new Product { ProductId = ++_productId, Name = "Orange number6", Price = 20 },
//            new Product { ProductId = ++_productId, Name = "Peanut butter number6", Price = 25 },
//        };

//        private readonly List<Customer> _customers = new List<Customer>
//        {
//            new Customer { CustomerId = 1, FirstName = "Var1", LastName = "Var2" }
//        };

//        public SampleContext()
//        {
//            _customers[0].Products = new List<Product>
//            {
//                _products[0],
//                _products[1]
//            };
//        }
//        #endregion

//        #region ISampleService
//        public IQueryable<Product> Products => _products.AsQueryable();

//        public IQueryable<Customer> Customers => _customers.AsQueryable();
//        #endregion

//        #region Products business logic


//        public Product AddProduct(Product product)
//        {
//            var existingProduct = FindProduct(product.ProductId);
//            if (existingProduct != null)
//            {
//                return existingProduct;
//            }
            
//            _products.Add(product);
//            return product;
//        }

//        public bool UpdateProduct(int id, Product product)
//        {
//            if (id != product.ProductId)
//            {
//                return false;
//            }

//            var index = _products.FindIndex(p => p.ProductId == id);
//            if (index == -1)
//            {
//                return false;
//            }

//            _products[index] = product;
//            return true;
//        }

//        public bool DeleteProduct(int id)
//        {
//            int count = _products.RemoveAll(p => p.ProductId == id);
//            return count > 0;
//        }
//        #endregion

//        #region Customers business logic


//        public Customer AddCustomer(Customer customer)
//        {
//            var existingCustomer = FindCustomer(customer.CustomerId);
//            if (existingCustomer != null)
//            {
//                return existingCustomer;
//            }

//            _customers.Add(customer);
//            return customer;
//        }

//        public bool UpdateCustomer(int id, Customer customer)
//        {
//            if (id != customer.CustomerId)
//            {
//                return false;
//            }

//            var index = _customers.FindIndex(p => p.CustomerId == id);
//            if (index == -1)
//            {
//                return false;
//            }

//            _customers[index] = customer;
//            return true;
//        }

//        public bool DeleteCustomer(int id)
//        {
//            int count = _customers.RemoveAll(p => p.CustomerId == id);
//            return count > 0;
//        }
//        #endregion
//    }
//}
