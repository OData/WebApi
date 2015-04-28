using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Microsoft.OData.Core;

namespace WebStack.QA.Test.OData.LowerCamelCase
{
    public class EmployeesController : ODataController
    {
        public EmployeesController()
        {
            if (null == _employees)
            {
                InitEmployeesAndManagers();
            }
        }

        /// <summary>
        /// static so that the data is shared among requests.
        /// </summary>
        private static List<Employee> _employees = null;

        private static void InitEmployeesAndManagers()
        {
            _employees = Enumerable.Range(1, 5).Select(i =>
                        new Employee
                        {
                            ID = i,
                            FullName = "Name" + i,
                            Sex = Gender.Female,
                            Address = new Address()
                            {
                                Street = "Street" + i,
                                City = "City" + i,
                            },
                        }).ToList();
            for (int i = 6; i <= 10; i++)
            {
                _employees.Add(
                            new Manager
                            {
                                ID = i,
                                FullName = "Name" + i,
                                Sex = Gender.Male,
                                Address = new Address()
                                {
                                    Street = "Street" + i,
                                    City = "City" + i,
                                },
                            });
            }

            foreach (Employee employee in _employees)
            {
                employee.Next = _employees.SingleOrDefault(e => e.ID == employee.ID + 1);
            }

            // Setup a circular reference
            Employee employeeNo2 = _employees.Single(e => e.ID == 2);
            employeeNo2.Next = _employees.Single(e => e.ID == 1);

            // Report lines:
            // 6 -> 7 -> 8 -> 9 -> 10
            // ^    ^    ^    ^    ^
            // 1    2    3    4    5
            for (int i = 1; i <= 5; i++)
            {
                Employee employee = _employees.Single(e => e.ID == i);
                employee.Manager = _employees.Single(e => e.ID == employee.ID + 5) as Manager;
            }
            for (int i = 6; i <= 9; i++)
            {
                Employee manager = _employees.Single(e => e.ID == i);
                manager.Manager = (Manager)_employees.Single(e => e.ID == manager.ID + 1);
            }
            for (int i = 6; i <= 10; i++)
            {
                Manager manager = (Manager)_employees.Single(e => e.ID == i);
                manager.DirectReports = _employees.Where(e => e.Manager != null && e.Manager.ID == manager.ID).ToList();
            }
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IHttpActionResult Get()
        {
            return Ok(_employees.AsQueryable());
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        [ODataRoute("Employees/WebStack.QA.Test.OData.LowerCamelCase.Manager")]
        public IHttpActionResult GetManagers()
        {
            return Ok(_employees.OfType<Manager>().AsQueryable());
        }

        public IHttpActionResult Get(int key, ODataQueryOptions<Employee> options)
        {
            if (options.SelectExpand != null)
            {
                options.SelectExpand.LevelsMaxLiteralExpansionDepth = 3;
            }

            var validationSettings = new ODataValidationSettings { MaxExpansionDepth = 3 };

            try
            {
                options.Validate(validationSettings);
            }
            catch (ODataException e)
            {
                HttpResponseMessage resoponse = Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message, e);
                return ResponseMessage(resoponse);
            }

            var querySettings = new ODataQuerySettings();
            Employee employee = _employees.Single(e => e.ID == key);
            var result = options.ApplyTo(employee, new ODataQuerySettings());
            Type type = result.GetType();
            return Ok(result, type);

        }

        public IHttpActionResult GetName(int key)
        {
            return Ok(_employees.Single(e => e.ID == key).FullName);
        }

        [ODataRoute("GetAddress(id={id})")]
        public IHttpActionResult GetAddress(int id)
        {
            return Ok(_employees.Single(e => e.ID == id).Address);
        }

        // GET ~/Employees/WebStack.QA.Test.OData.LowerCamelCase.GetEarliestTwoEmployees()
        [EnableQuery]
        public IHttpActionResult GetEarliestTwoEmployeesOnCollectionOfEmployee()
        {
            return Ok(_employees.AsQueryable().OrderBy(e => e.ID).Take(2));
        }

        // GET ~/Employees(1)/manager
        [EnableQuery(MaxExpansionDepth = 4)]
        public IHttpActionResult GetManager(int key)
        {
            return Ok(_employees.Single(e => e.ID == key).Manager);
        }

        public IHttpActionResult Post(Employee employee)
        {
            employee.ID = _employees.Count + 1;
            _employees.Add(employee);

            return Created(employee);
        }

        public IHttpActionResult Put(int key, Employee employee)
        {
            if (key != employee.ID)
            {
                return BadRequest("The ID of customer is not matched with the key");
            }

            Employee originalEmployee = _employees.FirstOrDefault(c => c.ID == key);
            _employees.Remove(originalEmployee);
            _employees.Add(employee);
            return Ok(employee);
        }

        [ODataRoute("SetAddress")]
        public IHttpActionResult SetAddress(ODataActionParameters parameters)
        {
            int id = int.Parse(parameters["id"].ToString());
            Address address = parameters["address"] as Address;
            Employee employee = _employees.FirstOrDefault(e => e.ID == id);

            employee.Address = address;
            return Ok(employee);
        }

        public IHttpActionResult Delete(int key)
        {
            IEnumerable<Employee> applied_employees = _employees.Where(c => c.ID == key);

            if (applied_employees.Count() == 0)
            {
                return BadRequest(string.Format("The entry with ID {0} doesn't exist", key));
            }

            Employee employee = applied_employees.Single();
            _employees.Remove(employee);
            return Ok(employee);
        }

        [ODataRoute("ResetDataSource")]
        public IHttpActionResult ResetDataSource()
        {
            InitEmployeesAndManagers();
            return this.StatusCode(HttpStatusCode.NoContent);
        }

        private IHttpActionResult Ok(object content, Type type)
        {
            var resultType = typeof(OkNegotiatedContentResult<>).MakeGenericType(type);
            return Activator.CreateInstance(resultType, content, this) as IHttpActionResult;
        }
    }
}
