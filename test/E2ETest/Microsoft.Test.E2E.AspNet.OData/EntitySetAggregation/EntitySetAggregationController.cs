using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.EntitySetAggregation
{
    public class CustomersController : TestODataController
    {
        private readonly AggregationContext _db = new AggregationContext();

        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new AggregationContext();
            return db.Customers;
        }

        [EnableQuery]
        public TestSingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new AggregationContext();
            return TestSingleResult.Create(db.Customers.Where(c => c.Id == key));
        }

        public void Generate()
        {
            for (int i = 1; i <= 3; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + (i+1) % 2,
                    Orders = 
                        new List<Order> {
                            new Order {
                                Name = "Order" + 2*i,
                                Price = i * 25,
                                SaleInfo = new SaleInfo { Quantity = i, UnitPrice = 25 }
                            },
                            new Order {
                                Name = "Order" + 2*i+1,
                                Price = i * 75,
                                SaleInfo = new SaleInfo { Quantity = i, UnitPrice = 75 }
                            }
                        },
                    Address = new Address
                    {
                        Name = "City" + i % 2,
                        Street = "Street" + i % 2,
                    }
                };

                _db.Customers.Add(customer);
            }

            _db.SaveChanges();
        }

        private void ResetDataSource()
        {
            if (_db.Database.Exists())
            {
                _db.Database.Delete();
                _db.Database.Create();
            }

            Generate();
        }
    }
}
