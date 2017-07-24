using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace WebStack.QA.Test.OData.ForeignKey
{
    public class ForeignKeyCustomersController : ODataController
    {
        ForeignKeyContext _db = new ForeignKeyContext();

        [EnableQuery]
        public IHttpActionResult Get(int key)
        {
            ForeignKeyCustomer customer = _db.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        public IHttpActionResult Delete(int key)
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
        public IHttpActionResult ResetDataSource()
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
    }

    public class ForeignKeyOrdersController : ODataController
    {
        private readonly ForeignKeyContext _db = new ForeignKeyContext();

        public IHttpActionResult Get(int key)
        {
            ForeignKeyOrder order = _db.Orders.FirstOrDefault(c => c.OrderId == key);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
    }

    // ActionOnDelete = none
    public class ForeignKeyCustomersNoCascadeController : ODataController
    {
        ForeignKeyContextNoCascade _db = new ForeignKeyContextNoCascade();

        [EnableQuery]
        public IHttpActionResult Get(int key)
        {
            ForeignKeyCustomer customer = _db.Customers.FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        public IHttpActionResult Delete(int key)
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
        public IHttpActionResult ResetDataSourceNonCacade()
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
    }

    public class ForeignKeyOrdersNoCascadeController : ODataController
    {
        private readonly ForeignKeyContextNoCascade _db = new ForeignKeyContextNoCascade();

        public IHttpActionResult Get(int key)
        {
            ForeignKeyOrder order = _db.Orders.FirstOrDefault(o => o.OrderId == key);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        public IHttpActionResult Delete(int key)
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
    }
}
