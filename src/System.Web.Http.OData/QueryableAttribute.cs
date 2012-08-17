// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http
{
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want to be able to subclass this type.")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class QueryableAttribute : ActionFilterAttribute
    {
        private bool? _handleNullPropagation;

        /// <summary>
        /// Enables a controller action to support OData query parameters.
        /// </summary>
        public QueryableAttribute()
        {
        }

        /// <summary>
        /// Enables a controller action to support OData query parameters.
        /// </summary>
        /// <param name="handleNullPropagation">If this filter should handle null propagation</param>
        public QueryableAttribute(bool handleNullPropagation)
        {
            _handleNullPropagation = handleNullPropagation;
        }

        /// <summary>
        /// Gets whether this filter should handle null propagation.
        /// </summary>
        public bool HandleNullPropagation
        {
            get
            {
                return _handleNullPropagation.Value;
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw Error.ArgumentNull("actionContext");
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            HttpRequestMessage request = actionExecutedContext.Request;
            if (request == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionExecutedContextMustHaveRequest);
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.RequestMustContainConfiguration);
            }

            if (actionExecutedContext.ActionContext == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionExecutedContextMustHaveActionContext);
            }

            HttpActionDescriptor actionDescriptor = actionExecutedContext.ActionContext.ActionDescriptor;
            if (actionDescriptor == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionContextMustHaveDescriptor);
            }

            HttpResponseMessage response = actionExecutedContext.Response;

            IEnumerable query;
            IQueryable queryable = null;
            if (response != null && response.IsSuccessStatusCode && response.TryGetContentValue(out query))
            {
                if (request.RequestUri != null && !String.IsNullOrWhiteSpace(request.RequestUri.Query))
                {
                    ValidateQuery(request);

                    try
                    {
                        ODataQueryContext queryContext;

                        Type originalQueryType = query.GetType();
                        Type entityClrType = TypeHelper.GetImplementedIEnumerableType(originalQueryType);

                        // Primitive types do not construct an EDM model and deal only with the CLR Type
                        if (TypeHelper.IsQueryPrimitiveType(entityClrType))
                        {
                            queryContext = new ODataQueryContext(entityClrType);
                        }
                        else
                        {
                            // Get model for the entire app
                            IEdmModel model = configuration.GetEdmModel();

                            if (entityClrType == null)
                            {
                                // The actual type is not IEnumerable or IQueryable
                                actionExecutedContext.Response = request.CreateErrorResponse(
                                   HttpStatusCode.InternalServerError,
                                   Error.Format(SRResources.FailedToRetrieveTypeToBuildEdmModel, originalQueryType.FullName,
                                       actionDescriptor.ActionName, actionDescriptor.ControllerDescriptor.ControllerName));
                                return;
                            }

                            if (model == null)
                            {
                                // user has not configured anything, now let's create one just for this type
                                // and cache it in the action descriptor
                                model = actionDescriptor.GetEdmModel(entityClrType);
                            }

                            if (model == null)
                            {
                                // we need to send 500 if we can't create a model
                                actionExecutedContext.Response = request.CreateErrorResponse(
                                    HttpStatusCode.InternalServerError,
                                    Error.Format(SRResources.FailedToBuildEdmModel, entityClrType.FullName,
                                        actionDescriptor.ActionName, actionDescriptor.ControllerDescriptor.ControllerName));
                                return;
                            }

                            // parses the query from request uri
                            queryContext = new ODataQueryContext(model, entityClrType);
                        }

                        ODataQueryOptions queryOptions = new ODataQueryOptions(queryContext, request);

                        // Filter and OrderBy require entity sets.  Top and Skip may accept primitives.
                        if (queryContext.IsPrimitiveClrType && (queryOptions.Filter != null || queryOptions.OrderBy != null))
                        {
                            // An attempt to use a query option not allowed for primitive types
                            // generates a BadRequest with a general message that avoids information disclosure.
                            actionExecutedContext.Response = request.CreateErrorResponse(
                                                                HttpStatusCode.BadRequest,
                                                                SRResources.OnlySkipAndTopSupported);
                            return;
                        }

                        // apply the query
                        queryable = query as IQueryable;
                        if (queryable == null)
                        {
                            queryable = query.AsQueryable();
                        }

                        if (_handleNullPropagation != null)
                        {
                            queryable = queryOptions.ApplyTo(queryable, _handleNullPropagation.Value);
                        }
                        else
                        {
                            queryable = queryOptions.ApplyTo(queryable);
                        }

                        Contract.Assert(queryable != null);

                        // we don't support shape changing query composition
                        ((ObjectContent)response.Content).Value = queryable;
                    }
                    catch (ODataException e)
                    {
                        actionExecutedContext.Response = request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            SRResources.UriQueryStringInvalid,
                            e);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Override this method to expand the out of box support for OData query.
        /// </summary>
        /// <param name="request">The incoming request</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed after being sent.")]
        private static void ValidateQuery(HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            IEnumerable<KeyValuePair<string, string>> queryParameters = request.GetQueryNameValuePairs();
            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                if (!ODataQueryOptions.IsSupported(kvp.Key) &&
                     kvp.Key.StartsWith("$", StringComparison.Ordinal))
                {
                    // we don't allow any query parameters that starts with $ but we don't understand
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Error.Format(SRResources.QueryParameterNotSupported, kvp.Key)));
                }
            }
        }
    }
}
