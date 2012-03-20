using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using System.Web.Http.Query;

namespace System.Web.Http.Filters
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    internal sealed class QueryCompositionFilterAttribute : ActionFilterAttribute
    {
        [SuppressMessage("Microsoft.Performance", "CA1802: Use Literals Where Appropriate", Justification = "to be consistent with usages elsewhere")]
        private static readonly string QueryKey = "MS_QueryKey";

        private readonly IQueryable _baseQuery;

        public QueryCompositionFilterAttribute(Type queryElementType, QueryValidator queryValidator)
        {
            if (queryElementType == null)
            {
                throw Error.ArgumentNull("queryElementType");
            }

            QueryValidator = queryValidator;
            QueryElementType = queryElementType;
            _baseQuery = Array.CreateInstance(queryElementType, 0).AsQueryable(); // T[]
        }

        private QueryValidator QueryValidator { get; set; }

        public Type QueryElementType { get; private set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }

            HttpRequestMessage request = actionContext.ControllerContext.Request;
            if (request != null && request.RequestUri != null && !String.IsNullOrWhiteSpace(request.RequestUri.Query))
            {
                Uri requestUri = request.RequestUri;
                try
                {
                    ServiceQuery serviceQuery = ODataQueryDeserializer.GetServiceQuery(requestUri);

                    if (serviceQuery.QueryParts.Count() > 0)
                    {
                        IQueryable deserializedQuery = ODataQueryDeserializer.Deserialize(_baseQuery, serviceQuery.QueryParts);
                        if (QueryValidator != null)
                        {
                            QueryValidator.Validate(deserializedQuery);
                        }

                        request.Properties.Add(QueryCompositionFilterAttribute.QueryKey, deserializedQuery);
                    }
                }
                catch (ParseException e)
                {
                    actionContext.Response = request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.UriQueryStringInvalid, e.Message));
                    return;
                }
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
            HttpResponseMessage response = actionExecutedContext.Result;

            IQueryable query;
            if (request == null || !request.Properties.TryGetValue(QueryCompositionFilterAttribute.QueryKey, out query))
            {
                return; // No Query to compose, return
            }

            IQueryable source;
            if (response != null && response.TryGetContentValue(out source))
            {
                IQueryable composedQuery = QueryComposer.Compose(source, query);
                ((ObjectContent)response.Content).Value = composedQuery;
            }
        }
    }
}
