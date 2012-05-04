// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Properties;
using System.Web.Http.Query;

namespace System.Web.Http
{
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want to be able to subclass this type")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class QueryableAttribute : ActionFilterAttribute
    {
        private readonly QueryValidator _queryValidator;
        private IStructuredQueryBuilder _structuredQueryBuilder;

        private static readonly IStructuredQueryBuilder _defaultStructuredQueryBuilder = new DefaultStructuredQueryBuilder();

        public QueryableAttribute()
        {
            _queryValidator = QueryValidator.Instance;
        }

        /// <summary>
        /// The maximum number of results that should be returned from this query regardless of query-specified limits. A value of <c>0</c>
        /// indicates no limit. Negative values are not supported and will cause a runtime exception.
        /// </summary>
        public int ResultLimit { get; set; }

        /// <summary>
        /// The <see cref="IStructuredQueryBuilder"/> to use. Derived classes can use this to have a per-attribute query builder 
        /// instead of the one on <see cref="HttpConfiguration"/>
        /// </summary>
        protected IStructuredQueryBuilder StructuredQueryBuilder
        {
            get
            {
                return _structuredQueryBuilder;
            }

            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }
                _structuredQueryBuilder = value;
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            if (ResultLimit < 0)
            {
                throw Error.InvalidOperation(SRResources.QueryableAttribute_InvalidResultLimit,
                    actionContext.ActionDescriptor.ActionName, actionContext.ControllerContext.ControllerDescriptor.ControllerName);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            Contract.Assert(actionExecutedContext.Request != null);

            HttpRequestMessage request = actionExecutedContext.Request;
            HttpResponseMessage response = actionExecutedContext.Response;

            IQueryable query;
            if (response != null && response.TryGetContentValue(out query))
            {
                IQueryable deserializedQuery = null;
                if (request != null && request.RequestUri != null && !String.IsNullOrWhiteSpace(request.RequestUri.Query))
                {
                    Uri requestUri = request.RequestUri;
                    try
                    {
                        StructuredQuery structuredQuery = GetStructuredQuery(request);

                        if (structuredQuery != null && structuredQuery.QueryParts.Any())
                        {
                            IQueryable baseQuery = Array.CreateInstance(query.ElementType, 0).AsQueryable(); // T[]
                            deserializedQuery = ODataQueryDeserializer.Deserialize(baseQuery, structuredQuery.QueryParts);
                            if (_queryValidator != null && deserializedQuery != null)
                            {
                                _queryValidator.Validate(deserializedQuery);
                            }
                        }
                    }
                    catch (ParseException e)
                    {
                        actionExecutedContext.Response = request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            new HttpError(e) { Message = Error.Format(SRResources.UriQueryStringInvalid, e.Message) });
                        return;
                    }
                }

                if (deserializedQuery != null)
                {
                    query = QueryComposer.Compose(query, deserializedQuery);
                }

                query = ApplyResultLimit(actionExecutedContext, query);

                ((ObjectContent)response.Content).Value = query;
            }
        }

        protected virtual IQueryable ApplyResultLimit(HttpActionExecutedContext actionExecutedContext, IQueryable query)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (ResultLimit > 0)
            {
                query = query.Take(ResultLimit);
            }
            return query;
        }

        private StructuredQuery GetStructuredQuery(HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            IStructuredQueryBuilder queryBuilder = null;
            if (StructuredQueryBuilder != null)
            {
                queryBuilder = StructuredQueryBuilder;
            }
            else
            {
                HttpConfiguration configuration = request.GetConfiguration();
                if (configuration != null)
                {
                    queryBuilder = configuration.Services.GetStructuredQueryBuilder();
                }
            }

            queryBuilder = queryBuilder ?? _defaultStructuredQueryBuilder;
            return queryBuilder.GetStructuredQuery(request.RequestUri);
        }
    }
}
