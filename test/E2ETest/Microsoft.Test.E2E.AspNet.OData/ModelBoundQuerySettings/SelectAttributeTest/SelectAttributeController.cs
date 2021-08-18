//-----------------------------------------------------------------------------
// <copyright file="SelectAttributeController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBoundQuerySettings.SelectAttributeTest
{
    public class CustomersController : TestODataController
    {
        private List<Customer> _customers;

        [EnableQuery(MaxExpansionDepth = 10)]
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

        public void Generate()
        {
            _customers = new List<Customer>();
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

                _customers.Add(customer);
            }
        }
    }

    public class OrdersController : TestODataController
    {
        private List<Order> _orders;
        private List<SpecialOrder> _specialOrders;

        [EnableQuery(MaxExpansionDepth = 6)]
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
                        },
                        Cars = new List<Car>
                        {
                            new Car
                            {
                                Id = i,
                                Name = "Car" + i
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

    public class CarsController : TestODataController
    {
        private List<Car> _cars;

        [EnableQuery]
        public List<Car> Get()
        {
            Generate();
            return _cars;
        }

        public void Generate()
        {
            if (_cars == null)
            {
                _cars = new List<Car>();
                for (int i = 1; i < 10; i++)
                {
                    var car = new Car
                    {
                        Id = i,
                        Name = "Order" + i,
                    };

                    _cars.Add(car);
                }
            }
        }
    }

    public class AutoSelectCustomersController : TestODataController
    {
        private List<AutoSelectCustomer> _customers;

        [EnableQuery]
        public List<AutoSelectCustomer> Get()
        {
            Generate();
            return _customers;
        }

        [EnableQuery]
        public AutoSelectCustomer Get(int key)
        {
            Generate();
            return _customers[key];
        }

        [EnableQuery]
        public AutoSelectOrder GetOrder(int key)
        {
            Generate();
            return _customers[key].Order;
        }

        [EnableQuery]
        public AutoSelectCar GetCar(int key)
        {
            Generate();
            return _customers[key].Car;
        }

        public void Generate()
        {
            _customers = new List<AutoSelectCustomer>();
            for (int i = 1; i < 10; i++)
            {
                var customer = new AutoSelectCustomer
                {
                    Id = i,
                    Name = "Customer" + i,
                    Order = new AutoSelectOrder
                    {
                        Id = i,
                        Name = "AutoExpandOrder" + i
                    },
                    Car = new AutoSelectCar
                    {
                        Id = i,
                        Name = "Car" + i
                    }
                };

                _customers.Add(customer);
            }

            _customers.Add(new SpecialCustomer
            {
                Id = 10,
                VIPNumber = "VIP 10"
            });
            _customers[1].Order.Customer = _customers[2];
        }
    }
}
