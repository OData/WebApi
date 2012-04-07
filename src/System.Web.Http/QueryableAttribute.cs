// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
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

        public QueryableAttribute()
        {
            _queryValidator = QueryValidator.Instance;
        }

        /// <summary>
        /// The maximum number of results that should be returned from this query regardless of query-specified limits. A value of <c>0</c>
        /// indicates no limit. Negative values are not supported and will cause a runtime exception.
        /// </summary>
        public int ResultLimit { get; set; }

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
                        ServiceQuery serviceQuery = ODataQueryDeserializer.GetServiceQuery(requestUri);

                        if (serviceQuery.QueryParts.Count > 0)
                        {
                            IQueryable baseQuery = Array.CreateInstance(query.ElementType, 0).AsQueryable(); // T[]
                            deserializedQuery = ODataQueryDeserializer.Deserialize(baseQuery, serviceQuery.QueryParts);
                            if (_queryValidator != null)
                            {
                                _queryValidator.Validate(deserializedQuery);
                            }
                        }
                    }
                    catch (ParseException e)
                    {
                        actionExecutedContext.Response = request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            Error.Format(SRResources.UriQueryStringInvalid, e.Message));
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
    }
}
