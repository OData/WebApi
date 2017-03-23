using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;

namespace WebStack.QA.Test.OData.ETags
{
    public class ETagsDerivedCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get()
        {
            return Ok(ETagsCustomersController.customers.Select(c => Helpers.CreateDerivedCustomer(c)));
        }
    }

    public class ETagsDerivedCustomersSingletonController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public IHttpActionResult Get()
        {
            return Ok(ETagsCustomersController.customers.Select(c => Helpers.CreateDerivedCustomer(c)).FirstOrDefault());
        }
    }

    public class ETagsCustomersController : ODataController
    {
        internal static IList<ETagsCustomer> customers = Enumerable.Range(0, 10).Select(i => Helpers.CreateCustomer(i)).ToList();

        [EnableQuery]
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

            return ApplyPut(eTagsCustomer, appliedCustomers.Single(), queryOptions);
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

            return ApplyPatch(patch, appliedCustomers.Single(), queryOptions);
        }

        public IHttpActionResult GetContainedCustomer(int key)
        {
            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == key);

            if (appliedCustomers.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            return Ok(appliedCustomers.Single().ContainedCustomer);
        }

        public IHttpActionResult GetRelatedCustomer(int key)
        {
            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == key);

            if (appliedCustomers.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            return Ok(appliedCustomers.Single().RelatedCustomer);
        }

        [HttpPut]
        [ODataRoute("ETagsCustomers({key})/RelatedCustomer")]
        public IHttpActionResult PutRelatedCustomer(int key, ETagsCustomer eTagsCustomer, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            if (key != eTagsCustomer.Id)
            {
                return BadRequest("The Id of customer is not matched with the key");
            }

            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == eTagsCustomer.Id);

            return ApplyPut(eTagsCustomer, appliedCustomers.Single().RelatedCustomer, queryOptions);
        }

        [HttpPatch]
        [ODataRoute("ETagsCustomers({key})/RelatedCustomer")]
        public IHttpActionResult PatchRelatedCustomer(int key, Delta<ETagsCustomer> patch, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == key);

            if (appliedCustomers.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            return ApplyPatch(patch, appliedCustomers.Single().RelatedCustomer, queryOptions);
        }

        [HttpPut]
        [ODataRoute("ETagsCustomers({key})/ContainedCustomer")]
        public IHttpActionResult PutContainedCustomer(int key, ETagsCustomer eTagsCustomer, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            if (key != eTagsCustomer.Id)
            {
                return BadRequest("The Id of customer is not matched with the key");
            }

            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == eTagsCustomer.Id);

            return ApplyPut(eTagsCustomer, appliedCustomers.Single().ContainedCustomer, queryOptions);
        }

        [HttpPatch]
        [ODataRoute("ETagsCustomers({key})/ContainedCustomer")]
        public IHttpActionResult PatchContainedCustomer(int key, Delta<ETagsCustomer> patch, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            IEnumerable<ETagsCustomer> appliedCustomers = customers.Where(c => c.Id == key);

            if (appliedCustomers.Count() == 0)
            {
                return BadRequest(string.Format("The entry with Id {0} doesn't exist", key));
            }

            return ApplyPatch(patch, appliedCustomers.Single().ContainedCustomer, queryOptions);
        }

        internal IHttpActionResult ApplyPatch(Delta<ETagsCustomer> patch, ETagsCustomer original, ODataQueryOptions queryOptions)
        {
            if (!Helpers.ValidateEtag(original, queryOptions))
            {
                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            patch.Patch(original);

            return Ok(original);
        }

        public IHttpActionResult ApplyPut(ETagsCustomer eTagsCustomer, ETagsCustomer original, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            if (!Helpers.ValidateEtag(original, queryOptions))
            {
                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            Helpers.ReplaceCustomer(original, eTagsCustomer);

            return Ok(original);
        }
    }

    public class ETagsCustomerController : ODataController
    {
        private static ETagsCustomer customer = Helpers.CreateCustomer(0);

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(customer);
        }

        public IHttpActionResult GetRelatedCustomer()
        {
            return Ok(customer.RelatedCustomer);
        }

        public IHttpActionResult GetContainedCustomer()
        {
            return Ok(customer.ContainedCustomer);
        }

        public IHttpActionResult Put(ETagsCustomer eTagsCustomer, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            return ApplyPut(eTagsCustomer, customer, queryOptions);
        }

        public IHttpActionResult Patch(Delta<ETagsCustomer> patch, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            return ApplyPatch(patch, customer, queryOptions);
        }

        [HttpPut]
        [ODataRoute("ETagsCustomer/RelatedCustomer")]
        public IHttpActionResult PutRelatedCustomer(ETagsCustomer eTagsCustomer, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            return ApplyPut(eTagsCustomer, customer.RelatedCustomer, queryOptions);
        }

        [HttpPatch]
        [ODataRoute("ETagsCustomer/RelatedCustomer")]
        public IHttpActionResult PatchRelatedCustomer(Delta<ETagsCustomer> patch, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            return ApplyPatch(patch, customer.RelatedCustomer, queryOptions);
        }

        [HttpPut]
        [ODataRoute("ETagsCustomer/ContainedCustomer")]
        public IHttpActionResult PutContainedCustomer(ETagsCustomer eTagsCustomer, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            return ApplyPut(eTagsCustomer, customer.ContainedCustomer, queryOptions);
        }

        [HttpPatch]
        [ODataRoute("ETagsCustomer/ContainedCustomer")]
        public IHttpActionResult PatchContainedCustomer(Delta<ETagsCustomer> patch, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            return ApplyPatch(patch, customer.ContainedCustomer, queryOptions);
        }

        internal IHttpActionResult ApplyPatch(Delta<ETagsCustomer> patch, ETagsCustomer original, ODataQueryOptions queryOptions)
        {
            if (!Helpers.ValidateEtag(original, queryOptions))
            {
                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            patch.Patch(original);

            return Ok(original);
        }

        public IHttpActionResult ApplyPut(ETagsCustomer eTagsCustomer, ETagsCustomer original, ODataQueryOptions<ETagsCustomer> queryOptions)
        {
            if (!Helpers.ValidateEtag(original, queryOptions))
            {
                return StatusCode(HttpStatusCode.PreconditionFailed);
            }

            Helpers.ReplaceCustomer(original, eTagsCustomer);

            return Ok(original);
        }
    }

    internal class Helpers
    {
        internal static ETagsCustomer CreateCustomer(int i)
        {
            return new ETagsDerivedCustomer
            {
                Id = i,
                Name = "Customer Name " + i,
                ShortProperty = (short)(Int16.MaxValue - i),
                DoubleProperty = 2.0 * (i + 1),
                Notes = Enumerable.Range(0, i + 1).Select(j => "This is note " + (i * 10 + j)).ToList(),
                RelatedCustomer = new ETagsCustomer
                {
                    Id = i + 1,
                    Name = "Customer Name " + i + 1,
                    ShortProperty = (short)(Int16.MaxValue - (i + 1) * 10),
                    DoubleProperty = 2.0 * (i + 1) * 10,
                    Notes = Enumerable.Range(0, (i + 1) * 10).Select(j => "This is note " + ((i + 1) * 10 + j)).ToList()
                },
                ContainedCustomer = new ETagsCustomer
                {
                    Id = (i + 1) * 100,
                    Name = "Customer Name " + i * 10,
                    ShortProperty = (short)(Int16.MaxValue - i * 10),
                    DoubleProperty = 2.0 * (i * 10 + 1),
                    Notes = Enumerable.Range(0, i * 10 + 1).Select(j => "This is note " + (i * 100 + j)).ToList()
                }
            };
        }

        internal static bool ValidateEtag(ETagsCustomer customer, ODataQueryOptions options)
        {
            if (options.IfMatch != null)
            {
                IQueryable<ETagsCustomer> ifMatchCustomers = options.IfMatch.ApplyTo((new ETagsCustomer[] { customer }).AsQueryable()).Cast<ETagsCustomer>();

                if (ifMatchCustomers.Count() == 0)
                {
                    return false;
                }
            }
            return true;
        }

        internal static ETagsDerivedCustomer CreateDerivedCustomer(ETagsCustomer customer)
        {
            ETagsDerivedCustomer newCustomer = new ETagsDerivedCustomer();
            newCustomer.Id = customer.Id;
            newCustomer.Role = customer.Name + customer.Id;
            ReplaceCustomer(newCustomer, customer);
            return newCustomer;
        }

        internal static void ReplaceCustomer(ETagsCustomer newCustomer, ETagsCustomer customer)
        {
            newCustomer.Name = customer.Name;
            newCustomer.Notes = customer.Notes;
            newCustomer.BoolProperty = customer.BoolProperty;
            newCustomer.ByteProperty = customer.ByteProperty;
            newCustomer.CharProperty = customer.CharProperty;
            newCustomer.DecimalProperty = customer.DecimalProperty;
            newCustomer.DoubleProperty = customer.DoubleProperty;
            newCustomer.ShortProperty = customer.ShortProperty;
            newCustomer.LongProperty = customer.LongProperty;
            newCustomer.SbyteProperty = customer.SbyteProperty;
            newCustomer.FloatProperty = customer.FloatProperty;
            newCustomer.UshortProperty = customer.UshortProperty;
            newCustomer.UintProperty = customer.UintProperty;
            newCustomer.UlongProperty = customer.UlongProperty;
            newCustomer.GuidProperty = customer.GuidProperty;
            newCustomer.DateTimeOffsetProperty = customer.DateTimeOffsetProperty;
        }
    }
}
