// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.NavigationPropertyOnComplexType
{
    public class PeopleController : TestODataController
    {
        private PeopleRepository _repo = new PeopleRepository();

        [HttpGet]
        [EnableQuery]
        public IEnumerable<Person> Get()
        {
            return _repo.Get();
        }

        [HttpGet]
        [EnableQuery]
        public ITestActionResult Get([FromODataUri]int key)
        {
            Person person = _repo.people.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }

            return Ok(person);
        }

        [EnableQuery]
        public ITestActionResult GetLocationFromPerson([FromODataUri]int key)
        {
            Person person = _repo.people.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person.Location);
        }

        [EnableQuery]
        public ITestActionResult GetLocationOfAddress([FromODataUri]int key)
        {
            Person person = _repo.people.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person.Location as Address);
        }

        [EnableQuery]
        public ITestActionResult GetLocationOfGeolocation([FromODataUri]int key)
        {
            Person person = _repo.people.FirstOrDefault(p => p.Id == key);
            if (person == null)
            {
                return NotFound();
            }
            return Ok(person.Location as GeoLocation);
        }

        [ODataRoute("people({id})/Location/ZipCode")]
        public ITestActionResult GetZipCode([FromODataUri]int id)
        {
            return Ok(_repo.people.FirstOrDefault().Location.ZipCode);
        }

        [ODataRoute("people({id})/Location/ZipCode/$ref")]
        public ITestActionResult CreateRefToZipCode([FromODataUri] int id, [FromBody] ZipCode zip)
        {
            return Ok(zip);
        }
    }
}
