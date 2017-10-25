﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Microsoft.AspNet.OData;
using WebStack.QA.Test.OData.Common;

namespace WebStack.QA.Test.OData.Singleton
{
    /// <summary>
    /// Present a singleton named "Umbrella"
    /// Use convention routing
    /// </summary>
    public class UmbrellaController : ODataController
    {
        public static Company Umbrella;

        static UmbrellaController()
        {
            InitData();
        }

        private static void InitData()
        {
            Umbrella = new Company()
            {
                ID = 1,
                Name = "Umbrella",
                Revenue = 1000,
                Category = CompanyCategory.Communication,
                Partners = new List<Partner>(),
                Branches = new List<Office>(),
            };
        }

        #region Query
        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(Umbrella);
        }

        public IHttpActionResult GetFromSubCompany()
        {
            var subCompany = Umbrella as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany);
            }
            return BadRequest("The target cannot be casted");
        }

        public IHttpActionResult GetRevenueFromCompany()
        {
            return Ok(Umbrella.Revenue);
        }

        public IHttpActionResult GetNameFromCompany()
        {
            return Ok(Umbrella.Name);
        }

        public IHttpActionResult GetCategoryFromCompany()
        {
            return Ok(Umbrella.Category);
        }

        public IHttpActionResult GetLocationFromSubCompany()
        {
            var subCompany = Umbrella as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Location);
            }
            return BadRequest("The target cannot be casted");
        }

        public IHttpActionResult GetOffice()
        {
            var subCompany = Umbrella as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Office);
            }
            return BadRequest("The target cannot be casted");
        }

        [EnableQuery]
        public IHttpActionResult GetPartnersFromCompany()
        {
            return Ok(Umbrella.Partners);
        }
        #endregion

        #region Update
        public IHttpActionResult Put(Company newCompany)
        {
            Umbrella = newCompany;
            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult PutUmbrellaFromSubCompany(SubCompany newCompany)
        {
            Umbrella = newCompany;
            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult Patch(Delta<Company> item)
        {
            item.Patch(Umbrella);
            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        #region Navigation link
        [AcceptVerbs("POST")]
        public IHttpActionResult CreateRef(string navigationProperty, [FromBody] Uri link)
        {
            int relatedKey = Request.GetKeyValue<int>(link);
            Partner partner = PartnersController.Partners.First(x => x.ID == relatedKey);

            if (navigationProperty != "Partners" || partner == null)
            {
                return BadRequest();
            }

            Umbrella.Partners.Add(partner);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("DELETE")]
        public IHttpActionResult DeleteRef(string relatedKey, string navigationProperty)
        {
            int key = int.Parse(relatedKey);
            Partner partner = Umbrella.Partners.First(x => x.ID == key);

            if (navigationProperty != "Partners")
            {
                return BadRequest();
            }

            Umbrella.Partners.Remove(partner);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        public IHttpActionResult PostToPartners([FromBody] Partner partner)
        {
            PartnersController.Partners.Add(partner);
            if (Umbrella.Partners == null)
            {
                Umbrella.Partners = new List<Partner>() { partner };
            }
            else
            {
                Umbrella.Partners.Add(partner);
            }

            return Created(partner);
        }
        #endregion

        #region Action and Function
        [HttpPost]
        public IHttpActionResult ResetDataSourceOnCompany()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }

        public IHttpActionResult GetPartnersCount()
        {
            return Ok(Umbrella.Partners.Count);
        }
        #endregion
    }
}
