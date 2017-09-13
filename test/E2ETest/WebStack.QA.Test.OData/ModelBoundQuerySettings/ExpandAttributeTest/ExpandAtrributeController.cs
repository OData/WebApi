// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData;

namespace WebStack.QA.Test.OData.ModelBoundQuerySettings.ExpandAttributeTest
{
    public class CustomersController : ODataController
    {
        private List<Customer> _customers;

        [EnableQuery(MaxExpansionDepth = 10)]
        public List<Customer> Get()
        {
            Generate();
            return _customers;
        }

        [EnableQuery(MaxExpansionDepth = 10)]
        public Customer Get(int key)
        {
            Generate();
            return _customers[key];
        }

        [EnableQuery]
        public List<Order> GetOrders(int key)
        {
            Generate();
            return _customers[key].Orders;
        }

        [EnableQuery]
        public Order GetOrder(int key)
        {
            Generate();
            return _customers[key].Order;
        }

        [EnableQuery]
        public Customer GetFriend(int key)
        {
            Generate();
            return _customers[key];
        }

        public void Generate()
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

                _customers.Add(customer);
            }

            _customers.Add(new SpecialCustomer
            {

                AutoExpandOrder = new Order
                {
                    Id = 10,
                    Name = "AutoExpandOrder" + 10
                },
                SpecialOrder = new Order
                {
                    Id = 10,
                    Name = "SpecialOrder" + 10
                }
            });
        }
    }

    public class OrdersController : ODataController
    {
        private List<Order> _orders;

        [EnableQuery(MaxExpansionDepth = 6)]
        public List<Order> Get()
        {
            Generate();
            return _orders;
        }

        [EnableQuery]
        public List<Customer> GetCustomers2(int key)
        {
            Generate();
            return _orders[key].Customers2;
        }

        public void Generate()
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
