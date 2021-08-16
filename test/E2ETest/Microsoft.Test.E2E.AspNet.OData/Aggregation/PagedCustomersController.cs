//-----------------------------------------------------------------------------
// <copyright file="PagedCustomersController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNet.OData;

namespace Microsoft.Test.E2E.AspNet.OData.Aggregation.Paged
{
    public class CustomersController : BaseCustomersController
    {
        [EnableQuery(PageSize = 5)]
        public IQueryable<Customer> Get()
        {
            ResetDataSource();
            var db = new AggregationContext();
            return db.Customers;
        }
    }
}
