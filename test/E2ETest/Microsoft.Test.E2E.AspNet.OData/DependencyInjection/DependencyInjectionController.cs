//-----------------------------------------------------------------------------
// <copyright file="DependencyInjectionController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.DependencyInjection
{
    public class CustomersController : TestODataController
    {
        [EnableQuery]
        public List<Customer> Get()
        {
            List<Customer> customers = Generate();
            return customers;
        }

        [EnableQuery]
        public Customer Get(int key)
        {
            List<Customer> customers = Generate();
            return customers[key - 1];
        }

        [HttpGet]
        public CustomerType EnumFunction()
        {
            return CustomerType.Normal;
        }

        [EnableQuery]
        public List<Order> GetOrders(int key)
        {
            List<Customer> customers = Generate();
            return customers[key].Orders;
        }

        [EnableQuery]
        public List<Address> GetAddresses(int key)
        {
            List<Address> addresses = new List<Address>();
            for (int i = 1; i < 10; i++)
            {
                var address = new Address
                {
                    Name = "Address" + i
                };

                addresses.Add(address);
            }

            return addresses;
        }

        public static List<Customer> Generate()
        {
            List<Customer> customers = new List<Customer>();
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + i,
                    Address = new Address
                    {
                        Name = "City" + i,
                        Street = "Street" + i,
                    },
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            Id = i * 2 - 1
                        },
                        new Order
                        {
                            Id = i * 2
                        }
                    },
                };

                customers.Add(customer);
            }

            return customers;
        }
    }

    public class OrdersController : TestODataController
    {
        [EnableQuery]
        public List<Order> Get()
        {
            List<Order> orders = Generate();
            return orders;
        }

        public static List<Order> Generate()
        {
            List<Order> orders = new List<Order>();
            for (int i = 1; i < 10; i++)
            {
                var order = new Order
                {
                    Id = i,
                    Name = "Order" + i
                };

                orders.Add(order);
            }

            return orders;
        }
    }
}
