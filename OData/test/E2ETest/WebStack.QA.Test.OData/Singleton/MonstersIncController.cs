using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using WebStack.QA.Test.OData.Common;

namespace WebStack.QA.Test.OData.Singleton
{
    /// <summary>
    /// Present a singleton named "MonstersInc"
    /// Use attribute routing
    /// </summary>
    [ODataRoutePrefix("MonstersInc")]
    public class MonstersIncController : ODataController
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
        public IHttpActionResult QueryCompany()
        {
            return Ok(MonstersInc);
        }

        [HttpGet]
        [ODataRoute("WebStack.QA.Test.OData.Singleton.SubCompany")]
        public IHttpActionResult QueryCompanyFromDerivedType()
        {
            var subCompany = MonstersInc as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany);
            }
            return BadRequest("The target cannot be casted");
        }

        [ODataRoute("Revenue")]
        public IHttpActionResult GetCompanyRevenue()
        {
            return Ok(MonstersInc.Revenue);
        }

        [HttpGet]
        [ODataRoute("Branches/$count")]
        public IHttpActionResult GetBranchesCount(ODataQueryOptions<Office> options)
        {
            IQueryable<Office> eligibleBranches = MonstersInc.Branches.AsQueryable();
            if (options.Filter != null)
            {
                eligibleBranches = options.Filter.ApplyTo(eligibleBranches, new ODataQuerySettings()).Cast<Office>();
            }
            return Ok(eligibleBranches.Count());
        }

        [ODataRoute("WebStack.QA.Test.OData.Singleton.SubCompany/Location")]
        public IHttpActionResult GetDerivedTypeProperty()
        {
            var subCompany = MonstersInc as SubCompany;
            if (subCompany != null)
            {
                return Ok(subCompany.Location);
            }
            return BadRequest("The target cannot be casted");
        }

        [HttpGet]
        [ODataRoute("WebStack.QA.Test.OData.Singleton.SubCompany/Office")]
        public IHttpActionResult QueryDerivedTypeComplexProperty()
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
        public IHttpActionResult QueryNavigationProperty()
        {
            return Ok(MonstersInc.Partners);
        }
        #endregion

        #region Update
        [HttpPut]
        [ODataRoute]
        public IHttpActionResult UpdateCompanyByPut(Company newCompany)
        {
            MonstersInc = newCompany;
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPut]
        [ODataRoute("WebStack.QA.Test.OData.Singleton.SubCompany")]
        public IHttpActionResult UpdateCompanyByPutWithDerivedTypeObject(SubCompany newCompany)
        {
            MonstersInc = newCompany;
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPatch]
        [ODataRoute]
        public IHttpActionResult UpdateCompanyByPatch(Delta<Company> item)
        {
            item.Patch(MonstersInc);
            return StatusCode(HttpStatusCode.NoContent);
        }
        #endregion

        #region Navigation link
        [HttpPost]
        [ODataRoute("Partners/$ref")]
        public IHttpActionResult AddOrUpdateNavigationLink([FromBody] Uri link)
        {
            int relatedKey = Request.GetKeyValue<int>(link);
            Partner partner = PartnersController.Partners.First(x => x.ID == relatedKey);
            MonstersInc.Partners.Add(partner);

            return StatusCode(HttpStatusCode.NoContent);
        }

        [ODataRoute("Partners({relatedKey})/$ref")]
        public IHttpActionResult DeleteNavigationLink(string relatedKey)
        {
            int key = int.Parse(relatedKey);
            Partner partner = MonstersInc.Partners.First(x => x.ID == key);

            MonstersInc.Partners.Remove(partner);
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("GET")]
        [ODataRoute("Partners/$ref")]
        public IHttpActionResult GetNavigationLink()
        {
            return Ok();
        }

        [HttpPost]
        [ODataRoute("Partners")]
        public IHttpActionResult AddPartnersToCompany([FromBody] Partner partner)
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
        [ODataRoute("WebStack.QA.Test.OData.Singleton.ResetDataSource")]
        public IHttpActionResult CallActionResetDataSource()
        {
            InitData();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        [ODataRoute("WebStack.QA.Test.OData.Singleton.GetPartnersCount()")]
        public IHttpActionResult CallFunctionGetPartnersCount()
        {
            return Ok(MonstersInc.Partners.Count);
        }
        #endregion
    }
}
