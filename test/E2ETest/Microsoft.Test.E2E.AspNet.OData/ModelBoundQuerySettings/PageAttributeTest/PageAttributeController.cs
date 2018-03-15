﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest
{
    public class CustomersController : TestODataController
    {
        private List<Customer> _customers;

        [EnableQuery]
        public List<Customer> Get()
        {
            Generate();
            return _customers;
        }

        [EnableQuery]
        public List<Order> GetOrders(int key)
        {
            Generate();
            return _customers[key].Orders;
        }

        [EnableQuery]
        public List<Address> GetAddresses(int key)
        {
            Generate();
            return _customers[key].Addresses;
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
                };

                _customers.Add(customer);
            }
        }
    }

    public class OrdersController : TestODataController
    {
        private List<Order> _orders;
        private List<SpecialOrder> _specialOrders;
        
        [EnableQuery]
        public List<Order> Get()
        {
            Generate();
            return _orders;
        }

        [EnableQuery]
        public List<Customer> GetCustomers(int key)
        {
            Generate();
            return _orders[key].Customers;
        }

        [EnableQuery]
        public List<SpecialOrder> GetFromSpecialOrder()
        {
            Generate();
            return _specialOrders;
        }

        public void Generate()
        {
            if (_orders == null)
            {
                _specialOrders = new List<SpecialOrder>();
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
                        }
                    };

                    _orders.Add(order);
                    var specialOrder = new SpecialOrder
                    {
                        Id = i,
                        SpecialName = "Special Order" + i
                    };

                    _specialOrders.Add(specialOrder);
                }
            }
        }
    }
}
