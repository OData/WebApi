// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;

namespace Microsoft.Test.E2E.AspNet.OData.UriParserExtension
{
    public class CustomersController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(UriParseExtenstionDbContext.GetCustomers());
        }

        public IHttpActionResult Get(int key)
        {
            return Ok(UriParseExtenstionDbContext.GetCustomers().FirstOrDefault(c => c.Id == key));
        }

        public IHttpActionResult GetName(int key)
        {
            var customer = UriParseExtenstionDbContext.GetCustomers().FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Name);
        }

        public IHttpActionResult GetVipProperty(int key)
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
        public IHttpActionResult GetOrders(int key)
        {
            var customer = UriParseExtenstionDbContext.GetCustomers().FirstOrDefault(c => c.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Orders);
        }

        public IHttpActionResult GetRef(int key, string navigationProperty)
        {
            var serviceRootUri = GetServiceRootUri();
            var entityId = string.Format("{0}/Customers({1})/{2}", serviceRootUri, key, navigationProperty);
            return Ok(new Uri(entityId));
        }

        [HttpGet]
        public IHttpActionResult CalculateSalary(int key, int month)
        {
            return Ok("CalculateSalary: Key(" + key + ")(" + month + ")");
        }

        [HttpPost]
        public IHttpActionResult UpdateAddress(int key)
        {
            return Ok("UpdateAddress: Key(" + key + ")");
        }

        [HttpGet]
        public IHttpActionResult GetCustomerByGender(Gender gender)
        {
            var customers = UriParseExtenstionDbContext.GetCustomers().Where(c => c.Gender == gender);
            return Ok(customers);
        }

        private string GetServiceRootUri()
        {
            var routeName = Request.ODataProperties().RouteName;
            ODataRoute odataRoute = Configuration.Routes[routeName] as ODataRoute;
            var prefixName = odataRoute.RoutePrefix;
            var requestUri = Request.RequestUri.ToString();
            var serviceRootUri = requestUri.Substring(0, requestUri.IndexOf(prefixName) + prefixName.Length);
            return serviceRootUri;
        }
    }

    public class OrdersController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(UriParseExtenstionDbContext.GetOrders());
        }

        public IHttpActionResult Get(int key)
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
