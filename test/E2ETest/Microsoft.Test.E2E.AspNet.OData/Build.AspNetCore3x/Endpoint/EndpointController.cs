//-----------------------------------------------------------------------------
// <copyright file="EndpointController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Endpoint
{
    public class EpCustomersController : TestODataController, IDisposable
    {
        private EndpointDbContext _context;

        public EpCustomersController(EndpointDbContext context)
        {
            _context = context;
            if (_context.Customers.Count() == 0)
            {
                IList<EpCustomer> customers = new List<EpCustomer>
                {
                    new EpCustomer
                    {
                        Name = "Jonier",
                        HomeAddress = new EpAddress { City = "Redmond", Street = "156 AVE NE" },
                        FavoriteAddresses = new List<EpAddress>
                        {
                            new EpAddress { City = "Redmond", Street = "256 AVE NE" },
                            new EpAddress { City = "Redd", Street = "56 AVE NE" },
                        },
                        Order = new EpOrder { Title = "104m" },
                        Orders = Enumerable.Range(0, 2).Select(e => new EpOrder { Title = "abc" + e }).ToList()
                    },
                    new EpCustomer
                    {
                        Name = "Sam",
                        HomeAddress = new EpAddress { City = "Bellevue", Street = "Main St NE" },
                        FavoriteAddresses = new List<EpAddress>
                        {
                            new EpAddress { City = "Red4ond", Street = "456 AVE NE" },
                            new EpAddress { City = "Re4d", Street = "51 NE" },
                        },
                        Order = new EpOrder { Title = "Zhang" },
                        Orders = Enumerable.Range(0, 2).Select(e => new EpOrder { Title = "xyz" + e }).ToList()
                    },
                    new EpCustomer
                    {
                        Name = "Peter",
                        HomeAddress = new EpAddress { City = "Hollewye", Street = "Main St NE" },
                        FavoriteAddresses = new List<EpAddress>
                        {
                            new EpAddress { City = "R4mond", Street = "546 NE" },
                            new EpAddress { City = "R4d", Street = "546 AVE" },
                        },
                        Order = new EpOrder { Title = "Jichan" },
                        Orders = Enumerable.Range(0, 2).Select(e => new EpOrder { Title = "ijk" + e }).ToList()
                    }
                };

                foreach (var customer in customers)
                {
                    _context.Customers.Add(customer);
                    _context.Orders.Add(customer.Order);
                    _context.Orders.AddRange(customer.Orders);
                }

                _context.SaveChanges();
            }
        }

        [HttpGet]
        [EnableQuery]
        public IEnumerable<EpCustomer> Get()
        {
            return _context.Customers;
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri]int key)
        {
            EpCustomer customer = _context.Customers.FirstOrDefault(p => p.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult GetHomeAddressFromEpCustomer([FromODataUri]int key)
        {
            EpCustomer customer = _context.Customers.FirstOrDefault(p => p.Id == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.HomeAddress);
        }

        [HttpPost]
        public ITestActionResult Post([FromBody]EpCustomer customer)
        {
            Assert.NotNull(customer);
            Assert.Equal("NewCustomerName", customer.Name);
            Assert.NotNull(customer.HomeAddress);
            Assert.Equal("NewCity", customer.HomeAddress.City);
            Assert.Equal("NewStreet", customer.HomeAddress.Street);
            Assert.NotNull(customer.FavoriteAddresses);
            Assert.Empty(customer.FavoriteAddresses);

            return Created(customer);
        }

        [HttpDelete]
        public ITestActionResult Delete([FromODataUri]int key)
        {
            Assert.Equal(99, key); // test the magic interger from test method
            return StatusCode(HttpStatusCode.NoContent);
        }

        public void Dispose()
        {
            // DO NOT dispose _context. Otherwise the $batch will get exception to complain about the re-using the disposed object.
            // _context.Dispose();
        }
    }
}
