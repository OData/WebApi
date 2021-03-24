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
            modelBuilder.Entity<Friend>().HasKey(c => c.Id);
        }

       
    }

    public class EmployeePatchHandler : PatchMethodHandler<Employee>
    {
        public override ResponseStatus TryCreate(out Employee createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Employee();
                EmployeesController.dbContext.Employees.Add(createdObject);

                return ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ResponseStatus.Failure;
            }
        }

        public override ResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var customer = EmployeesController.dbContext.Employees.First(x => x.ID == Int32.Parse(id));

                EmployeesController.dbContext.Employees.Remove(customer);

                return ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ResponseStatus.Failure;
            }
        }

        public override ResponseStatus TryGet(IDictionary<string, object> keyValues, out Employee originalObject, out string errorMessage)
        {
            ResponseStatus status = ResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["ID"].ToString();
                originalObject = EmployeesController.dbContext.Employees.First(x => x.ID == Int32.Parse(id));


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

        public override IPatchMethodHandler GetNestedPatchHandler(Employee parent, string navigationPropertyName)
        {
            return new FriendPatchHandler(parent);
        }

    }

    public class FriendPatchHandler : PatchMethodHandler<Friend>
    {
        Employee employee;
        public FriendPatchHandler(Employee employee)
        {
            this.employee = employee;
        }

        public override ResponseStatus TryCreate(out Friend createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Friend();
                employee.Friends.Add(createdObject);

                return ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ResponseStatus.Failure;
            }
        }

        public override ResponseStatus TryDelete(IDictionary<string, object> keyValues, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var id = keyValues.First().Value.ToString();
                var friend = employee.Friends.First(x => x.Id == Int32.Parse(id));

                employee.Friends.Remove(friend);

                return ResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ResponseStatus.Failure;
            }
        }

        public override ResponseStatus TryGet(IDictionary<string, object> keyValues, out Friend originalObject, out string errorMessage)
        {
            ResponseStatus status = ResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                originalObject = employee.Friends.First(x => x.Id == Int32.Parse(id));


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

        public override IPatchMethodHandler GetNestedPatchHandler(Friend parent, string navigationPropertyName)
        {
            throw new NotImplementedException();
        }

    }
}
