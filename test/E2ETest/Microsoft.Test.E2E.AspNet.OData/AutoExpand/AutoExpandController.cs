//-----------------------------------------------------------------------------
// <copyright file="AutoExpandController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.AutoExpand
{
    public class CustomersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private readonly AutoExpandCustomerContext _db = new AutoExpandCustomerContext();

        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new AutoExpandCustomerContext();
            return db.Customers;
        }

        [EnableQuery]
        public TestSingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new AutoExpandCustomerContext();
            return TestSingleResult.Create(db.Customers.Where(c => c.Id == key));
        }

        public void Generate()
        {
            Customer previousCustomer = null;
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Order = new Order
                    {
                        Id = i,
                        Choice = new ChoiceOrder
                        {
                            Id = i,
                            Amount = i * 1000
                        }
                    },
                };

                if (i > 1)
                {
                    customer.Friend = previousCustomer;
                }

                // For customer whose id is 8 will have SpecialOrder with SpecialChoice.
                if (i == 8)
                {
                    customer.Order = new SpecialOrder
                    {
                        Id = i,
                        Choice = new ChoiceOrder
                        {
                            Id = i,
                            Amount = i * 1000
                        },
                        SpecialChoice = new ChoiceOrder()
                        {
                            Id = i * 100,
                            Amount = i * 2000
                        }
                    };
                }

                // For customer whose id is 9 will have VipOrder with SpecialChoice and VipChoice.
                if (i == 9)
                {
                    customer.Order = new VipOrder
                    {
                        Id = i,
                        Choice = new ChoiceOrder
                        {
                            Id = i,
                            Amount = i * 1000
                        },
                        SpecialChoice = new ChoiceOrder()
                        {
                            Id = i * 100,
                            Amount = i * 2000
                        },
                        VipChoice = new ChoiceOrder()
                        {
                            Id = i * 1000,
                            Amount = i * 3000
                        }
                    };
                }

                _db.Customers.Add(customer);
                previousCustomer = customer;
            }

            _db.SaveChanges();
        }

        private void ResetDataSource()
        {
            if (!_db.Customers.Any())
            {
                Generate();
            }
        }

#if NETCORE
        public void Dispose()
        {
            //_db.Dispose();
        }
#endif
    }

    public class PeopleController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private readonly AutoExpandPeopleContext _db = new AutoExpandPeopleContext();

        [EnableQuery(MaxExpansionDepth = 4)]
        public IQueryable<People> Get()
        {
            ResetDataSource();
            var db = new AutoExpandPeopleContext();
            return db.People;
        }

        public void Generate()
        {
            People previousPeople = null;
            for (int i = 1; i < 10; i++)
            {
                var people = new People
                {
                    Id = i,
                    Order = new Order
                    {
                        Id = i,
                        Choice = new ChoiceOrder
                        {
                            Id = i,
                            Amount = i * 1000
                        }
                    },
                };

                if (i > 1)
                {
                    people.Friend = previousPeople;
                }

                _db.People.Add(people);
                previousPeople = people;
            }

            _db.SaveChanges();
        }

        private void ResetDataSource()
        {
            if (!_db.People.Any())
            {
                Generate();
            };
        }

#if NETCORE
        public void Dispose()
        {
            // _db.Dispose();
        }
#endif
    }

    public class NormalOrdersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        private readonly AutoExpandOrdersContext _db = new AutoExpandOrdersContext();

        [EnableQuery]
        public IQueryable<NormalOrder> Get()
        {
            ResetDataSource();
            var db = new AutoExpandOrdersContext();
            return db.NormalOrders;
        }

        [EnableQuery]
        public TestSingleResult<NormalOrder> Get(int key)
        {
            ResetDataSource();
            var db = new AutoExpandOrdersContext();
            return TestSingleResult.Create(db.NormalOrders.Where(o => o.Id == key));
        }

        public void Generate()
        {
            var order2 = new DerivedOrder
            {
                Id = 2,
                OrderDetail = new OrderDetail
                {
                    Id = 3,
                    Description = "OrderDetail"
                },
                NotShownDetail = new OrderDetail
                {
                    Id = 4,
                    Description = "NotShownOrderDetail4"
                }
            };

            var order1 = new DerivedOrder
            {
                Id = 1,
                OrderDetail = new OrderDetail
                {
                    Id = 1,
                    Description = "OrderDetail"
                },
                NotShownDetail = new OrderDetail
                {
                    Id = 2,
                    Description = "NotShownOrderDetail2"
                }
            };

            var order3 = new DerivedOrder2
            {
                Id = 3,
                NotShownDetail = new OrderDetail
                {
                    Id = 5,
                    Description = "NotShownOrderDetail4"
                }
            };

            order2.LinkOrder = order1;
            _db.NormalOrders.Add(order2);
            _db.NormalOrders.Add(order3);
            _db.SaveChanges();
        }

        private void ResetDataSource()
        {
            if (!_db.NormalOrders.Any())
            {
                Generate();
            };
        }

#if NETCORE
        public void Dispose()
        {
            //_db.Dispose();
        }
#endif
    }
}
