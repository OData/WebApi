// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Test.E2E.AspNet.OData.BulkInsert;

namespace Microsoft.Test.E2E.AspNet.OData.BulkOperation
{
    public class EmployeeDBContext : DbContext
    {
        public EmployeeDBContext()
        {

        }

        public EmployeeDBContext(DbContextOptions<EmployeeDBContext> options) : base(options)
        {

        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Friend> Friends { get; set; }

        protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasKey(c => c.ID);
            modelBuilder.Entity<Employee>().Ignore(c => c.SkillSet);
            modelBuilder.Entity<Employee>().Ignore(c => c.NewFriends);
            modelBuilder.Entity<Employee>().Ignore(c => c.UnTypedFriends);
            modelBuilder.Entity<Employee>().Ignore(c => c.InstanceAnnotations);

            modelBuilder.Entity<NewFriend>().Ignore(c => c.InstanceAnnotations);
            modelBuilder.Entity<UnTypedFriend>().Ignore(c => c.InstanceAnnotations);

            modelBuilder.Entity<Friend>().HasKey(c => c.Id);

            modelBuilder.Entity<NewOrder>().Ignore(c => c.Container);
            modelBuilder.Entity<MyNewOrder>().Ignore(c => c.Container);
        }

       
    }

    public class APIHandlerFactoryEF: ODataAPIHandlerFactory
    {
        EmployeeDBContext dbContext;

        public APIHandlerFactoryEF()
        {

        }

        public APIHandlerFactoryEF(EmployeeDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public override IODataAPIHandler GetHandler(NavigationPath navigationPath)
        {
            if (navigationPath != null)
            {
                var pathItems = navigationPath.GetNavigationPathItems();

                if (pathItems == null)
                {
                    switch (navigationPath.NavigationPathName)
                    {
                        case "Employees":
                        case "Employee":
                            return new EmployeeEFPatchHandler(dbContext);
                      
                        case "Company":
                            return new CompanyAPIHandler();
                        default:
                            return null;
                    }
                }
            }

            return null;

    }

    public class EmployeeEFPatchHandler : ODataAPIHandler<Employee>
    {
        EmployeeDBContext dbContext = null;

        public EmployeeEFPatchHandler(EmployeeDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out Employee createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Employee();
                dbContext.Employees.Add(createdObject);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var customer = dbContext.Employees.First(x => x.ID == Int32.Parse(id));

                dbContext.Employees.Remove(customer);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out Employee originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["ID"].ToString();
                originalObject = dbContext.Employees.First(x => x.ID == Int32.Parse(id));


                if (originalObject == null)
                {
                    status = ODataAPIResponseStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = ODataAPIResponseStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override IODataAPIHandler GetNestedHandler(Employee parent, string navigationPropertyName)
        {
            switch (navigationPropertyName)
            {
                case "Friends":
                    return new FriendEFPatchHandler(parent);
                case "NewFriends":
                    return new NewFriendEFPatchHandler(parent);
                default:
                    return null;
            }
            
        }

    }

    public class FriendEFPatchHandler : ODataAPIHandler<Friend>
    {
        Employee employee;
        public FriendEFPatchHandler(Employee employee)
        {
            this.employee = employee;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out Friend createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Friend();
                employee.Friends.Add(createdObject);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var friend = employee.Friends.First(x => x.Id == Int32.Parse(id));

                employee.Friends.Remove(friend);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out Friend originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                if (employee.Friends == null)
                {
                    status = ODataAPIResponseStatus.NotFound;
                }
                else
                {
                    originalObject = employee.Friends.FirstOrDefault(x => x.Id == Int32.Parse(id));
                }


                if (originalObject == null)
                {
                    status = ODataAPIResponseStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = ODataAPIResponseStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override IODataAPIHandler GetNestedHandler(Friend parent, string navigationPropertyName)
        {
            return new OrderEFPatchHandler(parent);
        }

    }


    public class NewFriendEFPatchHandler : ODataAPIHandler<NewFriend>
    {
        Employee employee;
        public NewFriendEFPatchHandler(Employee employee)
        {
            this.employee = employee;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out NewFriend createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new NewFriend();
                employee.NewFriends.Add(createdObject);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var friend = employee.NewFriends.First(x => x.Id == Int32.Parse(id));

                employee.NewFriends.Remove(friend);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out NewFriend originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                originalObject = employee.NewFriends.First(x => x.Id == Int32.Parse(id));


                if (originalObject == null)
                {
                    status = ODataAPIResponseStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = ODataAPIResponseStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override IODataAPIHandler GetNestedHandler(NewFriend parent, string navigationPropertyName)
        {
            return null;
        }

    }



        public class OrderEFPatchHandler : ODataAPIHandler<Order>
        {
            Friend friend;
            public OrderEFPatchHandler(Friend friend)
            {
                this.friend = friend;
            }

            public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out Order createdObject, out string errorMessage)
            {
                createdObject = null;
                errorMessage = string.Empty;

                try
                {
                    createdObject = new Order();
                    friend.Orders.Add(createdObject);

                    return ODataAPIResponseStatus.Success;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;

                    return ODataAPIResponseStatus.Failure;
                }
            }

            public override ODataAPIResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
            {
                errorMessage = string.Empty;

                try
                {
                    var id = keyValues.First().Value.ToString();
                    var order = friend.Orders.First(x => x.Id == Int32.Parse(id));

                    friend.Orders.Remove(order);

                    return ODataAPIResponseStatus.Success;
                }
                catch (Exception ex)
                {
                    errorMessage = ex.Message;

                    return ODataAPIResponseStatus.Failure;
                }
            }

            public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out Order originalObject, out string errorMessage)
            {
                ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
                errorMessage = string.Empty;
                originalObject = null;

                try
                {
                    var id = keyValues["Id"].ToString();
                    originalObject = friend.Orders.First(x => x.Id == Int32.Parse(id));


                    if (originalObject == null)
                    {
                        status = ODataAPIResponseStatus.NotFound;
                    }

                }
                catch (Exception ex)
                {
                    status = ODataAPIResponseStatus.Failure;
                    errorMessage = ex.Message;
                }

                return status;
            }

            public override IODataAPIHandler GetNestedHandler(Order parent, string navigationPropertyName)
            {
                throw new NotImplementedException();
            }
        }

    }
}
