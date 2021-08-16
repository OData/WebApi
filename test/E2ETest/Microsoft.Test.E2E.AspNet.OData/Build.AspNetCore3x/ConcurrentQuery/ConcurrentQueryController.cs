//-----------------------------------------------------------------------------
// <copyright file="ConcurrentQueryController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Test.E2E.AspNet.OData.ConcurrentQuery
{
    public class CustomersController : Controller
    {
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Count | AllowedQueryOptions.Filter)]
        public IQueryable<Customer> GetCustomers()
        {
            return Enumerable.Range(1, 100)
                .Select(i => new Customer
                {
                    Id = i,
                }).AsQueryable();
        }
    }
}
