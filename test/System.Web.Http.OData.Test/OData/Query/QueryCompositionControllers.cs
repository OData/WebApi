// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.OData.Query.Validators;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query.SemanticAst;

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

    public class QueryCompositionCustomerWithTaskOfIEnumerableController : ApiController
    {
        [Queryable]
        public Task<IEnumerable<QueryCompositionCustomer>> Get()
        {
            return TaskHelpers.FromResult(QueryCompositionCustomerController.CustomerList as IEnumerable<QueryCompositionCustomer>);
        }
    }

    public class QueryCompositionCustomerWithTaskOfHttpResponseMessageController : ApiController
    {
        [Queryable]
        public Task<HttpResponseMessage> Get()
        {
            return TaskHelpers.FromResult(
                Request.CreateResponse(HttpStatusCode.OK, QueryCompositionCustomerController.CustomerList.AsEnumerable()));
        }
    }

    public class QueryCompositionCustomerGlobalController : ApiController
    {
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return QueryCompositionCustomerController.CustomerList.AsQueryable();
        }
    }

    public class QueryCompositionCustomerValidationController : ApiController
    {
        [Queryable(MaxSkip = 1, MaxTop = 2, AllowedArithmeticOperators = AllowedArithmeticOperators.Modulo, AllowedFunctionNames = AllowedFunctionNames.Length,
            AllowedLogicalOperators = AllowedLogicalOperators.Equal, AllowedOrderByProperties = "Id,Name")]
        public IQueryable<QueryCompositionCustomer> Get()
        {
            return QueryCompositionCustomerController.CustomerList.AsQueryable();
        }

        // low level api
        [MyQueryable]
        public IQueryable<QueryCompositionCustomer> Get(int id)
        {
            return QueryCompositionCustomerController.CustomerList.AsQueryable();
        }
    }

    public class MyQueryableAttribute : QueryableAttribute
    {
        public override void ValidateQuery(HttpRequestMessage request, ODataQueryOptions queryOptions)
        {
            if (queryOptions.Filter != null)
            {
                queryOptions.Filter.Validator = new MyFilterQueryValidator();
            }

            if (queryOptions.OrderBy != null)
            {
                queryOptions.OrderBy.Validator = new MyOrderByQueryValidator();
            }

            base.ValidateQuery(request, queryOptions);
        }
    }

    public class MyFilterQueryValidator : FilterQueryValidator
    {
        public override void ValidateConstantNode(ConstantNode constantNode, ODataValidationSettings settings)
        {
            // Validate that client did not send a big constant in the query
            if (Convert.ToInt32(constantNode.Value) > 100)
            {
                throw new ODataException("Any constant that is more than 100 is not allowed.");
            }

            base.ValidateConstantNode(constantNode, settings);
        }
    }

    public class MyOrderByQueryValidator : OrderByQueryValidator
    {
        public override void Validate(OrderByQueryOption option, ODataValidationSettings validationSettings)
        {
            // validate the orderby is executed in a way that one can order either by Id or by Name, but not both
            if (option.PropertyNodes.Count > 1 )
            {
                throw new ODataException("Order by more than one property is not allowed.");
            }

            base.Validate(option, validationSettings);
        }
    }

    public class QueryCompositionCategoryValidationController : ApiController
    {
        [Queryable(AllowedQueryOptions = AllowedQueryOptions.OrderBy | AllowedQueryOptions.Filter)]
        public IQueryable<QueryCompositionCategory> Get()
        {
            return Enumerable.Empty<QueryCompositionCategory>().AsQueryable();
        }

        [Queryable(AllowedOrderByProperties="Id")]
        public IQueryable<QueryCompositionCategory> Get(int id)
        {
            return Enumerable.Empty<QueryCompositionCategory>().AsQueryable();
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

    public class QueryCompositionCustomerLowLevel_ODataQueryOptionsOfTController : ApiController
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
                ODataQuerySettings querySettings = new ODataQuerySettings
                {
                    EnsureStableOrdering = false
                };
                result = queryOptions.ApplyTo(QueryCompositionCustomerController.CustomerList.AsQueryable(), querySettings) as IQueryable<QueryCompositionCustomer>;
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

    public class QueryCompositionAnonymousTypesController : ApiController
    {
        [Queryable]
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
