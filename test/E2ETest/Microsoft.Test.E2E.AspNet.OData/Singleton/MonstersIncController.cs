//-----------------------------------------------------------------------------
// <copyright file="MonstersIncController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.Singleton
{
    /// <summary>
    /// Present a singleton named "MonstersInc"
    /// Use attribute routing
    /// </summary>
    [ODataRoutePrefix("MonstersInc")]
    public class MonstersIncController : TestODataController
    {
        public static Company MonstersInc;

        static MonstersIncController()
        {
            InitData();
        }

        private static void InitData()
        {
            MonstersInc = new Company()
            {
                ID = 1,
                Name = "MonstersInc",
                Revenue = 1000,
                Category = CompanyCategory.Electronics,
                Partners = new List<Partner>(),
                Branches = new List<Office>() { new Office { City = "Shanghai", Address = "Minhang" }, new Office { City = "Xi'an", Address = "Dayanta" } },
            };
        }

        #region Query
        [EnableQuery]
        [HttpGet]
        [ODataRoute]
        public ITestActionResult QueryCompany()
        {
            return Ok(MonstersInc);
        }

        [HttpGet]
        [ODataRoute("Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany")]
        public ITestActionResult QueryCompanyFromDerivedType()
        {
            var subCompany = MonstersInc as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany);
            }
            return BadRequest("The target cannot be casted");
        }

        [ODataRoute("Revenue")]
        public ITestActionResult GetCompanyRevenue()
        {
            return Ok(MonstersInc.Revenue);
        }

        [HttpGet]
        [ODataRoute("Branches/$count")]
        public ITestActionResult GetBranchesCount(ODataQueryOptions<Office> options)
        {
            IQueryable<Office> eligibleBranches = MonstersInc.Branches.AsQueryable();
            if (options.Filter != null)
            {
                eligibleBranches = options.Filter.ApplyTo(eligibleBranches, new ODataQuerySettings()).Cast<Office>();
            }
            return Ok(eligibleBranches.Count());
        }

        [ODataRoute("Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany/Location")]
        public ITestActionResult GetDerivedTypeProperty()
        {
            var subCompany = MonstersInc as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Location);
            }
            return BadRequest("The target cannot be casted");
        }

        [HttpGet]
        [ODataRoute("Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany/Office")]
        public ITestActionResult QueryDerivedTypeComplexProperty()
        {
            var subCompany = MonstersInc as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Office);
            }
            return BadRequest("The target cannot be casted");
        }

        [HttpGet]
        [ODataRoute("Partners")]
        public ITestActionResult QueryNavigationProperty()
        {
            return Ok(MonstersInc.Partners);
        }
        #endregion

        #region Update
        [HttpPut]
        [ODataRoute]
        public ITestActionResult UpdateCompanyByPut([FromBody] Company newCompany)
        {
            MonstersInc = newCompany;
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ODataRoute("Microsoft.Test.E2E.AspNet.OData.Singleton.SubCompany")]
        public ITestActionResult UpdateCompanyByPutWithDerivedTypeObject([FromBody] SubCompany newCompany)
        {
            MonstersInc = newCompany;
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPatch]
        [ODataRoute]
        public ITestActionResult UpdateCompanyByPatch([FromBody] Delta<Company> item)
        {
            item.Patch(MonstersInc);
            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        #region Navigation link
        [HttpPost]
        [ODataRoute("Partners/$ref")]
        public ITestActionResult AddOrUpdateNavigationLink([FromBody] Uri link)
        {
            int relatedKey = GetRequestValue<int>(link);
            Partner partner = PartnersController.Partners.First(x => x.ID == relatedKey);
            MonstersInc.Partners.Add(partner);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [ODataRoute("Partners({relatedKey})/$ref")]
        public ITestActionResult DeleteNavigationLink(string relatedKey)
        {
            int key = int.Parse(relatedKey);
            Partner partner = MonstersInc.Partners.First(x => x.ID == key);

            MonstersInc.Partners.Remove(partner);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("GET")]
        [ODataRoute("Partners/$ref")]
        public ITestActionResult GetNavigationLink()
        {
            return Ok();
        }

        [HttpPost]
        [ODataRoute("Partners")]
        public ITestActionResult AddPartnersToCompany([FromBody] Partner partner)
        {
            PartnersController.Partners.Add(partner);
            if (MonstersInc.Partners == null)
            {
                MonstersInc.Partners = new List<Partner>() { partner };
            }
            else
            {
                MonstersInc.Partners.Add(partner);
            }

            return Created(partner);
        }
        #endregion

        #region Action and function
        [HttpPost]
        [ODataRoute("Microsoft.Test.E2E.AspNet.OData.Singleton.ResetDataSource")]
        public ITestActionResult CallActionResetDataSource()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ODataRoute("Microsoft.Test.E2E.AspNet.OData.Singleton.GetPartnersCount()")]
        public ITestActionResult CallFunctionGetPartnersCount()
        {
            return Ok(MonstersInc.Partners.Count);
        }
        #endregion
    }
}
