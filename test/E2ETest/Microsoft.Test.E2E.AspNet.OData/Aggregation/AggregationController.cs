//-----------------------------------------------------------------------------
// <copyright file="AggregationController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation
{
    public class BaseCustomersController : TestODataController
#if NETCORE
        , IDisposable
#endif
    {
        protected readonly AggregationContext _db = new AggregationContext();
        protected readonly List<Customer> _customers = new List<Customer>();

        public void Generate()
        {
            for (int i = 1; i < 10; i++)
            {
                var customer = new Customer
                {
                    Id = i,
                    Name = "Customer" + i % 2,
                    Bucket = i % 2 == 0? (CustomerBucket?)CustomerBucket.Small : null,
                    Order = new Order
                    {
                        Id = i,
                        Name = "Order" + i % 2,
                        Price = i * 100
                    },
                    Address = new Address
                    {
                        Name = "City" + i % 2,
                        Street = "Street" + i % 2,
                    }
                };

                _customers.Add(customer);
            }

            _customers.Add(new Customer()
            {
                Id = 10,
                Name = null,
                Bucket = CustomerBucket.Big,
                Address = new Address
                {
                    Name = "City1",
                    Street = "Street",
                },
                Order = new Order
                {
                    Id = 10,
                    Name = "Order" + 10 % 2,
                    Price = 0
                },
            });

            SaveGenerated();
        }


        protected virtual void SaveGenerated()
        {
            _db.Customers.AddRange(_customers);
            _db.SaveChanges();
        }

        protected virtual void ResetDataSource()
        {
            if (!_db.Customers.Any())
            {
                Generate();
            }
            CleanCommands();
        }


        public virtual string LastCommand()
        {
            return null;
        }

        public virtual bool CleanCommands()
        {
            return true;
        }

#if NETCORE
        public void Dispose()
        {
            //_db.Dispose();
        }
#endif
    }

    public class CustomersController : BaseCustomersController
    {
          [EnableQuery]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new AggregationContext();
            return db.Customers;
        }

        [EnableQuery]
        public TestSingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new AggregationContext();
            return TestSingleResult.Create(db.Customers.Where(c => c.Id == key));
        }


        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetLastCommand()")]
        public override string LastCommand()
        {
            return AggregationContext.LastCommand;
        }

        [HttpGet]
        [ODataRoute("CleanCommands()")]
        public override bool CleanCommands()
        {
            AggregationContext.CleanCommands();
            return true;
        }
    }

#if !NETCORE
    public class LinqToSqlCustomersController : BaseCustomersController
    {
        [EnableQuery]
        [ODataRoute("Customers")]
        public IQueryable<Customer> Get()
        {
            var db = new LinqToSqlDatabaseContext();
            return db.Customers;
        }

        [EnableQuery]
        public TestSingleResult<Customer> Get(int key)
        {
            throw new NotSupportedException();
        }
    }
#endif

#if NETCORE
    public class CoreCustomersController<T> : BaseCustomersController where T: AggregationContextCoreBase, new()
    {
        [EnableQuery]
        [ODataRoute("Customers")]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new T();
            return db.Customers;
        }

        [EnableQuery]
        public TestSingleResult<Customer> Get(int key)
        {
            ResetDataSource();
            var db = new T();
            return TestSingleResult.Create(db.Customers.Where(c => c.Id == key));
        }


        protected override void SaveGenerated()
        {
            var db = new T();
            db.Customers.AddRange(_customers);
            db.SaveChanges();
        }

        protected override void ResetDataSource()
        {
            var db = new T();
            db.Database.EnsureCreated();
            if (!db.Customers.Any())
            {
                Generate();
            }
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("GetLastCommand()")]
        public override string LastCommand()
        {
            return TraceLoggerProvider.CurrentSQL;
        }

        [HttpGet]
        [ODataRoute("CleanCommands()")]
        public override bool CleanCommands()
        {
            TraceLoggerProvider.CleanCommands();
            return true;
        }
    }
#endif
}
