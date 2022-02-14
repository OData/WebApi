//-----------------------------------------------------------------------------
// <copyright file="IsOfController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition.IsOf
{
    public class BillingCustomersController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            if (GetRoutePrefix() == "EF")
            {
                return Ok(IsofDataSource.EfCustomers);
            }
            else
            {
                return Ok(IsofDataSource.InMemoryCustomers);
            }
        }
    }

    public class BillingsController : TestODataController
    {
        [EnableQuery]
        public ITestActionResult Get()
        {
            if (GetRoutePrefix() == "EF")
            {
                return Ok(IsofDataSource.EfBillings);
            }
            else
            {
                return Ok(IsofDataSource.InMemoryBillings);
            }
        }
    }
}
