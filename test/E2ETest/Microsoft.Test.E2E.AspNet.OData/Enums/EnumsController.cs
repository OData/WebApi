// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.Enums
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
        public ITestActionResult Get()
        {
            return Ok(Employees.AsQueryable());
        }

        public ITestActionResult Get(int key)
        {
            return Ok(Employees.SingleOrDefault(e => e.ID == key));
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

        public ITestActionResult Patch(int key, [FromBody]Delta<Employee> delta)
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

    public class WeatherForecastController : TestODataController
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [EnableQuery]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Id = index,
                Status = index % 2 == 0 ? Status.SoldOut : Status.InStore,
                Skill = index % 2 == 0 ? Skill.CSharp : Skill.Sql
            })
            .ToArray();
        }
    }
}
