﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace WebStack.QA.Test.OData.Enums
{
    public class EmployeesController : ODataController
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
                    }
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
                    }
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
                    }
                },
            };
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get()
        {
            return Ok(Employees.AsQueryable());
        }

        public IHttpActionResult Get(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key));
        }

        public IHttpActionResult GetAccessLevelFromEmployee(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key).AccessLevel);
        }

        public IHttpActionResult GetNameFromEmployee(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key).Name);
        }

        [EnableQuery]
        public IHttpActionResult GetSkillSetFromEmployee(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key).SkillSet);
        }

        [EnableQuery]
        public IHttpActionResult GetFavoriteSportsFromEmployee(int key)
        {
            var employee = Employees.SingleOrDefault(e => e.ID == key);
            return Ok(employee.FavoriteSports);
        }

        [HttpGet]
        [ODataRoute("Employees({key})/FavoriteSports/LikeMost")]
        public IHttpActionResult GetFavoriteSportLikeMost(int key)
        {
            var firstOrDefault = Employees.FirstOrDefault(e => e.ID == key);
            return Ok(firstOrDefault.FavoriteSports.LikeMost);
        }

        public IHttpActionResult Post(Employee employee)
        {
            employee.ID = Employees.Count + 1;
            Employees.Add(employee);

            return Created(employee);
        }

        public IHttpActionResult Put(int key, Employee employee)
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

        public IHttpActionResult Patch(int key, Delta<Employee> delta)
        {
            Employee originalEmployee = Employees.SingleOrDefault(c => c.ID == key);

            if (originalEmployee == null)
            {
                Employee temp = new Employee();
                delta.Patch(temp);
                Employees.Add(temp);
                return Created(temp);
            }

            delta.Patch(originalEmployee);
            return Ok(delta);
        }

        public IHttpActionResult Delete(int key)
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
        public IHttpActionResult AddSkill([FromODataUri] int key, ODataActionParameters parameters)
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
        public IHttpActionResult ResetDataSource()
        {
            this.InitEmployees();
            return this.StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        [ODataRoute("SetAccessLevel")]
        public IHttpActionResult SetAccessLevel(ODataActionParameters parameters)
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
        public IHttpActionResult GetAccessLevel([FromODataUri] int key)
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
        public IHttpActionResult HasAccessLevel([FromODataUri] int id, [FromODataUri] AccessLevel accessLevel)
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
