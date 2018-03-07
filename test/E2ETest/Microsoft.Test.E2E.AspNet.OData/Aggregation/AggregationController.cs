// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class BaseCustomersController : TestODataController
    {
        protected readonly AggregationContext _db = new AggregationContext();

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
                    Name = "City1",
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

        protected void ResetDataSource()
        {
            if (!_db.Customers.Any())
            {
                Generate();
            }
        }
    }

    public class CustomersController : BaseCustomersController
    {
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
    }
}
