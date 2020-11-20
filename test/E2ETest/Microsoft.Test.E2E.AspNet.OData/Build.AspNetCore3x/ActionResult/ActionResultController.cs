using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Test.E2E.AspNet.OData.ActionResult
{
    public class CustomersController : Controller
    {
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Expand)]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously - Intended for testing.
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously - Intended for testing.
        {
            return new List<Customer>
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
            };
        }
    }
}
