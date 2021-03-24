using Microsoft.AspNet.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Test.E2E.AspNet.OData.BulkInsert1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            
            modelBuilder.Entity<NewFriend>().Ignore(c => c.InstanceAnnotations);
            modelBuilder.Entity<UnTypedFriend>().Ignore(c => c.InstanceAnnotations);

            modelBuilder.Entity<Friend>().HasKey(c => c.Id);
            
        }

       
    }

    public class EmployeePatchHandler : PatchMethodHandler<Employee>
    {
        public override PatchStatus TryCreate(out Employee createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Employee();
                EmployeesController.dbContext.Employees.Add(createdObject);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var customer = EmployeesController.dbContext.Employees.First(x => x.ID == Int32.Parse(id));

                EmployeesController.dbContext.Employees.Remove(customer);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out Employee originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["ID"].ToString();
                originalObject = EmployeesController.dbContext.Employees.First(x => x.ID == Int32.Parse(id));


                if (originalObject == null)
                {
                    status = PatchStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = PatchStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override IPatchMethodHandler GetNestedPatchHandler(Employee parent, string navigationPropertyName)
        {
            switch (navigationPropertyName)
            {
                case "Friends":
                    return new FriendPatchHandler(parent);
                case "NewFriends":
                    return new NewFriendPatchHandler(parent);
                default:
                    return null;
            }
            
        }

    }

    public class FriendPatchHandler : PatchMethodHandler<Friend>
    {
        Employee employee;
        public FriendPatchHandler(Employee employee)
        {
            this.employee = employee;
        }

        public override PatchStatus TryCreate(out Friend createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Friend();
                employee.Friends.Add(createdObject);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var friend = employee.Friends.First(x => x.Id == Int32.Parse(id));

                employee.Friends.Remove(friend);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out Friend originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                originalObject = employee.Friends.First(x => x.Id == Int32.Parse(id));


                if (originalObject == null)
                {
                    status = PatchStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = PatchStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override IPatchMethodHandler GetNestedPatchHandler(Friend parent, string navigationPropertyName)
        {
            return new OrderPatchHandler(parent);
        }

    }


    public class NewFriendPatchHandler : PatchMethodHandler<NewFriend>
    {
        Employee employee;
        public NewFriendPatchHandler(Employee employee)
        {
            this.employee = employee;
        }

        public override PatchStatus TryCreate(out NewFriend createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new NewFriend();
                employee.NewFriends.Add(createdObject);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var friend = employee.NewFriends.First(x => x.Id == Int32.Parse(id));

                employee.NewFriends.Remove(friend);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out NewFriend originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                originalObject = employee.NewFriends.First(x => x.Id == Int32.Parse(id));


                if (originalObject == null)
                {
                    status = PatchStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = PatchStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override IPatchMethodHandler GetNestedPatchHandler(NewFriend parent, string navigationPropertyName)
        {
            return null;
        }

    }



    public class OrderPatchHandler : PatchMethodHandler<Order>
    {
        Friend friend;
        public OrderPatchHandler(Friend friend)
        {
            this.friend = friend;
        }

        public override PatchStatus TryCreate(out Order createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Order();
                friend.Orders.Add(createdObject);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var order = friend.Orders.First(x => x.Id == Int32.Parse(id));

                friend.Orders.Remove(order);

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out Order originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                originalObject = friend.Orders.First(x => x.Id == Int32.Parse(id));


                if (originalObject == null)
                {
                    status = PatchStatus.NotFound;
                }

            }
            catch (Exception ex)
            {
                status = PatchStatus.Failure;
                errorMessage = ex.Message;
            }

            return status;
        }

        public override IPatchMethodHandler GetNestedPatchHandler(Order parent, string navigationPropertyName)
        {
            throw new NotImplementedException();
        }

    }
}
