// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;

namespace WebStack.QA.Test.OData.Singleton
{
    public class PartnersController : ODataController
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
        public IHttpActionResult Get()
        {
            return Ok(Partners.AsQueryable());
        }

        [EnableQuery]
        public IHttpActionResult GetPartners()
        {
            return Ok(Partners.AsQueryable());
        }

        [EnableQuery]
        public IHttpActionResult Get(int key)
        {
            return Ok(Partners.SingleOrDefault(p=>p.ID == key));
        }

        public IHttpActionResult GetCompanyFromPartner([FromODataUri] int key)
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
        public IHttpActionResult POST([FromBody] Partner partner)
        {
            Partners.Add(partner);
            return Created(partner);
        }
        #endregion

        #region Navigation link
        [AcceptVerbs("PUT")]
        public IHttpActionResult CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
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

        public IHttpActionResult DeleteRef([FromODataUri] int key, string navigationProperty)
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
        public IHttpActionResult PutToCompany(int key, Company company)
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
        public IHttpActionResult PatchToCompany(int key, Delta<Company> company)
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
        public IHttpActionResult PostToCompany(int key, [FromBody] Company company)
        {
            return Ok();
        }

        #endregion

        #region Action
        [HttpPost]
        public IHttpActionResult ResetDataSourceOnCollectionOfPartner()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion
    }
}
