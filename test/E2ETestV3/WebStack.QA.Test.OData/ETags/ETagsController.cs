using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;

namespace WebStack.QA.Test.OData.ETags
{
    public class ETagsCustomersController : ODataController
    {
        private static IList<ETagsCustomer> customers = Enumerable.Range(0, 10).Select(i =>
                new ETagsCustomer
                {
                    Id = i,
                    Name = "Customer Name " + i,
                    Notes = Enumerable.Range(0, i + 1).Select(j => "This is note " + (i * 10 + j)).ToList()
                }).ToList();

        public static void ResetCustomers()
        {
            customers = Enumerable.Range(0, 10).Select(i =>
                new ETagsCustomer
                {
                    Id = i,
                    Name = "Customer Name " + i,
                    Notes = Enumerable.Range(0, i + 1).Select(j => "This is note " + (i * 10 + j)).ToList()
                }).ToList();
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get()
        {
            return Ok(customers);
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get(int key, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == key);

            if (appliedCustomers.Count() == 0)
            {
                return BadRequest("The key is not valid");
            }

            if (queryOptions.IfNoneMatch != null)
            {
                appliedCustomers = queryOptions.IfNoneMatch.ApplyTo(appliedCustomers.AsQueryable()).Cast<ETagsCustomer>();
            }

            if (queryOptions.IfMatch != null)
            {
                appliedCustomers = queryOptions.IfMatch.ApplyTo(appliedCustomers.AsQueryable()).Cast<ETagsCustomer>();
            }

            if (appliedCustomers.Count() == 0)
            {
                return StatusCode(HttpStatusCode.NotModified);
            }
            else
            {
                return Ok(new SingleResult<ETagsCustomer>(appliedCustomers.AsQueryable()));
            }
        }

        public IHttpActionResult Put(int key, ETagsCustomer eTagsCustomer, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            if (key != eTagsCustomer.Id)
            {
                return BadRequest("The Id of customer is not matched with the key");
            }

            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == eTagsCustomer.Id);

            if (appliedCustomers.Count() == 0)
            {
                customers.Add(eTagsCustomer);
                return Ok(eTagsCustomer);
            }

            if (queryOptions.IfMatch != null)
            {
                IQueryable<ETagsCustomer> ifMatchCustomers = queryOptions.IfMatch.ApplyTo(appliedCustomers.AsQueryable()).Cast<ETagsCustomer>();

                if (ifMatchCustomers.Count() == 0)
                {
                    return StatusCode(HttpStatusCode.PreconditionFailed);
                }
            }

            ETagsCustomer customer = appliedCustomers.Single();
            customer.Name = eTagsCustomer.Name;
            customer.Notes = eTagsCustomer.Notes;
            customer.BoolProperty = eTagsCustomer.BoolProperty;
            customer.ByteProperty = eTagsCustomer.ByteProperty;
            customer.CharProperty = eTagsCustomer.CharProperty;
            customer.DecimalProperty = eTagsCustomer.DecimalProperty;
            customer.DoubleProperty = eTagsCustomer.DoubleProperty;
            customer.ShortProperty = eTagsCustomer.ShortProperty;
            customer.LongProperty = eTagsCustomer.LongProperty;
            customer.SbyteProperty = eTagsCustomer.SbyteProperty;
            customer.FloatProperty = eTagsCustomer.FloatProperty;
            customer.UshortProperty = eTagsCustomer.UshortProperty;
            customer.UintProperty = eTagsCustomer.UintProperty;
            customer.UlongProperty = eTagsCustomer.UlongProperty;
            customer.GuidProperty = eTagsCustomer.GuidProperty;
            customer.DateTimeOffsetProperty = eTagsCustomer.DateTimeOffsetProperty;

            return Ok(customer);
        }

        public IHttpActionResult Delete(int key, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == key);

            if (appliedCustomers.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            if (queryOptions.IfMatch != null)
            {
                IQueryable<ETagsCustomer> ifMatchCustomers = queryOptions.IfMatch.ApplyTo(appliedCustomers.AsQueryable()).Cast<ETagsCustomer>();

                if (ifMatchCustomers.Count() == 0)
                {
                    return StatusCode(HttpStatusCode.PreconditionFailed);
                }
            }

            ETagsCustomer customer = appliedCustomers.Single();
            customers.Remove(customer);
            return Ok(customer);
        }

        public IHttpActionResult Patch(int key, Delta<ETagsCustomer> patch, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == key);

            if (appliedCustomers.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            if (queryOptions.IfMatch != null)
            {
                IQueryable<ETagsCustomer> ifMatchCustomers = queryOptions.IfMatch.ApplyTo(appliedCustomers.AsQueryable()).Cast<ETagsCustomer>();

                if (ifMatchCustomers.Count() == 0)
                {
                    return StatusCode(HttpStatusCode.PreconditionFailed);
                }
            }

            ETagsCustomer customer = appliedCustomers.Single();
            patch.Patch(customer);

            return Ok(customer);
        }
    }
}
