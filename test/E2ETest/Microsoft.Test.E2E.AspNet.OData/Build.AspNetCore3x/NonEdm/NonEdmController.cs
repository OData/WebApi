//-----------------------------------------------------------------------------
// <copyright file="NonEdmController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Test.E2E.AspNet.OData.NonEdm
{
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private static readonly List<Customer> customers = new List<Customer>
        {
            new Customer
            {
                Id = 1,
                Name = "Customer 1",
                RelationshipManager = new Employee
                {
                    Id = 1,
                    Name = "Employee 1"
                }
            },
            new EnterpriseCustomer
            {
                Id = 2,
                Name = "Customer 2",
                RelationshipManager = new Person
                {
                    Id = 3,
                    Name = "Employee 3"
                },
                AccountManager = new Employee
                {
                    Id = 2,
                    Name = "Employee 2"
                }
            }
        };

        [HttpGet]
        [EnableQuery]
        [Route("api/Customers")]
        public ActionResult<IEnumerable<Customer>> Get()
        {
            return customers;
        }

        [HttpGet]
        [EnableQuery]
        [Route("api/Customers/Microsoft.Test.E2E.AspNet.OData.NonEdm.EnterpriseCustomer")]
        public ActionResult<IEnumerable<EnterpriseCustomer>> GetFromEnterpriseCustomer()
        {
            return customers.OfType<EnterpriseCustomer>().ToList();
        }

        [HttpGet]
        [EnableQuery]
        [Route("api/Customers/{id}")]
        public ActionResult<Customer> Get(int id)
        {
            var customer = customers.FirstOrDefault(d => d.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return customer;
        }
    }
}
