//-----------------------------------------------------------------------------
// <copyright file="TopSkipOrderByTestsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Products;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.Controllers
{
    public class TopSkipOrderByTestsController : TestNonODataController
    {
        [EnableQuery(PageSize = 999999)]
        public IEnumerable<Customer> GetByQuerableAttribute()
        {
            return new Customer[] 
            {
                new Customer() { Id = 1, Name = "Tom" },
                new Customer() { Id = 2, Name = "Jerry" },
                new Customer() { Id = 2, Name = "Mike" }
            };
        }

        public IEnumerable<Customer> GetByODataQueryOptions(ODataQueryOptions options)
        {
            var customers = this.GetByQuerableAttribute().AsQueryable();

            return options.ApplyTo(customers) as IQueryable<Customer>;
        }

        [EnableQuery(PageSize = 999999)]
        public ITestActionResult GetHttpResponseByQuerableAttribute()
        {
            return Ok<IEnumerable<Customer>>(GetByQuerableAttribute());
        }

        [EnableQuery(PageSize = 999999)]
        public IEnumerable<Customer> GetCustomerCollection()
        {
            var col = new CustomerCollection();
            col.Add(new Customer() { Id = 1, Name = "Tom" });
            col.Add(new Customer() { Id = 2, Name = "Jerry" });
            return col;
        }

        public IEnumerable<ODataRawQueryOptions> GetODataQueryOptions(ODataQueryOptions options)
        {
            return new ODataRawQueryOptions[] { options.RawValues };
        }
    }
}
