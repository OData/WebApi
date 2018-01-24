// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.CombinedTest
{
    public class CustomersController : ODataController
    {
        private static List<Customer> _customers;

        static CustomersController()
        {
            Generate();
        }

        [EnableQuery(MaxExpansionDepth = 10)]
        public List<Customer> Get()
        {
            return _customers;
        }

        [EnableQuery]
        public List<Order> GetOrders(int key)
        {
            return _customers[key].Orders;
        }

        [EnableQuery]
        public Order GetOrder(int key)
        {
            return _customers[key].Order;
        }

        [EnableQuery]
        public Customer GetFriend(int key)
        {
            return _customers[key];
        }

        private static void Generate()
        {
            _customers = new List<Customer>();
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + i,
                    Order = new Order
                    {
                        Id = i,
                        Name = "Order" + i,
                        Price = i * 100
                    },
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
                    AutoExpandOrder = new Order
                    {
                        Id = i,
                        Name = "AutoExpandOrder" + i
                    }
                };
                customer.CountableOrders = customer.Orders;
                _customers.Add(customer);
            }
        }
    }

    public class OrdersController : ODataController
    {
        private static List<Order> _orders;

        static OrdersController()
        {
            Generate();
        }

        [EnableQuery(MaxExpansionDepth = 6)]
        public List<Order> Get()
        {
            return _orders;
        }

        [EnableQuery]
        public List<Customer> GetCustomers2(int key)
        {
            return _orders[key].Customers2;
        }

        private static void Generate()
        {
            if (_orders == null)
            {
                _orders = new List<Order>();
                for (int i = 1; i < 10; i++)
                {
                    var order = new Order
                    {
                        Id = i,
                        Name = "Order" + i,
                        Customers = new List<Customer>
                    {
                        new Customer
                        {
                            Id = i,
                            Name = "Customer" + i
                        }
                    },
                        Customers2 = new List<Customer>
                    {
                        new Customer
                        {
                            Id = i,
                            Name = "Customer" + i
                        }
                    }
                    };

                    _orders.Add(order);
                }
            }
        }
    }
}
