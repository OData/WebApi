//-----------------------------------------------------------------------------
// <copyright file="ActionResultController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Test.E2E.AspNet.OData.ActionResult
{
    public class CustomersController : Controller
    {
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Expand)]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await Task.FromResult(new List<Customer>
            { 
                new Customer
                {
                    Id = "CustId",
                    Books = new List<Book>
                    {
                        new Book
                        {
                            Id = "BookId",
                        },
                    },
                },
            });
        }
    }
}
