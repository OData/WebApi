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

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get()
        {
            return Ok(Employees.AsQueryable());
        }

        public ITestActionResult Get(int key)
        {
            var emp = Employees.SingleOrDefault(e => e.ID == key);
            return Ok(Employees.SingleOrDefault(e => e.ID == key));
        }

        [ODataRoute("Employees({key})/Friends")]
        public ITestActionResult GetFriends(int key)
        {
            var emp = Employees.SingleOrDefault(e => e.ID == key);
            return Ok(emp.Friends);
        }

        public ITestActionResult GetAccessLevelFromEmployee(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key).AccessLevel);
        }

        public ITestActionResult GetNameFromEmployee(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key).Name);
        }

        [EnableQuery]
        public ITestActionResult GetSkillSetFromEmployee(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key).SkillSet);
        }

        [EnableQuery]
        public ITestActionResult GetFavoriteSportsFromEmployee(int key)
        {
            var employee = Employees.SingleOrDefault(e => e.ID == key);
            return Ok(employee.FavoriteSports);
        }

        [HttpGet]
        [ODataRoute("Employees({key})/FavoriteSports/LikeMost")]
        public ITestActionResult GetFavoriteSportLikeMost(int key)
        {
            var firstOrDefault = Employees.FirstOrDefault(e => e.ID == key);
            return Ok(firstOrDefault.FavoriteSports.LikeMost);
        }

        public ITestActionResult Post([FromBody]Employee employee)
        {
            employee.ID = Employees.Count + 1;
            Employees.Add(employee);

            return Created(employee);
        }

        [ODataRoute("Employees({key})/FavoriteSports/LikeMost")]
        public ITestActionResult PostToSkillSet(int key, [FromBody]Skill newSkill)
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
        public ITestActionResult PatchEmployees( [FromBody] EdmChangedObjectCollection<Employee> coll)
        {
            Assert.NotNull(coll);
            coll.Patch(Employees);

            return Ok(Employees);
        }

        [ODataRoute("Employees({key})/Friends")]
        [HttpPatch]
        public ITestActionResult PatchFriends(int key, [FromBody] EdmChangedObjectCollection<Friend> friendColl)
        {
            Employee originalEmployee = Employees.SingleOrDefault(c => c.ID == key);
            Assert.NotNull(originalEmployee);

            friendColl.Patch(originalEmployee.Friends);

            return Ok(originalEmployee.Friends);
        }


        public ITestActionResult Put(int key, [FromBody]Employee employee)
        {
            employee.ID = key;
            Employee originalEmployee = Employees.SingleOrDefault(c => c.ID == key);

            if (originalEmployee == null)
            {
                Employees.Add(employee);

                return Created(employee);
            }

            Employees.Remove(originalEmployee);
            Employees.Add(employee);
            return Ok(employee);
        }

        [ODataRoute("Employees({key})")]
        public ITestActionResult Patch(int key, [FromBody]Delta<Employee> delta)
        {

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

        public ITestActionResult Delete(int key)
        {
            IEnumerable<Employee> appliedEmployees = Employees.Where(c => c.ID == key);

            if (appliedEmployees.Count() == 0)
            {
                return BadRequest(string.Format("The entry with ID {0} doesn't exist", key));
            }

            Employee employee = appliedEmployees.Single();
            Employees.Remove(employee);
            return this.StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        public ITestActionResult AddSkill([FromODataUri] int key, [FromBody]ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Skill skill = (Skill)parameters["skill"];

            Employee employee = Employees.FirstOrDefault(e => e.ID == key);
            if (!employee.SkillSet.Contains(skill))
            {
                employee.SkillSet.Add(skill);
            }

            return Ok(employee.SkillSet);
        }

        [HttpPost]
        [ODataRoute("ResetDataSource")]
        public ITestActionResult ResetDataSource()
        {
            this.InitEmployees();
            return this.StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ODataRoute("SetAccessLevel")]
        public ITestActionResult SetAccessLevel([FromBody]ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            int ID = (int)parameters["ID"];
            AccessLevel accessLevel = (AccessLevel)parameters["accessLevel"];
            Employee employee = Employees.FirstOrDefault(e => e.ID == ID);
            employee.AccessLevel = accessLevel;
            return Ok(employee.AccessLevel);
        }

        [HttpGet]
        public ITestActionResult GetAccessLevel([FromODataUri] int key)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            Employee employee = Employees.FirstOrDefault(e => e.ID == key);

            return Ok(employee.AccessLevel);
        }

        [HttpGet]
        [ODataRoute("HasAccessLevel(ID={id},AccessLevel={accessLevel})")]
        public ITestActionResult HasAccessLevel([FromODataUri] int id, [FromODataUri] AccessLevel accessLevel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Employee employee = Employees.FirstOrDefault(e => e.ID == id);
            var result = employee.AccessLevel.HasFlag(accessLevel);
            return Ok(result);
        }
    }
}
