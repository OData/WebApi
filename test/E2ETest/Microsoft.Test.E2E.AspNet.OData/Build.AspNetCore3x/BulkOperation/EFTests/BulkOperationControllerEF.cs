// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Test.E2E.AspNet.OData.BulkOperation;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert
{
    public class EmployeesControllerEF : TestODataController
    {
        public EmployeesControllerEF()
        {
           
        }

        public static EmployeeDBContext dbContext;
        public static List<Employee> employees;
        public static List<Friend> friends;

        private IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddDbContext<EmployeeDBContext>(opt => opt.UseInMemoryDatabase(databaseName: "InMemoryDb"));

            return services.BuildServiceProvider();
        }

        public static DbSet<Employee> GenerateData(EmployeeDBContext context)
        {
            if (context.Employees.Any())
            {
                return context.Employees;
            }

            var friends = GenerateDataOrders(context);

            employees = new List<Employee>();
            employees.Add(new Employee { ID = 1, Name = "Employee1", Friends = friends.Where(x => x.Id == 1 || x.Id == 2).ToList() });
            employees.Add(new Employee { ID = 2, Name = "Employee2", Friends = friends.Where(x => x.Id == 3 || x.Id == 4).ToList() });
            employees.Add(new Employee { ID = 3, Name = "Employee3", Friends = friends.Where(x => x.Id == 5 || x.Id == 6).ToList() });

            context.Employees.AddRange(employees);

            context.SaveChanges();

            return context.Employees;
        }

        public static DbSet<Friend> GenerateDataOrders(EmployeeDBContext context)
        {
            if (context.Friends.Any())
            {
                return context.Friends;
            }

            friends = new List<Friend>();
            friends.Add(new Friend { Id = 1, Age = 10 ,  Orders = new List<Order>() { new Order { Id = 1, Price = 5 }, new Order { Id = 2, Price = 5 } } });
            friends.Add(new Friend { Id = 2, Age = 20, Orders = new List<Order>() { new Order { Id = 10, Price = 5 }, new Order { Id = 20, Price = 5 } } });
            friends.Add(new Friend { Id = 3, Age = 30, Orders = new List<Order>() { new Order { Id = 3, Price = 5 }, new Order { Id = 4, Price = 5 } } });
            friends.Add(new Friend { Id = 4, Age = 40, Orders = new List<Order>() { new Order { Id = 30, Price = 5 }, new Order { Id = 40, Price = 5 } } });
            friends.Add(new Friend { Id = 5, Age = 50, Orders = new List<Order>() { new Order { Id = 5, Price = 5 }, new Order { Id = 6, Price = 5 } } });
            friends.Add(new Friend { Id = 6, Age = 60, Orders = new List<Order>() { new Order { Id = 50, Price = 5 }, new Order { Id = 60, Price = 5 } } });

            context.Friends.AddRange(friends);

            context.SaveChanges();

            return context.Friends;
        }


        [ODataRoute("Employees")]
        [HttpPatch]
        public ITestActionResult PatchEmployees([FromBody] DeltaSet<Employee> coll)
        {
            var serviceProvider = BuildServiceProvider();
            dbContext = serviceProvider.GetService<EmployeeDBContext>();
            GenerateData(dbContext);

            Assert.NotNull(coll);

            var returncoll = coll.Patch(new EmployeeEFPatchHandler()) ;

            return Ok(returncoll);
        }

        [ODataRoute("Employees({key})")]
        public ITestActionResult Patch(int key, [FromBody] Delta<Employee> delta)
        {
            var serviceProvider = BuildServiceProvider();
            dbContext = serviceProvider.GetService<EmployeeDBContext>();
            GenerateData(dbContext);

            delta.TrySetPropertyValue("ID", key); // It is the key property, and should not be updated.
            object obj;
            delta.TryGetPropertyValue("Friends", out obj);

            var employee = dbContext.Employees.First(x => x.ID == key);

            try
            {
                delta.Patch(employee, new EmployeeEFPatchHandler());
                dbContext.SaveChanges();
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }

            employee = dbContext.Employees.First(x => x.ID == key);

            Contract.Assert(employee.Friends.Count ==1);

            return Ok(employee);
        }


    }
}