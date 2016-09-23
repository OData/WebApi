﻿using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace WebStack.QA.Test.OData.Aggregation
{
    public class CustomersController : ODataController
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
        public SingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new AggregationContext();
            return SingleResult.Create(db.Customers.Where(c => c.Id == key));
        }

        public void Generate()
        {
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + i % 2,
                    Order = new Order
                    {
                        Id = i,
                        Name = "Order" + i % 2,
                        Price = i * 100
                    },
                    Address = new Address
                    {
                        Name = "City" + i % 2,
                        Street = "Street" + i % 2,
                    }
                };

                _db.Customers.Add(customer);
            }

            _db.Customers.Add(new Customer()
            {
                Id = 10,
                Name = null,
                Address = new Address
                {
                    Name = "City1" ,
                    Street = "Street",
                },
                Order = new Order
                {
                    Id = 10,
                    Name = "Order" + 10 % 2,
                    Price = 0
                },
            });

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
