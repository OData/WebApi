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
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Xunit;
using static Microsoft.AspNet.OData.PatchDelegates;

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
        private static IList<Employee> Employees = null;

        private void InitEmployees()
        {
            Employees = new List<Employee>
            {
                new Employee()
                {
                    ID=1,
                    Name="Name1",
                    SkillSet=new List<Skill>{Skill.CSharp,Skill.Sql},
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Execute,
                    FavoriteSports=new FavoriteSports()
                    {
                        LikeMost=Sport.Pingpong,
                        Like=new List<Sport>{Sport.Pingpong,Sport.Basketball}
                    },
                    Friends = new List<Friend>{new Friend { Id=1,Name="Test0"} ,new Friend { Id=2,Name="Test1"} }
                },
                new Employee()
                {
                    ID=2,Name="Name2",
                    SkillSet=new List<Skill>(),
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Read,
                    FavoriteSports=new FavoriteSports()
                    {
                        LikeMost=Sport.Pingpong,
                        Like=new List<Sport>{Sport.Pingpong,Sport.Basketball}
                    },
                    Friends = new List<Friend>{new Friend { Id=1,Name="Test0"} ,new Friend { Id=2,Name="Test1"} }
                },
                new Employee(){
                    ID=3,Name="Name3",
                    SkillSet=new List<Skill>{Skill.Web,Skill.Sql},
                    Gender=Gender.Female,
                    AccessLevel=AccessLevel.Read|AccessLevel.Write,
                    FavoriteSports=new FavoriteSports()
                    {
                        LikeMost=Sport.Pingpong|Sport.Basketball,
                        Like=new List<Sport>{Sport.Pingpong,Sport.Basketball}
                    },
                    Friends = new List<Friend>{new Friend { Id=1,Name="Test0"} ,new Friend { Id=2,Name="Test1"} }
                },
            };
        }

        public DeltaSet<NewFriend> PatchWithUsersMethod(DeltaSet<NewFriend> friendColl)
        {
            var friendCollection = new FriendColl<NewFriend>() { new NewFriend { Id = 2, Age = 15 } };

            DeleteDelegate _deleteDelegate = new DeleteDelegate(DeleteMethod);
            GetOrCreateDelegate _createDelegate = new GetOrCreateDelegate(GetOrCreateMethod);

            var changedObjColl = friendColl.Patch(_createDelegate, _deleteDelegate);

            return changedObjColl;
        }

        public EdmChangedObjectCollection PatchWithUsersMethodTypeLess(EdmChangedObjectCollection friendColl)
        {
            var entity = new EdmEntityObject(friendColl[0].GetEdmType().AsEntity());
            entity.TrySetPropertyValue("Id", 2);

            var friendCollection = new FriendColl<EdmStructuredObject>() { entity };

            DeleteDelegate _deleteDelegate = new DeleteDelegate(DeleteMethod);
            GetOrCreateDelegate _createDelegate = new GetOrCreateDelegate(GetOrCreateMethod);

            var changedObjColl = friendColl.Patch(_createDelegate, _deleteDelegate);

            return changedObjColl;
        }

        public void DeleteMethod(object original)
        {
            //Delete Logic

        }

        public object GetOrCreateMethod(IDictionary<string, object> keys)
        {
            return new NewFriend();
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

        [ODataRoute("Employees({key})/FavoriteSports/LikeMost")]
        public ITestActionResult PostToSkillSet(int key, [FromBody] Skill newSkill)
        {
            Employee employee = Employees.FirstOrDefault(e => e.ID == key);
            if (employee == null)
            {
                return NotFound();
            }
            employee.SkillSet.Add(newSkill);
            return Updated(employee.SkillSet);
        }

        [ODataRoute("Employees")]
        [HttpPatch]
        public ITestActionResult PatchEmployees([FromBody] DeltaSet<Employee> coll)
        {
            InitEmployees();

            Assert.NotNull(coll);
            var returncoll = coll.Patch(Employees);

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

            if(key == 1)
            {
                var deltaSet = PatchWithUsersMethod(friendColl);
                
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