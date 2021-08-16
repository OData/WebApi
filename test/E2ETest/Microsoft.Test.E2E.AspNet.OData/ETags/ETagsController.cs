//-----------------------------------------------------------------------------
// <copyright file="ETagsController.cs" company=".NET Foundation">
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
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.ETags
{
    public class ETagsDerivedCustomersController : TestODataController
    {
        internal static IList<ETagsCustomer> customers = Enumerable.Range(0, 10).Select(i => Helpers.CreateCustomer(i)).ToList();

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get()
        {
            return Ok(customers.Select(c => Helpers.CreateDerivedCustomer(c)));
        }
    }

    public class ETagsDerivedCustomersSingletonController : TestODataController
    {
        internal static IList<ETagsCustomer> customers = Enumerable.Range(0, 10).Select(i => Helpers.CreateCustomer(i)).ToList();

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get()
        {
            return Ok(customers.Select(c => Helpers.CreateDerivedCustomer(c)).FirstOrDefault());
        }
    }

    public class ETagsCustomersController : TestODataController
    {
        internal static IList<ETagsCustomer> customers = Enumerable.Range(0, 10).Select(i => Helpers.CreateCustomer(i)).ToList();

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(customers);
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 5)]
        public ITestActionResult Get(int key, ODataQueryOptions<ETagsCustomer> queryOptions)
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
                return Ok(new TestSingleResult<ETagsCustomer>(appliedCustomers.AsQueryable()));
            }
        }

        public ITestActionResult Put(int key, [FromBody]ETagsCustomer eTagsCustomer, ODataQueryOptions<ETagsCustomer> queryOptions)
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

        public ITestActionResult Delete(int key, ODataQueryOptions<ETagsCustomer> queryOptions)
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

        public ITestActionResult Patch(int key, [FromBody]Delta<ETagsCustomer> patch, ODataQueryOptions<ETagsCustomer> queryOptions)
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
