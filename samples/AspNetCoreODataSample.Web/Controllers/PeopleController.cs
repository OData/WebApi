// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using AspNetCoreODataSample.Web.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreODataSample.Web.Controllers
{
    public class PeopleController : ODataController
    {
        [EnableQuery]
        public IActionResult Get([FromODataUri]string keyFirstName, [FromODataUri]string keyLastName)
        {
            Person m = new Person
            {
                FirstName = keyFirstName,
                LastName = keyLastName,
                DynamicProperties = new Dictionary<string, object>
                {
                    { "abc", "abcValue" }
                }
                MyLevel = Level.High
            };

            return Ok(m);
        }

        [EnableQuery]
        public IActionResult Post([FromBody]Person person)
        {
            return Created(person);
        }
    }
}
