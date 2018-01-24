// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;

namespace Microsoft.Test.E2E.AspNet.OData.SingleResultTest
{
    public class CustomersController : ODataController
    {
        private readonly SingleResultContext _db = new SingleResultContext();

        [EnableQuery]
        public SingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new SingleResultContext();
            return SingleResult.Create(db.Customers.Where(c => c.Id == key));
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
    }
}
