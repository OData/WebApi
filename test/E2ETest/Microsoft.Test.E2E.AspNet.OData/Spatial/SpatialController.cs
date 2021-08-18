//-----------------------------------------------------------------------------
// <copyright file="SpatialController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.Spatial;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Spatial
{
    public class SpatialCustomersController : TestODataController
    {
        private static readonly IEnumerable<SpatialCustomer> Customers;

        static SpatialCustomersController()
        {
            if (Customers == null)
            {
                string[] names = { "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune" };

                Customers = Enumerable.Range(1, 7).Select(e => new SpatialCustomer
                {
                    CustomerId = e,
                    Name = names[e - 1],
                    Location = GeographyFactory.Point(e, e, e, e),
                    Region = (e != 2
                        ? GeographyFactory.LineString(1 + e, 2 + e).LineTo(3 * e, 4 * e)
                        : GeographyFactory.LineString(55, 66, null, 0).LineTo(33, 44, null, 12.3)),
                    HomePoint = GeometryFactory.Point(e * 2, e * 5)
                });
            }
        }

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(Customers);
        }

        [EnableQuery]
        public ITestActionResult Get(int key)
        {
            SpatialCustomer customer = Customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [EnableQuery]
        public ITestActionResult Post([FromBody]SpatialCustomer customer)
        {
            // Assert part
            Assert.Equal("Sam", customer.Name);

            Assert.Equal(7, customer.Location.Longitude);
            Assert.Equal(8, customer.Location.Latitude);
            Assert.Equal(9, customer.Location.Z);
            Assert.Equal(10, customer.Location.M);

            return Created(customer);
        }

        [EnableQuery]
        public ITestActionResult Patch(int key, Delta<SpatialCustomer> customer)
        {
            // Assert part
            Assert.Equal(3, key);

            Assert.Equal(new[] {"Location"}, customer.GetChangedPropertyNames());

            object value;
            customer.TryGetPropertyValue("Location", out value);

            GeographyPoint point = value as GeographyPoint;
            Assert.NotNull(point);
            Assert.Equal(7, point.Longitude);
            Assert.Equal(8, point.Latitude);
            Assert.Equal(9, point.Z);
            Assert.Equal(10, point.M);

            return Ok();
        }
    }
}
