// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.PageAttributeTest.SkipTokenTest
{
    public class CustomersController : TestODataController
    {
        private static List<Customer> _customers;

        [EnableQuery]
        public List<Customer> Get()
        {
            Generate();
            return _customers;
        }

        [EnableQuery(AllowedQueryOptions = Microsoft.AspNet.OData.Query.AllowedQueryOptions.SkipToken)]
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
            List<OrderDetail> details = new List<OrderDetail>
                            {
                                new OrderDetail()
                                {
                                    Id = 1,
                                    Name = "1stOrder",
                                },
                                new OrderDetail()
                                {
                                    Id = 2,
                                    Name = "2ndOrder",
                                },
                                new OrderDetail()
                                {
                                    Id = 3,
                                    Name = "3rdOrder"
                                },
                                new OrderDetail()
                                {
                                    Id = 4,
                                    Name = "4thOrder"
                                }
                            };
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
                    Token = i % 2 == 0 ? Guid.Parse("d1b6a349-e6d6-4fb2-91c6-8b2eceda85c7") : Guid.Parse("5af3c516-2d3c-4033-95af-07591f18439c"),
                    Skill = i % 2 == 0 ? Enums.Skill.CSharp : Enums.Skill.Sql,
                    DateTimeOfBirth = new DateTimeOffset(2000, 1, i, 0, 0, 0, new TimeSpan()),
                    Orders = new List<Order>
                    {
                        new Order
                        {
                            Id = i * 3 - 2,
                            Details = details

                        },
                        new Order
                        {
                            Id = i * 3-1,
                            Details = details
                        },
                        new Order
                        {
                            Id = i * 3,
                            Details = details
                        }
                    },
                    Addresses = new List<Address>()
                    {
                        new Address
                        {
                            Name = "CityA" + i
                        },
                        new Address
                        {
                            Name = "CityB" + i
                        },
                        new Address
                        {
                            Name = "CityC" + i
                        },
                    },
                };
                customer.DynamicProperties["DynamicProperty1"] = 10 - i;
                _customers.Add(customer);
            }
        }
    }

    public class OrdersController : TestODataController
    {
        private List<Order> _orders;
        private static List<SpecialOrder> _specialOrders;

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

    public class DatesController : TestODataController
    {
        private static readonly DateTime _baseDate = new DateTime(2019, 11, 09, 0, 0, 0, DateTimeKind.Utc);
        private readonly List<Date> _dates = Enumerable.Range(0, 5).Select(i => new Date() { DateValue = _baseDate.AddSeconds(i) }).ToList();

        [EnableQuery]
        public List<Date> Get() => _dates;
    }
}
