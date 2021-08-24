// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.BulkInsert;

namespace Microsoft.Test.E2E.AspNet.OData.BulkOperation
{
    public class APIHandlerFactory : ODataAPIHandlerFactory
    {
        public override IODataAPIHandler GetHandler(NavigationPath navigationPath)
        {
            if(navigationPath != null)
            {
                var pathItems = navigationPath.GetNavigationPathItems();
                int cnt = 0;
                                    
                    switch (pathItems[cnt].Name)
                    {
                        case "Employees":
                            { 
                                Employee employee;
                                string msg;
                                if ((new EmployeeAPIHandler().TryGet(pathItems[cnt].KeyProperties, out employee, out msg)) == ODataAPIResponseStatus.Success)
                            {
                                return GetNestedHandlerForEmployee(pathItems, cnt, employee);
                            }
                        }
                        return null;

                        default:
                            return null;

                    }
                
            }

            return null;
        }

        private static IODataAPIHandler GetNestedHandlerForEmployee(PathItem[] pathItems, int cnt, Employee employee)
        {
            switch (pathItems[++cnt].Name)
            {
                case "NewFriends":
                    NewFriend friend = employee.NewFriends.FirstOrDefault(x => x.Id == (int)pathItems[cnt].KeyProperties["Id"]);

                    if (friend != null)
                    {
                        switch (pathItems[++cnt].Name)
                        {
                            case "NewOrders":
                                return new NewOrderAPIHandler(friend);

                            default:
                                return null;

                        }
                    }
                    return null;

                default:
                    return null;

            }
        }
    }

    public class CompanyAPIHandler : ODataAPIHandler<Company>
    {
        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out Company createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Company();
                CompanyController.Companies.Add(createdObject);

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
                var company = CompanyController.Companies.First(x => x.Id == Int32.Parse(id));

                CompanyController.Companies.Remove(company);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out Company originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                originalObject = CompanyController.Companies.First(x => x.Id == Int32.Parse(id));


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

        public override IODataAPIHandler GetNestedHandler(Company parent, string navigationPropertyName)
        {
            switch (navigationPropertyName)
            {
                case "OverdueOrders":
                    return new OverdueOrderAPIHandler(parent);
                default:
                    return null;
            }

        }
    }

    public class OverdueOrderAPIHandler : ODataAPIHandler<NewOrder>
    {
        Company parent;

        public OverdueOrderAPIHandler(Company parent)
        {
            this.parent = parent;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out NewOrder createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new NewOrder();
                parent.OverdueOrders.Add(createdObject);

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
                var newOrders = CompanyController.OverdueOrders.First(x => x.Id == Int32.Parse(id));

                parent.OverdueOrders.Remove(newOrders);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out NewOrder originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                originalObject = parent.OverdueOrders.First(x => x.Id == Int32.Parse(id));


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

        public override IODataAPIHandler GetNestedHandler(NewOrder parent, string navigationPropertyName)
        {
            switch (navigationPropertyName)
            {

                default:
                    return null;
            }

        }
    }


    public class EmployeeAPIHandler : ODataAPIHandler<Employee>
    {
        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out Employee createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new Employee();
                EmployeesController.Employees.Add(createdObject);

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
                var customer = EmployeesController.Employees.First(x => x.ID == Int32.Parse(id));

                EmployeesController.Employees.Remove(customer);

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
                originalObject = EmployeesController.Employees.First(x => x.ID == Int32.Parse(id));


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
                    return new FriendAPIHandler(parent);
                case "NewFriends":
                    return new NewFriendAPIHandler(parent);
                default:
                    return null;
            }
            
        }
    }

    public class FriendAPIHandler : ODataAPIHandler<Friend>
    {
        Employee employee;
        public FriendAPIHandler(Employee employee)
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
                var friend = employee.Friends.FirstOrDefault(x => x.Id == Int32.Parse(id));

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
                originalObject = employee.Friends.FirstOrDefault(x => x.Id == Int32.Parse(id));


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
            switch (navigationPropertyName)
            {
                case "Orders":
                    return new OrderAPIHandler(parent);
                default:
                    return null;

            }
        }

    }

    public class NewOrderAPIHandler : ODataAPIHandler<NewOrder>
    {
        NewFriend friend;
        public NewOrderAPIHandler(NewFriend friend)
        {
            this.friend = friend;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out NewOrder createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new NewOrder();

                if (friend.NewOrders == null)
                {
                    friend.NewOrders = new List<NewOrder>();
                }

                friend.NewOrders.Add(createdObject);

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
                var friend = this.friend.NewOrders.FirstOrDefault(x => x.Id == int.Parse(id));

                this.friend.NewOrders.Remove(friend);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out NewOrder originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                if (friend.NewOrders != null)
                {
                    var id = keyValues["Id"].ToString();
                    originalObject = friend.NewOrders.FirstOrDefault(x => x.Id == Int32.Parse(id));
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

        public override IODataAPIHandler GetNestedHandler(NewOrder parent, string navigationPropertyName)
        {
            throw new NotImplementedException();
        }

    }



    public class OrderAPIHandler : ODataAPIHandler<Order>
    {
        Friend friend;
        public OrderAPIHandler(Friend friend)
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
                
                if(friend.Orders == null)
                {
                    friend.Orders = new List<Order>();
                }

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
                var friend = this.friend.Orders.FirstOrDefault(x => x.Id == int.Parse(id));

                this.friend.Orders.Remove(friend);

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
                if (friend.Orders != null)
                {
                    var id = keyValues["Id"].ToString();
                    originalObject = friend.Orders.FirstOrDefault(x => x.Id == Int32.Parse(id));
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

        public override IODataAPIHandler GetNestedHandler(Order parent, string navigationPropertyName)
        {
            throw new NotImplementedException();
        }

    }



    public class NewFriendAPIHandler : ODataAPIHandler<NewFriend>
    {
        Employee employee;
        public NewFriendAPIHandler(Employee employee)
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

                if(employee.NewFriends == null)
                {
                    employee.NewFriends = new List<NewFriend>();
                }

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

                if(employee.NewFriends == null)
                {
                    return ODataAPIResponseStatus.NotFound;
                }

                originalObject = employee.NewFriends.FirstOrDefault(x => x.Id == Int32.Parse(id));


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
            throw new NotImplementedException();
        }

    }


    public class EmployeeEdmAPIHandler : EdmODataAPIHandler
    {
        IEdmEntityType entityType;
        public EmployeeEdmAPIHandler(IEdmEntityType entityType)
        {
            this.entityType = entityType;
        }

        public override ODataAPIResponseStatus TryCreate(IEdmChangedObject changedObject, out IEdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new EdmEntityObject(entityType);
                EmployeesController.EmployeesTypeless.Add(createdObject as EdmStructuredObject);

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
                              

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
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

        public override EdmODataAPIHandler GetNestedHandler(IEdmStructuredObject parent, string navigationPropertyName)
        {
            switch (navigationPropertyName)
            {
                case "UnTypedFriends":
                    return new FriendTypelessAPIHandler(parent, entityType.DeclaredNavigationProperties().First().Type.Definition.AsElementType() as IEdmEntityType);
                    
                default:
                    return null;
            }
              
        }

    }

    public class FriendTypelessAPIHandler : EdmODataAPIHandler
    {
        IEdmEntityType entityType;
        EdmStructuredObject employee;

        public FriendTypelessAPIHandler(IEdmStructuredObject employee,  IEdmEntityType entityType)
        {
            this.employee = employee as EdmStructuredObject;
            this.entityType = entityType;
        }

        public override ODataAPIResponseStatus TryCreate(IEdmChangedObject changedObject, out IEdmStructuredObject createdObject, out string errorMessage)
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

                var friends = obj as ICollection<IEdmStructuredObject>;

                if(friends == null)
                {
                    friends = new List<IEdmStructuredObject>();
                }

                friends.Add(createdObject);

                employee.TrySetPropertyValue("UnTypedFriends", friends);

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

                        var friends = obj as IList<IEdmStructuredObject>;

                        friends.Remove(emp);

                        employee.TrySetPropertyValue("UnTypedFriends", friends);
                                                
                        break;
                    }
                }


                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out IEdmStructuredObject originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
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
                    return ODataAPIResponseStatus.NotFound;
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

        public override EdmODataAPIHandler GetNestedHandler(IEdmStructuredObject parent, string navigationPropertyName)
        {
            return null;            
        }

    }
}
