// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query
{
    public class QueryCompositionCustomerController : ApiController
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
                    }
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
        [Queryable]
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return CustomerList.AsQueryable();
        }
    }

    [Queryable]
    public class QueryCompositionCustomerQueryableController : ApiController
    {
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return QueryCompositionCustomerController.CustomerList.AsQueryable();
        }
    }

    public class QueryCompositionCustomerGlobalController : ApiController
    {
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return QueryCompositionCustomerController.CustomerList.AsQueryable();
        }
    }

    public class QueryCompositionCustomerLowLevelController : ApiController
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

    public class QueryCompositionCustomerLowLevelWithoutDefaultOrderController : ApiController
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
                result = queryOptions.ApplyTo(QueryCompositionCustomerController.CustomerList.AsQueryable(), handleNullPropagation: true, canUseDefaultOrderBy: false) as IQueryable<QueryCompositionCustomer>;
            }
            catch (ODataException exception)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, exception));
            }

            return result;
        }
    }

    public class QueryCompositionCategoryController : ApiController
    {
        [Queryable]
        public IQueryable<QueryCompositionCategory> Get()
        {
            return Enumerable.Empty<QueryCompositionCategory>().AsQueryable();
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
    }

    public class QueryCompositionAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
    }

    public class QueryCompositionCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QueryCompositionAddress[] Locations { get; set; }
        public int[] AlternateIds { get; set; }
    }
}
