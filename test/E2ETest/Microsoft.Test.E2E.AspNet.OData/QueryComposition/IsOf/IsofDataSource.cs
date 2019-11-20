// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf
{
    public class IsofDataSource : IDisposable
    {
        private static BillingCustomerContextTest _context = null;
        private static IEnumerable<BillingCustomer> _customers = null;
        private static IEnumerable<BillingDetail> _billings = null;

        public static IEnumerable<BillingDetail> InMemoryBillings
        {
            get
            {
                if (_billings == null)
                {
                    IList<BillingDetail> billings = new List<BillingDetail>();

                    // #1. Base type instance
                    BillingDetail billing = new BillingDetail
                    {
                        Id = 1,
                        Owner = "John"
                    };
                    billings.Add(billing);

                    // #2. two subclass instances
                    billing = new CreditCard
                    {
                        Id = 2,
                        Owner = "Mike",
                        CardType = CardType.Rewards,
                        ExpiryYear = 2018
                    };
                    billings.Add(billing);

                    billing = new BankAccount
                    {
                        Id = 3,
                        Owner = "Tony",
                        BankName = "Universal Bank"
                    };
                    billings.Add(billing);

                    _billings = new List<BillingDetail>(billings);
                }

                return _billings;
            }
        }

        public static IEnumerable<BillingCustomer> InMemoryCustomers
        {
            get
            {
                if (_customers == null)
                {
                    IEnumerable<BillingDetail> billings = InMemoryBillings;

                    IList<BillingCustomer> customers = new List<BillingCustomer>();

                    BillingAddress cnAddress = new BillingCnAddress
                    {
                        City = "Shanghai",
                        PostCode = 2001100
                    };

                    BillingAddress usAddress = new BillingUsAddress
                    {
                        City = "Redmond",
                        Street = "Microsoft one way"
                    };

                    int customerId = 1;
                    foreach (var billing in billings)
                    {
                        BillingCustomer customer = new BillingCustomer
                        {
                            CustomerId = customerId++,
                            Name = billing.Owner,
                            Birthday = new DateTimeOffset(2015, 2, 8, 1, 2, 3, 4, new TimeSpan(0, 8, 0)),
                            Token = customerId % 2 == 0 ? (Guid?)null : Guid.NewGuid(),
                            CustomerType = customerId % 2 ==0 ? CustomerType.Normal : CustomerType.Vip,
                            Address = customerId % 2 == 0 ? cnAddress : usAddress,
                            Billing = billing
                        };

                        customers.Add(customer);
                    }

                    _customers = new List<BillingCustomer>(customers);
                }

                return _customers;
            }
        }

        public static IEnumerable<BillingCustomer> EfCustomers
        {
            get
            {
                GenertateEFData();
                var context = new BillingCustomerContextTest();
                return context.Customers;
            }
        }

        public static IEnumerable<BillingDetail> EfBillings
        {
            get
            {
                GenertateEFData();
                var context = new BillingCustomerContextTest();
                return context.Billings;
            }
        }
        private static void GenertateEFData()
        {
            if (_context == null)
            {
                _context = new BillingCustomerContextTest();

                if (!_context.Customers.Any())
                {
                    foreach (var customer in InMemoryCustomers)
                    {
                        _context.Customers.Add(customer);
                    }

                    foreach (var billing in InMemoryBillings)
                    {
                        _context.Billings.Add(billing);
                    }

                    _context.SaveChanges();
                }
            }
        }

        public void Dispose()
        {
            // _context.Dispose();
        }
    }

    public class BillingCustomerContextTest : DbContext
    {
        public static string ConnectionString =
            @"Data Source=(LocalDb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=BillingCustomerContextTest1";

        public BillingCustomerContextTest()
            : base(ConnectionString)
        {
        }

        public DbSet<BillingCustomer> Customers { get; set; }

        public DbSet<BillingDetail> Billings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BillingCustomer>();
            modelBuilder.ComplexType<BillingCnAddress>();
            modelBuilder.ComplexType<BillingUsAddress>();
            modelBuilder.ComplexType<BillingAddress>();
        }
    }
}
