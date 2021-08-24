//-----------------------------------------------------------------------------
// <copyright file="BulkOperationController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BulkOperation
{
    public class EmployeesController : TestODataController
    {
        public EmployeesController()
        {
            if (null == Employees)
            {
                InitEmployees();
            }
        }

        /// <summary>
        /// static so that the data is shared among requests.
        /// </summary>
        public static IList<Employee> Employees = null;

        public static IList<IEdmStructuredObject> EmployeesTypeless = null;

        private List<Friend> Friends = null;

        private void InitEmployees()
        {
            Friends = new List<Friend> { new Friend { Id = 1, Name = "Test0", Age = 33 }, new Friend { Id = 2, Name = "Test1", Orders = new List<Order>() { new Order { Id = 1, Price = 2 } } }, new Friend { Id = 3, Name = "Test3" }, new Friend { Id = 4, Name = "Test4" } };

            Employees = new List<Employee>
            {
                new Employee()
                {
                    ID=1,
                    Name="Name1",
                    SkillSet=new List<Skill>{Skill.CSharp,Skill.Sql},
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Execute,
                    FavoriteSports = new FavoriteSports{Sport ="Football"},
                    NewFriends = new List<NewFriend>(){new NewFriend {Id =1, Name ="NewFriendTest1", Age=33, NewOrders= new List<NewOrder>() { new NewOrder {Id=1, Price =101 } } } },
                    Friends = this.Friends.Where(x=>x.Id ==1 || x.Id==2).ToList()
                },
                new Employee()
                {
                    ID=2,Name="Name2",
                    SkillSet=new List<Skill>(),
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Read,
                    NewFriends = new List<NewFriend>(){ new MyNewFriend { Id = 2, MyNewOrders = new List<MyNewOrder>() { new MyNewOrder { Id = 2, Price = 444 , Quantity=2 } } } },
                    Friends =  this.Friends.Where(x=>x.Id ==3 || x.Id==4).ToList()
                },
                new Employee(){
                    ID=3,Name="Name3",
                    SkillSet=new List<Skill>{Skill.Web,Skill.Sql},
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Read|AccessLevel.Write

                },
            };
        }

        private void InitTypeLessEmployees(IEdmEntityType entityType)
        {
            EmployeesTypeless = new List<IEdmStructuredObject>();
            var emp1 = new EdmEntityObject(entityType);
            emp1.TrySetPropertyValue("ID", 1);
            emp1.TrySetPropertyValue("Name", "Test1");

            var friendType = entityType.DeclaredNavigationProperties().First().Type.Definition.AsElementType() as IEdmEntityType;

            var friends = new List<EdmStructuredObject>();
            var friend1 = new EdmEntityObject(friendType);
            friend1.TrySetPropertyValue("Id", 1);
            friend1.TrySetPropertyValue("Age", 33);
            friend1.TrySetPropertyValue("Name", "Test1");

            var friend2 = new EdmEntityObject(friendType);
            friend2.TrySetPropertyValue("Id", 2);
            friend2.TrySetPropertyValue("Name", "Test2");

            friends.Add(friend1);
            friends.Add(friend2);

            emp1.TrySetPropertyValue("UnTypedFriends", friends);

            var emp2 = new EdmEntityObject(entityType);
            emp2.TrySetPropertyValue("ID", 2);
            emp2.TrySetPropertyValue("Name", "Test2");

            var friends2 = new List<EdmStructuredObject>();
            var friend3 = new EdmEntityObject(friendType);
            friend3.TrySetPropertyValue("Id", 3);
            friend3.TrySetPropertyValue("Name", "Test3");

            var friend4 = new EdmEntityObject(friendType);
            friend4.TrySetPropertyValue("Id", 4);
            friend4.TrySetPropertyValue("Name", "Test4");

            friends2.Add(friend3);
            friends2.Add(friend4);

            emp2.TrySetPropertyValue("UnTypedFriends", friends2);

            var emp3 = new EdmEntityObject(entityType);
            emp3.TrySetPropertyValue("ID", 3);
            emp3.TrySetPropertyValue("Name", "Test3");

            var friends35 = new List<EdmStructuredObject>();
            var friend5 = new EdmEntityObject(friendType);
            friend5.TrySetPropertyValue("Id", 5);
            friend5.TrySetPropertyValue("Name", "Test5");

            friends35.Add(friend5);

            emp3.TrySetPropertyValue("UnTypedFriends", friends35);

            EmployeesTypeless.Add(emp1);
            EmployeesTypeless.Add(emp2);
            EmployeesTypeless.Add(emp3);
        }

        public DeltaSet<NewFriend> PatchWithUsersMethod(DeltaSet<NewFriend> friendColl, Employee employee)
        {
            var changedObjColl = friendColl.Patch(new NewFriendAPIHandler(employee), new APIHandlerFactory(Request.GetModel()));

            return changedObjColl;
        }
        public EdmChangedObjectCollection PatchWithUsersMethodTypeLess(int key, EdmChangedObjectCollection friendColl)
        {
            var entity = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.BulkOperation.UnTypedEmployee") as IEdmEntityType;
            InitTypeLessEmployees(entity);

            var entity1 = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.BulkOperation.UnTypedFriend") as IEdmEntityType;

            var changedObjColl = friendColl.Patch(new FriendTypelessAPIHandler(EmployeesTypeless[key - 1], entity), new TypelessAPIHandlerFactory(Request.GetModel(), entity));

            return changedObjColl;
        }

        public EdmChangedObjectCollection EmployeePatchMethodTypeLess(EdmChangedObjectCollection empColl)
        {
            var entity = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.BulkOperation.UnTypedEmployee") as IEdmEntityType;
            InitTypeLessEmployees(entity);

            var changedObjColl = empColl.Patch(new EmployeeEdmAPIHandler(entity), new TypelessAPIHandlerFactory(Request.GetModel(), entity));
            ValidateSuccessfulTypeless();

            return changedObjColl;
        }

        private void ValidateSuccessfulTypeless()
        {
            object obj;
            Assert.True(EmployeesTypeless.First().TryGetPropertyValue("UnTypedFriends", out obj));

            var friends = obj as ICollection<EdmStructuredObject>;
            Assert.NotNull(friends);

            object obj1;

            friends.First().TryGetPropertyValue("Name", out obj1);

            object name;

            if (EmployeesTypeless.First().TryGetPropertyValue("Name", out name) && name.ToString() == "Employeeabcd")
            {
                Assert.Equal("abcd", obj1.ToString());

                object age;
                friends.First().TryGetPropertyValue("Age", out age);

                Assert.Equal(33, (int)age);
            }
            else
            {
                Assert.Equal("Friend1", obj1.ToString());
            }

        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get()
        {
            return Ok(Employees.AsQueryable());
        }

        [EnableQuery]
        public ITestActionResult Get(int key)
        {
            var emp = Employees.SingleOrDefault(e => e.ID == key);
            return Ok(emp);
        }

        [ODataRoute("Employees({key})/Friends")]
        public ITestActionResult GetFriends(int key)
        {
            var emp = Employees.SingleOrDefault(e => e.ID == key);
            return Ok(emp.Friends);
        }

        [ODataRoute("Employees({key})/UnTypedFriends")]
        public ITestActionResult GetUnTypedFriends(int key)
        {
            var entity = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.BulkOperation.UnTypedEmployee") as IEdmEntityType;
            InitTypeLessEmployees(entity);

            foreach (var emp in EmployeesTypeless)
            {
                object obj;
                emp.TryGetPropertyValue("ID", out obj);

                if (Equals(key, obj))
                {
                    object friends;
                    emp.TryGetPropertyValue("UntypedFriends", out friends);
                    return Ok(friends);
                }
            }
            return Ok();
        }

        [ODataRoute("Employees")]
        [HttpPatch]
        [EnableQuery]
        public ITestActionResult PatchEmployees([FromBody] DeltaSet<Employee> coll)
        {
            InitEmployees();

            Assert.NotNull(coll);

            var returncoll = coll.Patch(new EmployeeAPIHandler(), new APIHandlerFactory(Request.GetModel()));

            return Ok(returncoll);
        }

        [ODataRoute("Employees")]
        [HttpPost]
        public ITestActionResult Post([FromBody] Employee employee)
        {
            InitEmployees();

            var handler = new EmployeeAPIHandler();

            handler.UpdateLinkedObjects(employee, Request.GetModel());

            return Ok(employee);
        }

        [ODataRoute("Employees({key})/Friends")]
        [HttpPatch]
        public ITestActionResult PatchFriends(int key, [FromBody] DeltaSet<Friend> friendColl)
        {
            InitEmployees();

            Employee originalEmployee = Employees.SingleOrDefault(c => c.ID == key);
            Assert.NotNull(originalEmployee);

            var changedObjColl = friendColl.Patch(originalEmployee.Friends);

            return Ok(changedObjColl);
        }

        [ODataRoute("Employees({key})/NewFriends")]
        [HttpPatch]
        public ITestActionResult PatchNewFriends(int key, [FromBody] DeltaSet<NewFriend> friendColl)
        {
            InitEmployees();

            if (key == 1)
            {
                var deltaSet = PatchWithUsersMethod(friendColl, Employees.First(x => x.ID == key));

                return Ok(deltaSet);
            }
            {
                Employee originalEmployee = Employees.SingleOrDefault(c => c.ID == key);
                Assert.NotNull(originalEmployee);

                var friendCollection = new FriendColl<NewFriend>() { new NewFriend { Id = 2, Age = 15 } };

                var changedObjColl = friendColl.Patch(friendCollection);

                return Ok(changedObjColl);
            }
        }

        [ODataRoute("Employees({key})/UnTypedFriends")]
        [HttpPatch]
        public ITestActionResult PatchUnTypedFriends(int key, [FromBody] EdmChangedObjectCollection friendColl)
        {
            if (key == 1)
            {
                var changedObjColl = PatchWithUsersMethodTypeLess(key, friendColl);

                var emp = EmployeesTypeless[key - 1];
                object obj;
                emp.TryGetPropertyValue("UnTypedFriends", out obj);
                var lst = obj as List<EdmStructuredObject>;

                if (lst != null && lst.Count > 1)
                {
                    object obj1;
                    if (lst[1].TryGetPropertyValue("Name", out obj1) && Equals("Friend007", obj1))
                    {
                        lst[1].TryGetPropertyValue("Address", out obj1);
                        Assert.NotNull(obj1);
                        object obj2;
                        (obj1 as EdmStructuredObject).TryGetPropertyValue("Street", out obj2);

                        Assert.Equal("Abc 123", obj2);
                    }
                }

                return Ok(changedObjColl);
            }
            else if (key == 2)
            {
                var entitytype = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.BulkOperation.UnTypedEmployee") as IEdmEntityType;
                var entity = new EdmEntityObject(friendColl[0].GetEdmType().AsEntity());
                entity.TrySetPropertyValue("Id", 2);

                var friendCollection = new FriendColl<IEdmStructuredObject>() { entity };

                var changedObjColl = PatchWithUsersMethodTypeLess(key, friendColl);

                object obj;
                Assert.Single(changedObjColl);

                changedObjColl.First().TryGetPropertyValue("Age", out obj);
                Assert.Equal(35, obj);

                return Ok(changedObjColl);
            }
            else
            {
                var changedObjColl = PatchWithUsersMethodTypeLess(key, friendColl);

                return Ok(changedObjColl);
            }
        }

        [ODataRoute("UnTypedEmployees")]
        [HttpPatch]
        public ITestActionResult PatchUnTypedEmployees([FromBody] EdmChangedObjectCollection empColl)
        {
            var changedObjColl = EmployeePatchMethodTypeLess(empColl);

            return Ok(changedObjColl);
        }

        [ODataRoute("Employees({key})")]
        [EnableQuery]
        public ITestActionResult Patch(int key, [FromBody] Delta<Employee> delta)
        {
            InitEmployees();

            delta.TrySetPropertyValue("ID", key); // It is the key property, and should not be updated.

            Employee employee = Employees.FirstOrDefault(e => e.ID == key);

            if (employee == null)
            {
                employee = new Employee();
                delta.Patch(employee, new EmployeeAPIHandler(), new APIHandlerFactory(Request.GetModel()));
                return Created(employee);
            }

            try
            {
                // todo: put APIHandlerFactory in request context
                delta.Patch(employee, new EmployeeAPIHandler(), new APIHandlerFactory(Request.GetModel()));

                if (employee.Name == "Bind1")
                {
                    Assert.NotNull(employee.Friends.Single(x => x.Id == 3));
                }
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }

            return Ok(employee);
        }
    }

    public class CompanyController : TestODataController
    {
        public static IList<Company> Companies = null;
        public static IList<NewOrder> OverdueOrders = null;
        public static IList<MyNewOrder> MyOverdueOrders = null;

        public CompanyController()
        {
            if (null == Companies)
            {
                InitCompanies();
            }
        }

        private void InitCompanies()
        {
            OverdueOrders = new List<NewOrder>() { new NewOrder { Id = 1, Price = 10, Quantity = 1 }, new NewOrder { Id = 2, Price = 20, Quantity = 2 }, new NewOrder { Id = 3, Price = 30 }, new NewOrder { Id = 4, Price = 40 } };
            MyOverdueOrders = new List<MyNewOrder>() { new MyNewOrder { Id = 1, Price = 10, Quantity = 1 }, new MyNewOrder { Id = 2, Price = 20, Quantity = 2 }, new MyNewOrder { Id = 3, Price = 30 }, new MyNewOrder { Id = 4, Price = 40 } };

            Companies = new List<Company>() { new Company { Id = 1, Name = "Company1", OverdueOrders = OverdueOrders.Where(x => x.Id == 2).ToList(), MyOverdueOrders = MyOverdueOrders.Where(x => x.Id == 2).ToList() } ,
                        new Company { Id = 2, Name = "Company2", OverdueOrders = OverdueOrders.Where(x => x.Id == 3 || x.Id == 4).ToList() } };
        }

        [ODataRoute("Companies")]
        [HttpPatch]
        public ITestActionResult PatchCompanies([FromBody] DeltaSet<Company> coll)
        {
            InitCompanies();

            Assert.NotNull(coll);

            var returncoll = coll.Patch(new CompanyAPIHandler(), new APIHandlerFactory(Request.GetModel()));

            var comp = coll.First() as Delta<Company>;
            object val;
            if (comp.TryGetPropertyValue("Name", out val))
            {
                if (val.ToString() == "Company02")
                {
                    ValidateOverdueOrders2(1, 2, 9);
                }
                else
                {
                    ValidateOverdueOrders1(1, 1, 9);
                }
            }

            return Ok(returncoll);
        }

        [ODataRoute("Companies")]
        [HttpPost]
        public ITestActionResult Post([FromBody] Company company)
        {

            InitCompanies();
            InitEmployees();

            if (company.Id == 4)
            {
                AddNewOrder(company);
            }

            var handler = new CompanyAPIHandler();

            handler.UpdateLinkedObjects(company, Request.GetModel());

            Companies.Add(company);

            if (company.Id == 4)
            {
                ValidateOverdueOrders1(4, 4, 0, 30);
            }
            else
            {
                ValidateOverdueOrders1(3, 1);
            }

            return Ok(company);
        }

        private static void AddNewOrder(Company company)
        {
            var newOrder = new NewOrder { Id = 4, Price = company.OverdueOrders[1].Price, Quantity = company.OverdueOrders[1].Quantity };
            OverdueOrders.Add(newOrder);
            company.OverdueOrders[1] = newOrder;
        }

        private void InitEmployees()
        {
            var cntrl = new EmployeesController();
        }

        private void ValidateOverdueOrders1(int companyId, int orderId, int quantity = 0, int price = 101)
        {
            var comp = Companies.FirstOrDefault(x => x.Id == companyId);
            Assert.NotNull(comp);

            NewOrder order = comp.OverdueOrders.FirstOrDefault(x => x.Id == orderId);
            Assert.NotNull(order);
            Assert.Equal(orderId, order.Id);
            Assert.Equal(price, order.Price);
            Assert.Equal(quantity, order.Quantity);
        }

        private void ValidateOverdueOrders2(int companyId, int orderId, int quantity = 0)
        {
            var comp = Companies.FirstOrDefault(x => x.Id == companyId);
            Assert.NotNull(comp);

            MyNewOrder order = comp.MyOverdueOrders.FirstOrDefault(x => x.Id == orderId);
            Assert.NotNull(order);
            Assert.Equal(orderId, order.Id);
            Assert.Equal(444, order.Price);
            Assert.Equal(quantity, order.Quantity);
        }
    }
}