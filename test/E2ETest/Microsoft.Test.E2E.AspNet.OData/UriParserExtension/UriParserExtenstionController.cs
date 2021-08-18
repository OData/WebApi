//-----------------------------------------------------------------------------
// <copyright file="UriParserExtenstionController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.UriParserExtension
{
    public class CustomersController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(UriParseExtenstionDbContext.GetCustomers());
        }

        public ITestActionResult Get(int key)
        {
            return Ok(UriParseExtenstionDbContext.GetCustomers().FirstOrDefault(c => c.Id == key));
        }

        public ITestActionResult GetName(int key)
        {
            var customer = UriParseExtenstionDbContext.GetCustomers().FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Name);
        }

        public ITestActionResult GetVipProperty(int key)
        {
            var customer = UriParseExtenstionDbContext.GetCustomers().FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            VipCustomer vipCusomter = customer as VipCustomer;
            if (vipCusomter == null)
            {
                return NotFound();
            }

            return Ok(vipCusomter.VipProperty);
        }

        [EnableQuery]
        public ITestActionResult GetOrders(int key)
        {
            var customer = UriParseExtenstionDbContext.GetCustomers().FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Orders);
        }

        public ITestActionResult GetRef(int key, string navigationProperty)
        {
            var serviceRootUri = GetServiceRootUri();
            var entityId = string.Format("{0}/Customers({1})/{2}", serviceRootUri, key, navigationProperty);
            return Ok(new Uri(entityId));
        }

        [HttpGet]
        public ITestActionResult CalculateSalary(int key, int month)
        {
            return Ok("CalculateSalary: Key(" + key + ")(" + month + ")");
        }

        [HttpPost]
        public ITestActionResult UpdateAddress(int key)
        {
            return Ok("UpdateAddress: Key(" + key + ")");
        }

        [HttpGet]
        public ITestActionResult GetCustomerByGender(Gender gender)
        {
            var customers = UriParseExtenstionDbContext.GetCustomers().Where(c => c.Gender == gender);
            return Ok(customers);
        }
    }

    public class OrdersController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(UriParseExtenstionDbContext.GetOrders());
        }

        public ITestActionResult Get(int key)
        {
            return Ok(UriParseExtenstionDbContext.GetOrders().FirstOrDefault(c => c.Id == key));
        }
    }

    public class UriParseExtenstionDbContext
    {
        private static IList<Customer> _customers;
        private static IList<Order> _orders;

        public static IList<Customer> GetCustomers()
        {
            if (_customers == null)
            {
                Generate();
            }

            return _customers;
        }

        public static IList<Order> GetOrders()
        {
            if (_orders == null)
            {
                Generate();
            }

            return _orders;
        }

        private static void Generate()
        {
            _customers = Enumerable.Range(1, 5).Select(e =>
                new Customer
                {
                    Id = e,
                    Name = "Customer #" + e,
                    Gender = e%2 == 0 ? Gender.Female : Gender.Male,
                    Orders = Enumerable.Range(1, e + 1).Select(f =>
                        new Order
                        {
                            Id = f,
                            Title = "Order #" + f
                        }).ToList()
                }).ToList();

            _customers.Add(new VipCustomer
            {
                Id = 6,
                Name = "VipCustomer #6",
                Gender = Gender.Female,
                Orders = Enumerable.Range(1, 3).Select(f =>
                    new Order
                    {
                        Id = f,
                        Title = "Order #" + f
                    }).ToList(),
                VipProperty = "VipProperty "
            });

            _orders = new List<Order>();
            foreach (var customer in _customers)
            {
                foreach (var order in customer.Orders)
                {
                    _orders.Add(order);
                }
            }
        }
    }
}
