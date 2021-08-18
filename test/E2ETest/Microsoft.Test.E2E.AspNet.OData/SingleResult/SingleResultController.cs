//-----------------------------------------------------------------------------
// <copyright file="SingleResultController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.SingleResultTest
{
    public class CustomersController : TestODataController
#if NETCORE
        , IDisposable
#endif
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

#if NETCORE
        public void Dispose()
        {
            //_db.Dispose();
        }
#endif
    }
}
