// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.SingleResultTest
{
    public class CustomersController : TestODataController, IDisposable
    {
        private readonly SingleResultContext _db = new SingleResultContext();

        [EnableQuery]
        public TestSingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new SingleResultContext();
            return TestSingleResult.Create<Customer>(db.Customers.Where(c => c.Id == key));
        }

        public void Generate()
        {
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            Id = i,
                        }
                    }
                };

                _db.Customers.Add(customer);
            }

            _db.SaveChanges();
        }

        private void ResetDataSource()
        {
            if (!_db.Customers.Any())
            {
                Generate();
            }
        }

        public void Dispose()
        {
            // _db.Dispose();
        }
    }
}
