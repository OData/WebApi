using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.BulkInsert1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.Test.E2E.AspNet.OData.BulkOperation
{

    public class EmployeePatchHandler : PatchMethodHandler<Employee>
    {
        public override PatchStatus TryCreate(Delta<Employee> patchObject, out Employee createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Employee();
                EmployeesController.Employees.Add(createdObject);

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
                var customer = EmployeesController.Employees.First(x => x.ID == Int32.Parse(id));

                EmployeesController.Employees.Remove(customer);

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
                originalObject = EmployeesController.Employees.First(x => x.ID == Int32.Parse(id));


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

        public override PatchStatus TryCreate(Delta<Friend> patchObject, out Friend createdObject, out string errorMessage)
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
                var friend = employee.Friends.FirstOrDefault(x => x.Id == Int32.Parse(id));

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
                originalObject = employee.Friends.FirstOrDefault(x => x.Id == Int32.Parse(id));


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
            switch (navigationPropertyName)
            {
                case "Orders":
                    return new OrderPatchHandler(parent);
                default:
                    return null;

            }
        }

    }


    public class OrderPatchHandler : PatchMethodHandler<Order>
    {
        Friend friend;
        public OrderPatchHandler(Friend friend)
        {
            this.friend = friend;
        }

        public override PatchStatus TryCreate(Delta<Order> patchObject, out Order createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Order();
                
                if(friend.Orders == null)
                {
                    friend.Orders = new List<Order>();
                }

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
                var friend = this.friend.Orders.FirstOrDefault(x => x.Id == int.Parse(id));

                this.friend.Orders.Remove(friend);

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
                if (friend.Orders != null)
                {
                    var id = keyValues["Id"].ToString();
                    originalObject = friend.Orders.FirstOrDefault(x => x.Id == Int32.Parse(id));
                }

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



    public class NewFriendPatchHandler : PatchMethodHandler<NewFriend>
    {
        Employee employee;
        public NewFriendPatchHandler(Employee employee)
        {
            this.employee = employee;
        }

        public override PatchStatus TryCreate(Delta<NewFriend> patchObject, out NewFriend createdObject, out string errorMessage)
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
            throw new NotImplementedException();
        }

    }


    public class EmployeeTypelessPatchHandler : TypelessPatchMethodHandler
    {
        IEdmEntityType entityType;
        public EmployeeTypelessPatchHandler(IEdmEntityType entityType)
        {
            this.entityType = entityType;
        }

        public override PatchStatus TryCreate(IEdmChangedObject changedObject, out EdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new EdmEntityObject(entityType);
                EmployeesController.EmployeesTypeless.Add(createdObject);

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
                foreach (var emp in EmployeesController.EmployeesTypeless)
                {
                    object id1;
                    emp.TryGetPropertyValue("ID", out id1);

                    if (id == id1.ToString())
                    {
                        EmployeesController.EmployeesTypeless.Remove(emp);
                        break;
                    }
                }
                              

                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out EdmStructuredObject originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["ID"].ToString();
               foreach (var emp in EmployeesController.EmployeesTypeless)
               {
                    object id1;
                    emp.TryGetPropertyValue("ID", out id1);

                    if(id == id1.ToString())
                    {
                        originalObject = emp;
                        break;
                    }
               }


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

        public override TypelessPatchMethodHandler GetNestedPatchHandler(EdmStructuredObject parent, string navigationPropertyName)
        {
            switch (navigationPropertyName)
            {
                case "UnTypedFriends":
                    return new FriendTypelessPatchHandler(parent, entityType.DeclaredNavigationProperties().First().Type.Definition.AsElementType() as IEdmEntityType);
                    
                default:
                    return null;
            }
              
        }

    }

    public class FriendTypelessPatchHandler : TypelessPatchMethodHandler
    {
        IEdmEntityType entityType;
        EdmStructuredObject employee;

        public FriendTypelessPatchHandler(EdmStructuredObject employee,  IEdmEntityType entityType)
        {
            this.employee = employee;
            this.entityType = entityType;
        }

        public override PatchStatus TryCreate(IEdmChangedObject changedObject, out EdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                object empid;
                if(employee.TryGetPropertyValue("ID" , out empid) && empid as int? == 3)
                {
                    throw new Exception("Testing Error");
                }

                createdObject = new EdmEntityObject(entityType);
                object obj;
                employee.TryGetPropertyValue("UnTypedFriends", out obj);

                var friends = obj as ICollection<EdmStructuredObject>;

                if(friends == null)
                {
                    friends = new List<EdmStructuredObject>();
                }

                friends.Add(createdObject);

                employee.TrySetPropertyValue("UnTypedFriends", friends);

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
                if(id == "5")
                {
                    throw new Exception("Testing Error");
                }
                foreach (var emp in EmployeesController.EmployeesTypeless)
                {
                    object id1;
                    emp.TryGetPropertyValue("ID", out id1);

                    if (id == id1.ToString())
                    {
                        object obj;
                        employee.TryGetPropertyValue("UnTypedFriends", out obj);

                        var friends = obj as IList<EdmStructuredObject>;

                        friends.Remove(emp);

                        employee.TrySetPropertyValue("UnTypedFriends", friends);
                                                
                        break;
                    }
                }


                return PatchStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return PatchStatus.Failure;
            }
        }

        public override PatchStatus TryGet(IDictionary<string, object> keyValues, out EdmStructuredObject originalObject, out string errorMessage)
        {
            PatchStatus status = PatchStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                object obj;
                employee.TryGetPropertyValue("UnTypedFriends", out obj);

                var friends = obj as IList<EdmStructuredObject>;

                if(friends == null)
                {
                    return PatchStatus.NotFound;
                }

                foreach (var friend in friends)
                {
                    object id1;
                    friend.TryGetPropertyValue("Id", out id1);

                    if (id == id1.ToString())
                    {
                        originalObject = friend;
                        break;
                    }
                }


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

        public override TypelessPatchMethodHandler GetNestedPatchHandler(EdmStructuredObject parent, string navigationPropertyName)
        {
            return null;            
        }

    }
}
