// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.BulkOperation;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Vehicle;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.BulkInsert1
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

        public static IList<EdmStructuredObject> EmployeesTypeless = null;

        private  List<Friend> Friends = null;

        private IEdmTypeReference edmType;

        private void InitEmployees()
        {
            Friends = new List<Friend> { new Friend { Id = 1, Name = "Test0" }, new Friend { Id = 2, Name = "Test1" } }; 

            Employees = new List<Employee>
            {
                new Employee()
                {
                    ID=1,
                    Name="Name1",
                    SkillSet=new List<Skill>{Skill.CSharp,Skill.Sql},
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Execute,
                    
                    Friends = this.Friends
                },
                new Employee()
                {
                    ID=2,Name="Name2",
                    SkillSet=new List<Skill>(),
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Read,
                  
                    Friends = this.Friends
                },
                new Employee(){
                    ID=3,Name="Name3",
                    SkillSet=new List<Skill>{Skill.Web,Skill.Sql},
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Read|AccessLevel.Write,
                   
                    Friends = this.Friends
                },
            };
        }

        private void InitTypeLessEmployees(IEdmEntityTypeReference entityType)
        {
            EmployeesTypeless = new List<EdmStructuredObject>();
            var emp1 = new EdmEntityObject(entityType);
            emp1.TrySetPropertyValue("ID", 1);
            emp1.TrySetPropertyValue("Name", "Test1");

            var friendType = entityType.DeclaredNavigationProperties().First().Type.Definition.AsElementType() as IEdmEntityType;

            var friends = new List<EdmStructuredObject>();
            var friend1 = new EdmEntityObject(friendType);
            friend1.TrySetPropertyValue("Id", 1);
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

            EmployeesTypeless.Add(emp1);
            EmployeesTypeless.Add(emp2);
        }

        public DeltaSet<NewFriend> PatchWithUsersMethod(DeltaSet<NewFriend> friendColl, Employee employee)
        {
            var changedObjColl = friendColl.Patch(new NewFriendPatchHandler(employee));

            return changedObjColl;
        }
        public EdmChangedObjectCollection PatchWithUsersMethodTypeLess(EdmChangedObjectCollection friendColl)
        {
            edmType = friendColl[0].GetEdmType();
            var entity = edmType.AsEntity();
            InitTypeLessEmployees(entity);

            var changedObjColl = friendColl.Patch(new EmployeeTypelessPatchHandler(entity));

            return changedObjColl;
        }

        public EdmChangedObjectCollection EmployeePatchWithUsersMethodTypeLess(EdmChangedObjectCollection empColl)
        {
            edmType = empColl[0].GetEdmType();
            var entity = edmType.AsEntity();
            InitTypeLessEmployees(entity);

            var changedObjColl = empColl.Patch(EmployeesTypeless);
            // var changedObjColl = empColl.Patch(new EmployeeTypelessPatchHandler(entity));

            return changedObjColl;
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get()
        {
            return Ok(Employees.AsQueryable());
        }

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


        [ODataRoute("Employees")]
        [HttpPatch]
        public ITestActionResult PatchEmployees([FromBody] DeltaSet<Employee> coll)
        {
            InitEmployees();

            Assert.NotNull(coll);

            var returncoll = coll.Patch(Employees);
            //var returncoll = coll.Patch(new EmployeePatchHandler());

            return Ok(returncoll);
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
                var deltaSet = PatchWithUsersMethod(friendColl, Employees.First(x=>x.ID ==key));

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
                var changedObjColl = PatchWithUsersMethodTypeLess(friendColl);

                return Ok(changedObjColl);
            }
            else
            {
                var entity = new EdmEntityObject(friendColl[0].GetEdmType().AsEntity());
                entity.TrySetPropertyValue("Id", 2);

                var friendCollection = new FriendColl<EdmStructuredObject>() { entity };

                var changedObjColl = friendColl.Patch(friendCollection);

                return Ok(changedObjColl);
            }
        }


        [ODataRoute("UnTypedEmployees")]
        [HttpPatch]
        public ITestActionResult PatchUnTypedEmployees([FromBody] EdmChangedObjectCollection empColl)
        {
         
            var changedObjColl = EmployeePatchWithUsersMethodTypeLess(empColl);

            return Ok(changedObjColl);
           
        }



        [ODataRoute("Employees({key})")]
        public ITestActionResult Patch(int key, [FromBody] Delta<Employee> delta)
        {
            InitEmployees();

            delta.TrySetPropertyValue("ID", key); // It is the key property, and should not be updated.

            Employee employee = Employees.FirstOrDefault(e => e.ID == key);
            if (employee == null)
            {
                employee = new Employee();
                delta.Patch(employee);
                return Created(employee);
            }

            try
            {
                delta.Patch(employee);
            }
            catch (ArgumentException ae)
            {
                return BadRequest(ae.Message);
            }

            return Ok(employee);
        }


        [HttpPost]
        [ODataRoute("ResetDataSource")]
        public ITestActionResult ResetDataSource()
        {
            this.InitEmployees();
            return this.StatusCode(HttpStatusCode.NoContent);
        }

    }
}