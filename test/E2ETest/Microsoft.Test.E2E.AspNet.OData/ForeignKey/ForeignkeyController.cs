// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ForeignKey
{
    public class ForeignKeyCustomersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        ForeignKeyContext _db = new ForeignKeyContext();

        [EnableQuery]
        public ITestActionResult Get(int key)
        {
            ForeignKeyCustomer customer = _db.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        public ITestActionResult Delete(int key)
        {
            ForeignKeyCustomer customer = _db.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            _db.Customers.Remove(customer);
            _db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ODataRoute("ResetDataSource")]
        public ITestActionResult ResetDataSource()
        {
            if (_db.Database.Exists())
            {
                _db.Database.Delete();
                _db.Database.Create();
            }

            Generate();
            return Ok();
        }

        private void Generate()
        {
            int orderId = 1;
            for (int i = 1; i <= 5; i++)
            {
                ForeignKeyCustomer customer = new ForeignKeyCustomer
                {
                    Id = i,
                    Name = "Customer #" + i,
                    Orders = Enumerable.Range(1, 3).Select(e =>
                        new ForeignKeyOrder
                        {
                            OrderId = orderId,
                            OrderName = "Order #" + orderId++,
                            CustomerId = i
                        }).ToList()
                };

                foreach (var order in customer.Orders)
                {
                    order.Customer = customer;
                }

                _db.Customers.Add(customer);
                _db.Orders.AddRange(customer.Orders);
            }

            _db.SaveChanges();
        }

#if NETCORE
        public void Dispose()
        {
             //_db.Dispose();
        }
#endif
    }

    public class ForeignKeyOrdersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private readonly ForeignKeyContext _db = new ForeignKeyContext();

        public ITestActionResult Get(int key)
        {
            ForeignKeyOrder order = _db.Orders.FirstOrDefault(c => c.OrderId == key);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

#if NETCORE
        public void Dispose()
        {
            //_db.Dispose();
        }
#endif
    }

    // ActionOnDelete = none
    public class ForeignKeyCustomersNoCascadeController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        ForeignKeyContextNoCascade _db = new ForeignKeyContextNoCascade();

        [EnableQuery]
        public ITestActionResult Get(int key)
        {
            ForeignKeyCustomer customer = _db.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        public ITestActionResult Delete(int key)
        {
            ForeignKeyCustomer customer = _db.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            try
            {
                _db.Customers.Remove(customer);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ODataRoute("ResetDataSourceNonCacade")]
        public ITestActionResult ResetDataSourceNonCacade()
        {
            if (_db.Database.Exists())
            {
                _db.Database.Delete();
                _db.Database.Create();
            }

            Generate();
            return Ok();
        }

        private void Generate()
        {
            int orderId = 1;
            for (int i = 1; i <= 5; i++)
            {
                ForeignKeyCustomer customer = new ForeignKeyCustomer
                {
                    Id = i,
                    Name = "Customer #" + i,
                    Orders = Enumerable.Range(1, 3).Select(e =>
                        new ForeignKeyOrder
                        {
                            OrderId = orderId,
                            OrderName = "Order #" + orderId++,
                            CustomerId = i
                        }).ToList()
                };

                foreach (var order in customer.Orders)
                {
                    order.Customer = customer;
                }

                _db.Customers.Add(customer);
                _db.Orders.AddRange(customer.Orders);
            }

            _db.SaveChanges();
        }

#if NETCORE
        public void Dispose()
        {
            // _db.Dispose();
        }
#endif
    }

    public class ForeignKeyOrdersNoCascadeController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private readonly ForeignKeyContextNoCascade _db = new ForeignKeyContextNoCascade();

        public ITestActionResult Get(int key)
        {
            ForeignKeyOrder order = _db.Orders.FirstOrDefault(o => o.OrderId == key);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        public ITestActionResult Delete(int key)
        {
            ForeignKeyOrder order = _db.Orders.FirstOrDefault(o => o.OrderId == key);
            if (order == null)
            {
                return NotFound();
            }

            try
            {
                _db.Orders.Remove(order);
                _db.SaveChanges();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

#if NETCORE
        public void Dispose()
        {
             //_db.Dispose();
        }
#endif
    }
}
