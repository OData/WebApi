// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.
#if NETCOREAPP3_1

using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Test.Builder.Conventions.Attributes;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.Linq;
using Xunit;
using static Microsoft.AspNet.OData.PatchMethodHandler;

namespace Microsoft.AspNet.OData.Test.DeltaSet
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Order> Orders { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public int Price { get; set; }
    }

    public class CustomerDBContext: DbContext
    {
        public CustomerDBContext()
        {

        }

        public CustomerDBContext(DbContextOptions<CustomerDBContext> options) : base(options)
        {

        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasKey(c => c.Id);
            modelBuilder.Entity<Order>().HasKey(c => c.Id);
        }
    }


    public class DeltaSetTest
    {
        public static List<Customer> customers;
        public static List<Order> orders;

        CustomerDBContext dbContext;

        private IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddDbContext<CustomerDBContext>(opt => opt.UseInMemoryDatabase(databaseName: "InMemoryDb"));

            return services.BuildServiceProvider();
        }

        public static DbSet<Customer> GenerateData(CustomerDBContext context)
        {
            if (context.Customers.Any())
            {
                return context.Customers;
            }

            var orders = GenerateDataOrders(context);

            customers = new List<Customer>();
            customers.Add(new Customer { Id = 1, Name = "Customer1" , Orders = orders.Where(x=>x.Id ==1 || x.Id ==2).ToList() });
            customers.Add(new Customer { Id = 2, Name = "Customer2", Orders = orders.Where(x => x.Id == 3 || x.Id == 4).ToList() });
            customers.Add(new Customer { Id = 3, Name = "Customer3", Orders = orders.Where(x => x.Id == 5 || x.Id == 6).ToList() });

            context.Customers.AddRange(customers);

            context.SaveChanges();

            return context.Customers;
        }

        public static DbSet<Order> GenerateDataOrders(CustomerDBContext context)
        {
            if (context.Orders.Any())
            {
                return context.Orders;
            }

            orders = new List<Order>();
            orders.Add(new Order { Id = 1, Price = 10 });
            orders.Add(new Order { Id = 2, Price = 20 });
            orders.Add(new Order { Id = 3, Price = 30 });
            orders.Add(new Order { Id = 4, Price = 40 });
            orders.Add(new Order { Id = 5, Price = 50 });
            orders.Add(new Order { Id = 6, Price = 60 });

            context.Orders.AddRange(orders);

            context.SaveChanges();

            return context.Orders;
        }

        [Fact]
        public void DeltaSet_Patch()
        {
            //Assign
            EdmEntityType _entityType = new EdmEntityType("namespace Microsoft.AspNet.OData.Test", "Friend");
            _entityType.AddKeys(_entityType.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32));

            var lstId = new List<string>() { "Id" };
            var custDeltaSet = new DeltaSet<Customer>(lstId);

            custDeltaSet.CreateDelegate = new TryCreate(TryCreateCustomer);
            custDeltaSet.GetDelegate = new TryGet(TryGetCustomer);
            custDeltaSet.DeleteDelegate = new TryDelete(TryDeleteCustomer);

            var deltaCust = new Delta<Customer>(typeof(Customer), (typeof(Customer)).GetProperties().Select(x => x.Name).ToList());
            deltaCust.TrySetPropertyValue("Id", 2);
            deltaCust.TrySetPropertyValue("Name", "Name2");

            var deltaSetOrder = new DeltaSet<Order>(lstId);

            deltaSetOrder.CreateDelegate = new TryCreate(TryCreateOrder);
            deltaSetOrder.GetDelegate = new TryGet(TryGetOrder);
            deltaSetOrder.DeleteDelegate = new TryDelete(TryDeleteOrder);

            var deltaOrder = new DeltaDeletedEntityObject<Order>(typeof(Order), (typeof(Order)).GetProperties().Select(x => x.Name).ToList());
            deltaOrder.TrySetPropertyValue("Id", 3);

            var deltaOrder1 = new Delta<Order>(typeof(Order), (typeof(Order)).GetProperties().Select(x => x.Name).ToList());
            deltaOrder1.TrySetPropertyValue("Id", 4);
            deltaOrder1.TrySetPropertyValue("Price", 400);

            deltaSetOrder.Add(deltaOrder);
            deltaSetOrder.Add(deltaOrder1);

            deltaCust.TrySetPropertyValue("Orders", deltaSetOrder);

            custDeltaSet.Add(deltaCust);

            var serviceProvider = BuildServiceProvider();

            //Act
            dbContext = serviceProvider.GetService<CustomerDBContext>();
            
            GenerateData(dbContext);
                        
            custDeltaSet.Patch();

            dbContext.SaveChanges();

            var lstOrders= dbContext.Customers.First(x => x.Id == 2).Orders;

            //Assert
            Assert.Single(lstOrders);
            Assert.Equal(400, lstOrders.First().Price);            

        }

        private ResponseStatus TryGetCustomer(IDictionary<string, object> keyValues, out object originalObject, out string errorMessage)
        {
            ResponseStatus status = ResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues.First().Value.ToString();
                originalObject = dbContext.Customers.First(x=>x.Id == Int32.Parse(id));

                if (originalObject == null)
                {
                    status = ResponseStatus.NotFound;
                }
                
            }
            catch (Exception ex)
            {
                status = ResponseStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        private bool TryCreateCustomer(out object createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Customer();
                dbContext.Customers.Add(createdObject as Customer);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return false;
            }
        }

        private bool TryDeleteCustomer(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var customer = dbContext.Customers.First(x => x.Id == Int32.Parse(id));

                dbContext.Customers.Remove(customer);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return false;
            }
        }

        private ResponseStatus TryGetOrder(IDictionary<string, object> keyValues, out object originalObject, out string errorMessage)
        {
            ResponseStatus status = ResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues.First().Value.ToString();
                originalObject = dbContext.Orders.First(x => x.Id == Int32.Parse(id));

                if (originalObject == null)
                {
                    status = ResponseStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = ResponseStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        private bool TryCreateOrder(out object createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Order();
                dbContext.Orders.Add(createdObject as Order);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return false;
            }
        }

        private bool TryDeleteOrder(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var order = dbContext.Orders.First(x => x.Id == Int32.Parse(id));

                dbContext.Orders.Remove(order);

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return false;
            }
        }


    }


}
#endif