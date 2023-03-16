//-----------------------------------------------------------------------------
// <copyright file="BulkOperationPatchHandlers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.BulkOperation;

namespace Microsoft.Test.E2E.AspNet.OData.BulkOperation
{
    internal class APIHandlerFactory : ODataAPIHandlerFactory
    {
        public APIHandlerFactory(IEdmModel model) : base(model)
        {
        }

        public override IODataAPIHandler GetHandler(ODataPath odataPath)
        {
            if (odataPath != null)
            {
                int currentPosition = 0;

                if (odataPath.Count == 1)
                {
                    GetHandlerInternal(odataPath.FirstSegment.Identifier, currentPosition);
                }

                List<ODataPathSegment> pathSegments = odataPath.GetSegments();

                ODataPathSegment currentPathSegment = pathSegments[currentPosition];

                if (currentPathSegment is EntitySetSegment || currentPathSegment is NavigationPropertySegment || currentPathSegment is SingletonSegment)
                {
                    int keySegmentPosition = ODataPathHelper.GetNextKeySegmentPosition(pathSegments, currentPosition);
                    KeySegment keySegment = (KeySegment)pathSegments[keySegmentPosition];

                    currentPosition = keySegmentPosition;

                    return GetHandlerInternal(
                        currentPathSegment.Identifier,
                        currentPosition,
                        ODataPathHelper.KeySegmentAsDictionary(keySegment),
                        pathSegments);
                }
            }

            return null;
        }

        private IODataAPIHandler GetHandlerInternal(
            string pathName,
            int currentPosition,
            Dictionary<string, object> keys = null,
            List<ODataPathSegment> pathSegments = null)
        {
            switch (pathName)
            {
                case "Employees":
                    Employee employee;
                    string msg;
                    if ((new EmployeeAPIHandler().TryGet(keys, out employee, out msg)) == ODataAPIResponseStatus.Success)
                    {
                        return GetNestedHandlerForEmployee(pathSegments, currentPosition, employee);
                    }
                    return null;
                case "Companies":
                    return new CompanyAPIHandler();

                default:
                    return null;
            }
        }

        private static IODataAPIHandler GetNestedHandlerForEmployee(List<ODataPathSegment> pathSegments, int currentPosition, Employee employee)
        {
            ++currentPosition;

            if (pathSegments.Count <= currentPosition)
            {
                return null;
            }

            ODataPathSegment currentPathSegment = pathSegments[currentPosition];

            if (currentPathSegment is NavigationPropertySegment)
            {
                int keySegmentPosition = ODataPathHelper.GetNextKeySegmentPosition(pathSegments, currentPosition);
                KeySegment keySegment = (KeySegment)pathSegments[keySegmentPosition];
                Dictionary<string,object> keys = ODataPathHelper.KeySegmentAsDictionary(keySegment);

                currentPosition = keySegmentPosition;

                switch (currentPathSegment.Identifier)
                {
                    case "NewFriends":
                        ODataPathSegment nextPathSegment = pathSegments[++currentPosition];

                        if (nextPathSegment is TypeSegment)
                        {
                            currentPosition++;
                            TypeSegment typeSegment = nextPathSegment as TypeSegment;

                            if (typeSegment.Identifier == "Microsoft.Test.E2E.AspNet.OData.BulkOperation.MyNewFriend")
                            {
                                MyNewFriend friend = employee.NewFriends.FirstOrDefault(x => x.Id == (int)keys["Id"]) as MyNewFriend;

                                if (friend != null)
                                {
                                    switch (pathSegments[++currentPosition].Identifier)
                                    {
                                        case "MyNewOrders":
                                            return new MyNewOrderAPIHandler(friend);

                                        default:
                                            return null;

                                    }
                                }
                            }
                        }
                        else
                        {
                            NewFriend friend = employee.NewFriends.FirstOrDefault(x => x.Id == (int)keys["Id"]);

                            if (friend != null)
                            {
                                switch (pathSegments[++currentPosition].Identifier)
                                {
                                    case "NewOrders":
                                        return new NewOrderAPIHandler(friend);

                                    default:
                                        return null;
                                }
                            }
                        }
                        return null;

                    case "Friends":
                        return new FriendAPIHandler(employee);

                    default:
                        return null;
                }
            }
            return null;
        }
    }

    internal class TypelessAPIHandlerFactory : EdmODataAPIHandlerFactory
    {
        IEdmEntityType entityType;

        public TypelessAPIHandlerFactory(IEdmModel model, IEdmEntityType entityType) : base(model)
        {
            this.entityType = entityType;
        }

        public override EdmODataAPIHandler GetHandler(ODataPath odataPath)
        {
            if (odataPath != null)
            {
                string pathName = odataPath.GetLastNonTypeNonKeySegment().Identifier;

                switch (pathName)
                {
                    case "UnTypedEmployees":
                        return new EmployeeEdmAPIHandler(entityType);

                    default:
                        return null;
                }
            }

            return null;
        }
    }

    internal class CompanyAPIHandler : ODataAPIHandler<Company>
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
                case "MyOverdueOrders":
                    return new MyOverdueOrderAPIHandler(parent);
                default:
                    return null;
            }
        }

        public override ODataAPIResponseStatus TryAddRelatedObject(Company resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }

    internal class OverdueOrderAPIHandler : ODataAPIHandler<NewOrder>
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
                originalObject = parent.OverdueOrders.FirstOrDefault(x => x.Id == Int32.Parse(id));

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

        public override ODataAPIResponseStatus TryAddRelatedObject(NewOrder resource, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                parent.OverdueOrders.Add(resource);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }
    }

    internal class MyOverdueOrderAPIHandler : ODataAPIHandler<MyNewOrder>
    {
        Company parent;

        public MyOverdueOrderAPIHandler(Company parent)
        {
            this.parent = parent;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out MyNewOrder createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new MyNewOrder();
                parent.MyOverdueOrders.Add(createdObject);

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
                var newOrders = CompanyController.MyOverdueOrders.First(x => x.Id == Int32.Parse(id));

                parent.MyOverdueOrders.Remove(newOrders);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out MyNewOrder originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                var id = keyValues["Id"].ToString();
                originalObject = parent.MyOverdueOrders.FirstOrDefault(x => x.Id == Int32.Parse(id));


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

        public override IODataAPIHandler GetNestedHandler(MyNewOrder parent, string navigationPropertyName)
        {
            switch (navigationPropertyName)
            {

                default:
                    return null;
            }
        }

        public override ODataAPIResponseStatus TryAddRelatedObject(MyNewOrder resource, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                parent.MyOverdueOrders.Add(resource);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }
    }

    internal class EmployeeAPIHandler : ODataAPIHandler<Employee>
    {
        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out Employee createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = null;

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
            errorMessage = null;

            try
            {
                var id = keyValues.First().Value.ToString();
                var employee = EmployeesController.Employees.First(x => x.ID == Int32.Parse(id));

                EmployeesController.Employees.Remove(employee);

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
            errorMessage = null;
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

        public override ODataAPIResponseStatus TryAddRelatedObject(Employee resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }

    internal class FriendAPIHandler : ODataAPIHandler<Friend>
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

        public override ODataAPIResponseStatus TryAddRelatedObject(Friend resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }

    internal class NewOrderAPIHandler : ODataAPIHandler<NewOrder>
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

        public override ODataAPIResponseStatus TryAddRelatedObject(NewOrder resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }

    internal class MyNewOrderAPIHandler : ODataAPIHandler<MyNewOrder>
    {
        MyNewFriend friend;
        public MyNewOrderAPIHandler(MyNewFriend friend)
        {
            this.friend = friend;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out MyNewOrder createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                createdObject = new MyNewOrder();

                if (friend.MyNewOrders == null)
                {
                    friend.MyNewOrders = new List<MyNewOrder>();
                }

                friend.MyNewOrders.Add(createdObject);

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
                var friend = this.friend.MyNewOrders.FirstOrDefault(x => x.Id == int.Parse(id));

                this.friend.MyNewOrders.Remove(friend);

                return ODataAPIResponseStatus.Success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;

                return ODataAPIResponseStatus.Failure;
            }
        }

        public override ODataAPIResponseStatus TryGet(IDictionary<string, object> keyValues, out MyNewOrder originalObject, out string errorMessage)
        {
            ODataAPIResponseStatus status = ODataAPIResponseStatus.Success;
            errorMessage = string.Empty;
            originalObject = null;

            try
            {
                if (friend.MyNewOrders != null)
                {
                    var id = keyValues["Id"].ToString();
                    originalObject = friend.MyNewOrders.FirstOrDefault(x => x.Id == Int32.Parse(id));
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

        public override IODataAPIHandler GetNestedHandler(MyNewOrder parent, string navigationPropertyName)
        {
            throw new NotImplementedException();
        }

        public override ODataAPIResponseStatus TryAddRelatedObject(MyNewOrder resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }

    internal class OrderAPIHandler : ODataAPIHandler<Order>
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

                if (friend.Orders == null)
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

        public override ODataAPIResponseStatus TryAddRelatedObject(Order resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }

    internal class NewFriendAPIHandler : ODataAPIHandler<NewFriend>
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

                if (employee.NewFriends == null)
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

                if (employee.NewFriends == null)
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

        public override ODataAPIResponseStatus TryAddRelatedObject(NewFriend resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }

    internal class EmployeeEdmAPIHandler : EdmODataAPIHandler
    {
        IEdmEntityType entityType;
        public EmployeeEdmAPIHandler(IEdmEntityType entityType)
        {
            this.entityType = entityType;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out IEdmStructuredObject createdObject, out string errorMessage)
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

                    if (id == id1.ToString())
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

        public override ODataAPIResponseStatus TryAddRelatedObject(IEdmStructuredObject resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }

    internal class FriendTypelessAPIHandler : EdmODataAPIHandler
    {
        IEdmEntityType entityType;
        EdmStructuredObject employee;

        public FriendTypelessAPIHandler(IEdmStructuredObject employee, IEdmEntityType entityType)
        {
            this.employee = employee as EdmStructuredObject;
            this.entityType = entityType;
        }

        public override ODataAPIResponseStatus TryCreate(IDictionary<string, object> keyValues, out IEdmStructuredObject createdObject, out string errorMessage)
        {
            createdObject = null;
            errorMessage = string.Empty;

            try
            {
                object empid;
                if (employee.TryGetPropertyValue("ID", out empid) && empid as int? == 3)
                {
                    throw new Exception("Testing Error");
                }

                createdObject = new EdmEntityObject(entityType);
                object obj;
                employee.TryGetPropertyValue("UnTypedFriends", out obj);

                var friends = obj as ICollection<IEdmStructuredObject>;

                if (friends == null)
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
                if (id == "5")
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

                if (friends == null)
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

        public override ODataAPIResponseStatus TryAddRelatedObject(IEdmStructuredObject resource, out string errorMessage)
        {
            //throw new NotImplementedException();
            errorMessage = string.Empty;
            return ODataAPIResponseStatus.Success;
        }
    }
}
