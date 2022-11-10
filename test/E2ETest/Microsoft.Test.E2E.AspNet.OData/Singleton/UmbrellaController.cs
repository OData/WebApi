//-----------------------------------------------------------------------------
// <copyright file="UmbrellaController.cs" company=".NET Foundation">
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

namespace Microsoft.Test.E2E.AspNet.OData.Singleton
{
    /// <summary>
    /// Present a singleton named "Umbrella"
    /// Use convention routing
    /// </summary>
    public class UmbrellaController : TestODataController
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
                Projects = new List<Project>(),
            };
        }

        #region Query
        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(Umbrella);
        }

        public ITestActionResult GetFromSubCompany()
        {
            var subCompany = Umbrella as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany);
            }
            return BadRequest();
        }

        public ITestActionResult GetRevenueFromCompany()
        {
            return Ok(Umbrella.Revenue);
        }

        public ITestActionResult GetNameFromCompany()
        {
            return Ok(Umbrella.Name);
        }

        public ITestActionResult GetCategoryFromCompany()
        {
            return Ok(Umbrella.Category);
        }

        public ITestActionResult GetLocationFromSubCompany()
        {
            var subCompany = Umbrella as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Location);
            }
            return BadRequest();
        }

        public ITestActionResult GetOffice()
        {
            var subCompany = Umbrella as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Office);
            }
            return BadRequest();
        }

        [EnableQuery]
        public ITestActionResult GetPartnersFromCompany()
        {
            return Ok(Umbrella.Partners);
        }
        #endregion

        #region Update
        public ITestActionResult Put([FromBody]Company newCompany)
        {
            Umbrella = newCompany;
            return StatusCode(HttpStatusCode.NoContent);
        }

        public ITestActionResult PutUmbrellaFromSubCompany([FromBody]SubCompany newCompany)
        {
            Umbrella = newCompany;
            return StatusCode(HttpStatusCode.NoContent);
        }

        public ITestActionResult Patch([FromBody]Delta<Company> item)
        {
            item.Patch(Umbrella);
            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        #region Navigation link
        [AcceptVerbs("POST")]
        public ITestActionResult CreateRef(string navigationProperty, [FromBody] Uri link)
        {
            int relatedKey = GetRequestValue<int>(link);
            Partner partner = PartnersController.Partners.First(x => x.ID == relatedKey);

            if (navigationProperty != "Partners" || partner == null)
            {
                return BadRequest();
            }

            Umbrella.Partners.Add(partner);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("DELETE")]
        public ITestActionResult DeleteRef(string relatedKey, string navigationProperty)
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
        public ITestActionResult PostToPartners([FromBody] Partner partner)
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
        public ITestActionResult ResetDataSourceOnCompany()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }

        public ITestActionResult GetPartnersCount()
        {
            return Ok(Umbrella.Partners.Count);
        }
        #endregion
    }
}
