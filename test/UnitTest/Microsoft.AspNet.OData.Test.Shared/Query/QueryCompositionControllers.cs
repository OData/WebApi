//-----------------------------------------------------------------------------
// <copyright file="QueryCompositionControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Builder.TestModels;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
#endif

namespace Microsoft.AspNet.OData.Test.Query
{
    public class QueryCompositionPrimitiveController : TestControllerBase
    {
        [EnableQuery]
        public IQueryable<int> GET()
        {
            return Enumerable.Range(0, 100).AsQueryable();
        }
    }

    public class QueryCompositionCustomerController : TestControllerBase
    {
        internal static List<QueryCompositionCustomer> CustomerList = new List<QueryCompositionCustomer>
            {  
                new QueryCompositionCustomer 
                { 
                    Id = 11, 
                    Name = "Lowest", 
                    Address = new QueryCompositionAddress { City = "redmond", Zipcode = "98052" }, 
                    Contacts = new QueryCompositionCustomer[]
                    {
                        new QueryCompositionCustomer { Id = 111, Name = "Primary" },
                        new QueryCompositionCustomer { Id = 112, Name = "Secondary" },
                    },
                    Image = new byte[] { 1, 2, 3 }
                }, 
                new QueryCompositionCustomer 
                { 
                    Id = 33, 
                    Name = "Highest", 
                    Address = new QueryCompositionAddress { City = "seattle", Zipcode = "98000" },
                    Contacts = new QueryCompositionCustomer[]
                    {
                        new QueryCompositionCustomer { Id = 331, Name = "Primary" },
                    },
                    Tags = new string[] {"tag33"} 
                }, 
                new QueryCompositionCustomer 
                { 
                    Id = 22, 
                    Name = "Middle",
                    Tags = new string[] {"tag1", "tag22"} 
                }, 
                new QueryCompositionCustomer 
                { 
                    Id = 3, 
                    Name = "NewLow", 
                    Tags = new string[] {"tag1", "tag3"} 
                },
            };

        static QueryCompositionCustomerController()
        {
            CustomerList[1].RelationshipManager = CustomerList[0];
        }

        // Most common: using high level APIs
        [EnableQuery]
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return CustomerList.AsQueryable();
        }
    }

    [EnableQuery]
    public class QueryCompositionCustomerQueryableController : TestControllerBase
    {
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return QueryCompositionCustomerController.CustomerList.AsQueryable();
        }
    }

    public class QueryCompositionCustomerWithTaskOfIEnumerableController : TestControllerBase
    {
        [EnableQuery]
        public Task<IEnumerable<QueryCompositionCustomer>> Get()
        {
            return Task.FromResult(QueryCompositionCustomerController.CustomerList as IEnumerable<QueryCompositionCustomer>);
        }
    }

    public class QueryCompositionCustomerGlobalController : TestControllerBase
    {
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return QueryCompositionCustomerController.CustomerList.AsQueryable();
        }
    }

    public class QueryCompositionCustomerValidationController : TestControllerBase
    {
        [EnableQuery(MaxSkip = 1, MaxTop = 2, AllowedArithmeticOperators = AllowedArithmeticOperators.Modulo, AllowedFunctions = AllowedFunctions.Length,
            AllowedLogicalOperators = AllowedLogicalOperators.Equal, AllowedOrderByProperties = "Id,Name")]
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return QueryCompositionCustomerController.CustomerList.AsQueryable();
        }
    }

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
    public class QueryCompositionCustomerLowLevelController : TestControllerBase
    {
        // demo 2: low level APIs
        public IQueryable<QueryCompositionCustomer> Get(ODataQueryOptions queryOptions)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            IQueryable<QueryCompositionCustomer> result = null;
            try
            {
                result = queryOptions.ApplyTo(QueryCompositionCustomerController.CustomerList.AsQueryable()) as IQueryable<QueryCompositionCustomer>;
            }
            catch (ODataException exception)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception));
            }

            return result;
        }
    }

    public class QueryCompositionCustomerLowLevel_ODataQueryOptionsOfTController : TestControllerBase
    {
        public int GetCount(ODataQueryOptions<QueryCompositionCustomer> queryOptions)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            IQueryable<QueryCompositionCustomer> result = null;
            try
            {
                result = queryOptions.ApplyTo(QueryCompositionCustomerController.CustomerList.AsQueryable()) as IQueryable<QueryCompositionCustomer>;
            }
            catch (ODataException exception)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception));
            }

            return result.Count();
        }
    }
#endif

    public class QueryCompositionCategoryController : TestControllerBase
    {
        [EnableQuery]
        public IQueryable<QueryCompositionCategory> Get()
        {
            return Enumerable.Empty<QueryCompositionCategory>().AsQueryable();
        }
    }

    public class QueryCompositionAnonymousTypesController : TestControllerBase
    {
        [EnableQuery]
        public IQueryable Get()
        {
            return
                Enumerable
                .Range(1, 10)
                .Select(i => new { Id = i })
                .AsQueryable();
        }
    }

    public class QueryCompositionCustomerBase
    {
        public int Id { get; set; }
    }

    public class QueryCompositionCustomer : QueryCompositionCustomerBase
    {
        public string Name { get; set; }
        public QueryCompositionAddress Address { get; set; }
        public QueryCompositionCustomer RelationshipManager { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<QueryCompositionCustomer> Contacts { get; set; }
        public byte[] Image { get; set; }
        public DateTimeOffset Birthday { get; set; }
        public double AmountSpent { get; set; }
        public Color FavoriteColor { get; set; }


        public QueryCompositionAddress NavigationWithNotFilterableProperty { get; set; }
        [NotFilterable]
        public QueryCompositionCustomer NotFilterableNavigationProperty { get; set; }
        [NotFilterable]
        public string NotFilterableProperty { get; set; }
        [NotSortable]
        public string NotSortableProperty { get; set; }
        [NonFilterable]
        public QueryCompositionCustomer NonFilterableNavigationProperty { get; set; }
        [NonFilterable]
        public string NonFilterableProperty { get; set; }
        [Unsortable]
        public string UnsortableProperty { get; set; }
    }

    public class QueryCompositionAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }

        [NotFilterable]
        public string NotFilterableProperty { get; set; }
        [NonFilterable]
        public string NonFilterableProperty { get; set; }
    }

    public class QueryCompositionCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QueryCompositionAddress[] Locations { get; set; }
        public int[] AlternateIds { get; set; }
    }
}
