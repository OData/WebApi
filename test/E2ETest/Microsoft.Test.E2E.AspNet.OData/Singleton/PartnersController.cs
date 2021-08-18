//-----------------------------------------------------------------------------
// <copyright file="PartnersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.Singleton
{
    public class PartnersController : TestODataController
    {
        public static List<Partner> Partners;

        static PartnersController()
        {
            InitData();
        }

        private static void InitData()
        {
            Partners = Enumerable.Range(0, 10).Select(i =>
                   new Partner()
                   {
                       ID = i,
                       Name = string.Format("Name {0}", i)
                   }).ToList();
        }

        #region Query
        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(Partners.AsQueryable());
        }

        [EnableQuery]
        public ITestActionResult GetPartners()
        {
            return Ok(Partners.AsQueryable());
        }

        [EnableQuery]
        public ITestActionResult Get(int key)
        {
            return Ok(Partners.SingleOrDefault(p=>p.ID == key));
        }

        public ITestActionResult GetCompanyFromPartner([FromODataUri] int key)
        {
            var company = Partners.First(e => e.ID == key).Company;
            if (company == null)
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
            return Ok(company);
        }
        #endregion 

        #region Update
        public ITestActionResult POST([FromBody] Partner partner)
        {
            Partners.Add(partner);
            return Created(partner);
        }
        #endregion

        #region Navigation link
        [AcceptVerbs("PUT")]
        public ITestActionResult CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            if (navigationProperty != "Company")
            {
                return BadRequest();
            }

            var strArray = link.AbsoluteUri.Split('/');
            var company = strArray[strArray.Length - 1];

            if (company == "Umbrella")
            {
                Partners.First(e => e.ID == key).Company = UmbrellaController.Umbrella;                
            }
            else if (company == "MonstersInc")
            {
                Partners.First(e => e.ID == key).Company = MonstersIncController.MonstersInc;
            }
            else
                return BadRequest();

            return StatusCode(HttpStatusCode.NoContent);
        }

        public ITestActionResult DeleteRef([FromODataUri] int key, string navigationProperty)
        {
            if (navigationProperty != "Company")
            {
                return BadRequest();
            }

            Partners.First(e => e.ID == key).Company = null;
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ODataRoute("Partners({key})/Company")]
        public ITestActionResult PutToCompany(int key, [FromBody]Company company)
        {
            var navigateCompany = Partners.First(e => e.ID == key).Company;
            Partners.First(e => e.ID == key).Company = company;
            if (navigateCompany.Name == "Umbrella")
            {
                UmbrellaController.Umbrella = navigateCompany;
            }
            else
            {
                MonstersIncController.MonstersInc = navigateCompany;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPatch]
        [ODataRoute("Partners({key})/Company")]
        public ITestActionResult PatchToCompany(int key, Delta<Company> company)
        {
            var navigateCompany = Partners.First(e => e.ID == key).Company;
            company.Patch(Partners.First(e => e.ID == key).Company);
            if (navigateCompany.Name == "Umbrella")
            {
                company.Patch(UmbrellaController.Umbrella);
            }
            else
            {
                company.Patch(MonstersIncController.MonstersInc);
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        public ITestActionResult PostToCompany(int key, [FromBody] Company company)
        {
            return Ok();
        }

        #endregion

        #region Action
        [HttpPost]
        public ITestActionResult ResetDataSourceOnCollectionOfPartner()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion
    }
}
