////-----------------------------------------------------------------------------
//// <copyright file="BulkOperationControllerEF.cs" company=".NET Foundation">
////      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
////      See License.txt in the project root for license information.
//// </copyright>
////------------------------------------------------------------------------------

//using System;
//using System.Collections.Generic;
//using System.Diagnostics.Contracts;
//using System.Linq;
//using Microsoft.AspNet.OData;
//using Microsoft.AspNet.OData.Extensions;
//using Microsoft.AspNet.OData.Routing;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Test.E2E.AspNet.OData.BulkOperation;
//using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
//using Xunit;
//using static Microsoft.Test.E2E.AspNet.OData.BulkOperation.APIHandlerFactoryEF;

//namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert
//{
//    public class EmployeesControllerEF : TestODataController
//    {
//        public EmployeesControllerEF()
//        {

//        }

//        public static List<Employee> employees;
//        public static List<Friend> friends;

//        internal DbSet<Employee> GenerateData(EmployeeDBContext context)
//        {
//            if (context.Employees.Any())
//            {
//                return context.Employees;
//            }

//            var friends = GenerateDataOrders(context);

//            employees = new List<Employee>();
//            employees.Add(new Employee { ID = 1, Name = "Employee1", Friends = friends.Where(x => x.Id == 1 || x.Id == 2).ToList() });
//            employees.Add(new Employee { ID = 2, Name = "Employee2", Friends = friends.Where(x => x.Id == 3 || x.Id == 4).ToList() });
//            employees.Add(new Employee { ID = 3, Name = "Employee3", Friends = friends.Where(x => x.Id == 5 || x.Id == 6).ToList() });

//            context.Employees.AddRange(employees);

//            context.SaveChanges();

//            return context.Employees;
//        }

//        internal DbSet<Friend> GenerateDataOrders(EmployeeDBContext context)
//        {
//            if (context.Friends.Any())
//            {
//                return context.Friends;
//            }

//            friends = new List<Friend>();
//            friends.Add(new Friend { Id = 1, Age = 10, Orders = new List<Order>() { new Order { Id = 1, Price = 5 }, new Order { Id = 2, Price = 5 } } });
//            friends.Add(new Friend { Id = 2, Age = 20, Orders = new List<Order>() { new Order { Id = 10, Price = 5 }, new Order { Id = 20, Price = 5 } } });
//            friends.Add(new Friend { Id = 3, Age = 30, Orders = new List<Order>() { new Order { Id = 3, Price = 5 }, new Order { Id = 4, Price = 5 } } });
//            friends.Add(new Friend { Id = 4, Age = 40, Orders = new List<Order>() { new Order { Id = 30, Price = 5 }, new Order { Id = 40, Price = 5 } } });
//            friends.Add(new Friend { Id = 5, Age = 50, Orders = new List<Order>() { new Order { Id = 5, Price = 5 }, new Order { Id = 6, Price = 5 } } });
//            friends.Add(new Friend { Id = 6, Age = 60, Orders = new List<Order>() { new Order { Id = 50, Price = 5 }, new Order { Id = 60, Price = 5 } } });

//            context.Friends.AddRange(friends);

//            context.SaveChanges();

//            return context.Friends;
//        }


//        [ODataRoute("Employees")]
//        [HttpPatch]
//        public ITestActionResult PatchEmployees([FromBody] DeltaSet<Employee> coll)
//        {
//            using (var dbContext = CreateDbContext())
//            {
//                GenerateData(dbContext);

//                Assert.NotNull(coll);

//                var returncoll = coll.Patch(new EmployeeEFPatchHandler(dbContext), new APIHandlerFactoryEF(Request.GetModel(), dbContext));


//                return Ok(returncoll);
//            }
//        }

//        private EmployeeDBContext CreateDbContext()
//        {
//            var buiilder = new DbContextOptionsBuilder<EmployeeDBContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
//            var dbContext = new EmployeeDBContext(buiilder.Options);
//            return dbContext;
//        }

//        [ODataRoute("Employees({key})")]
//        public ITestActionResult Patch(int key, [FromBody] Delta<Employee> delta)
//        {
//            using (var dbContext = CreateDbContext())
//            {
//                GenerateData(dbContext);

//                delta.TrySetPropertyValue("ID", key); // It is the key property, and should not be updated.
//                object obj;
//                delta.TryGetPropertyValue("Friends", out obj);

//                var employee = dbContext.Employees.First(x => x.ID == key);

//                try
//                {
//                    delta.Patch(employee, new EmployeeEFPatchHandler(dbContext), new APIHandlerFactoryEF(Request.GetModel(), dbContext));

//                }
//                catch (ArgumentException ae)
//                {
//                    return BadRequest(ae.Message);
//                }

//                employee = dbContext.Employees.First(x => x.ID == key);

//                ValidateFriends(key, employee);

//                return Ok(employee);
//            }
//        }

//        private static void ValidateFriends(int key, Employee employee)
//        {
//            if (key == 1 && employee.Name == "SqlUD")
//            {
//                Contract.Assert(employee.Friends.Count == 2);
//                Contract.Assert(employee.Friends[0].Id == 2);
//                Contract.Assert(employee.Friends[1].Id == 3);
//            }
//            else if (key == 1 && employee.Name == "SqlFU")
//            {
//                Contract.Assert(employee.Friends.Count == 3);
//                Contract.Assert(employee.Friends[0].Id == 345);
//                Contract.Assert(employee.Friends[1].Id == 400);
//                Contract.Assert(employee.Friends[2].Id == 900);
//            }
//            else if (key == 1 && employee.Name == "SqlMU")
//            {
//                Contract.Assert(employee.Friends.Count == 3);
//                Contract.Assert(employee.Friends[0].Id == 2);
//                Contract.Assert(employee.Friends[1].Id == 1);
//                Contract.Assert(employee.Friends[1].Name == "Test_1");
//                Contract.Assert(employee.Friends[2].Id == 3);
//            }
//            else if (key == 1 && employee.Name == "SqlMU1")
//            {
//                Contract.Assert(employee.Friends.Count == 2);
//                Contract.Assert(employee.Friends[0].Id == 2);
//                Contract.Assert(employee.Friends[1].Id == 3);
//            }
//        }

//        [ODataRoute("Employees({key})/Friends")]
//        [HttpPatch]
//        public ITestActionResult PatchFriends(int key, [FromBody] DeltaSet<Friend> friendColl)
//        {
//            using (var dbContext = CreateDbContext())
//            {
//                GenerateData(dbContext);

//                Employee originalEmployee = dbContext.Employees.SingleOrDefault(c => c.ID == key);
//                Assert.NotNull(originalEmployee);

//                var changedObjColl = friendColl.Patch(originalEmployee.Friends);

//                return Ok(changedObjColl);
//            }
//        }

//        public ITestActionResult Get(int key)
//        {
//            using (var dbContext = CreateDbContext())
//            {
//                var emp = dbContext.Employees.SingleOrDefault(e => e.ID == key);
//                return Ok(emp);
//            }
//        }

//        [ODataRoute("Employees({key})/Friends")]
//        public ITestActionResult GetFriends(int key)
//        {
//            using (var dbContext = CreateDbContext())
//            {
//                var emp = dbContext.Employees.SingleOrDefault(e => e.ID == key);
//                return Ok(emp.Friends);
//            }
//        }


//    }
//}