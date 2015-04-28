using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;

namespace WebStack.QA.Test.OData.StoreGeneratedPattern
{
    public class StoreGeneratedPatternCustomersController : ODataController
    {
        private const string ComputedPropertyValue = "ComputedProperty";

        private static IList<StoreGeneratedPatternCustomer> customers = Enumerable.Range(0, 10).Select(i =>
            new StoreGeneratedPatternCustomer
            {
                Id = i,
                Name = "Customer Name " + i,
                ComputedProperty = ComputedPropertyValue,
            }).ToList();

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get()
        {
            return Ok(customers);
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get(int key, ODataQueryOptions<StoreGeneratedPatternCustomer> queryOptions)
        {
            IEnumerable<StoreGeneratedPatternCustomer> appliedCustomers = customers.Where(c => c.Id == key);

            if (appliedCustomers.Count() == 0)
            {
                return BadRequest("The key is not valid");
            }
            else
            {
                return Ok(new SingleResult<StoreGeneratedPatternCustomer>(appliedCustomers.AsQueryable()));
            }
        }

        public IHttpActionResult Post(StoreGeneratedPatternCustomer customer)
        {
            customer.Id = customers.Max(c => c.Id) + 1;
            customer.ComputedProperty = ComputedPropertyValue;

            customers.Add(customer);
            return Created(customer);
        }

        public IHttpActionResult Put(int key, StoreGeneratedPatternCustomer customer)
        {
            var originalCustomer = customers.Single(c => c.Id == key);
            customer.Id = originalCustomer.Id;
            customer.ComputedProperty = ComputedPropertyValue;

            customers.Remove(originalCustomer);
            customers.Add(customer);
            return Ok(customer);
        }
    }
}
