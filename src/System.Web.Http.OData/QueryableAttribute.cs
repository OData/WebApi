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
        private HandleNullPropagationOption _handleNullPropagationOption = HandleNullPropagationOption.Default;
        private int _lambdaNestingLimit = 1;
        private int? _resultLimit;

        /// <summary>
        /// Enables a controller action to support OData query parameters.
        /// </summary>
        public QueryableAttribute()
        {
            EnsureStableOrdering = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether query composition should
        /// alter the original query when necessary to ensure a stable sort order.
        /// </summary>
        /// <value>A <c>true</c> value indicates the original query should
        /// be modified when necessary to guarantee a stable sort order.
        /// A <c>false</c> value indicates the sort order can be considered
        /// stable without modifying the query.  Query providers that ensure
        /// a stable sort order should set this value to <c>false</c>.
        /// The default value is <c>true</c>.</value>
        public bool EnsureStableOrdering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how null propagation should
        /// be handled during query composition. 
        /// </summary>
        /// <value>
        /// The default is <see cref="HandleNullPropagationOption.Default"/>.
        /// </value>
        public HandleNullPropagationOption HandleNullPropagation
        {
            get
            {
                return _handleNullPropagationOption;
            }
            set
            {
                HandleNullPropagationOptionHelper.Validate(value, "value");
                _handleNullPropagationOption = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth of the Any or All elements nested inside the query.
        /// </summary>
        /// <remarks>
        /// This limit helps prevent Denial of Service attacks. The default value is 1.
        /// </remarks>
        /// <value>
        /// The maxiumum depth of the Any or All elements nested inside the query.
        /// </value>
        public int LambdaNestingLimit
        {
            get
            {
                return _lambdaNestingLimit;
            }
            set
            {
                if (value <= 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
                }

                _lambdaNestingLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of query results to send back to clients.
        /// </summary>
        /// <value>
        /// The maximum number of query results to send back to clients.
        /// </value>
        public int ResultLimit
        {
            get
            {
                return _resultLimit ?? default(int);
            }
            set
            {
                if (value <= 0)
                {
                    throw Error.ArgumentMustBeGreaterThanOrEqualTo("value", value, 1);
                }
                _resultLimit = value;
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

            if (response != null && response.IsSuccessStatusCode)
            {
                ObjectContent responseContent = response.Content as ObjectContent;
                if (responseContent == null)
                {
                    throw Error.InvalidOperation(SRResources.QueryingRequiresObjectContent, response.Content.GetType().FullName);
                }
                ValidateReturnType(responseContent.ObjectType, actionDescriptor);

                // Apply the query if there are any query options or if there is a result limit set
                if (responseContent.Value != null && request.RequestUri != null &&
                    (!String.IsNullOrWhiteSpace(request.RequestUri.Query) || _resultLimit.HasValue))
                {
                    ValidateQuery(request);

                    try
                    {
                        IEnumerable query = responseContent.Value as IEnumerable;
                        Contract.Assert(query != null, "ValidateResponseContent should have ensured the responseContent implements IEnumerable");
                        IQueryable queryResults = ExecuteQuery(query, request, configuration, actionDescriptor);
                        responseContent.Value = queryResults;
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

        private static void ValidateReturnType(Type responseContentType, HttpActionDescriptor actionDescriptor)
        {
            if (!IsSupportedReturnType(responseContentType))
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidReturnTypeForQuerying,
                    actionDescriptor.ActionName,
                    actionDescriptor.ControllerDescriptor.ControllerName,
                    responseContentType.FullName);
            }
        }

        internal static bool IsSupportedReturnType(Type objectType)
        {
            Contract.Assert(objectType != null);

            if (objectType == typeof(IEnumerable) || objectType == typeof(IQueryable))
            {
                return true;
            }

            if (objectType.IsGenericType)
            {
                Type genericTypeDefinition = objectType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(IEnumerable<>) || genericTypeDefinition == typeof(IQueryable<>))
                {
                    return true;
                }
            }

            return false;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed after being sent.")]
        private IQueryable ExecuteQuery(IEnumerable query, HttpRequestMessage request, HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            Type originalQueryType = query.GetType();
            Type entityClrType = TypeHelper.GetImplementedIEnumerableType(originalQueryType);

            if (entityClrType == null)
            {
                // The element type cannot be determined because the type of the content
                // is not IEnumerable<T> or IQueryable<T>.
                throw Error.InvalidOperation(
                    SRResources.FailedToRetrieveTypeToBuildEdmModel,
                    this.GetType().Name,
                    actionDescriptor.ActionName,
                    actionDescriptor.ControllerDescriptor.ControllerName,
                    originalQueryType.FullName);
            }

            ODataQueryContext queryContext = CreateQueryContext(entityClrType, configuration, actionDescriptor);
            ODataQueryOptions queryOptions = new ODataQueryOptions(queryContext, request);

            // Filter and OrderBy require entity sets.  Top and Skip may accept primitives.
            if (queryContext.IsPrimitiveClrType && (queryOptions.Filter != null || queryOptions.OrderBy != null))
            {
                // An attempt to use a query option not allowed for primitive types
                // generates a BadRequest with a general message that avoids information disclosure.
                throw new HttpResponseException(request.CreateErrorResponse(
                                                    HttpStatusCode.BadRequest,
                                                    SRResources.OnlySkipAndTopSupported));
            }

            // apply the query
            IQueryable queryable = query as IQueryable;
            if (queryable == null)
            {
                queryable = query.AsQueryable();
            }

            ODataQuerySettings querySettings = new ODataQuerySettings
            {
                EnsureStableOrdering = EnsureStableOrdering,
                HandleNullPropagation = HandleNullPropagation,
                LambdaNestingLimit = LambdaNestingLimit,
                ResultLimit = _resultLimit
            };

            return queryOptions.ApplyTo(queryable, querySettings);
        }

        private static ODataQueryContext CreateQueryContext(Type entityClrType, HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            // Primitive types do not construct an EDM model and deal only with the CLR Type
            if (TypeHelper.IsQueryPrimitiveType(entityClrType))
            {
                return new ODataQueryContext(entityClrType);
            }
            else
            {
                // Get model for the entire app
                IEdmModel model = configuration.GetEdmModel();

                if (model == null)
                {
                    // user has not configured anything, now let's create one just for this type
                    // and cache it in the action descriptor
                    model = actionDescriptor.GetEdmModel(entityClrType);
                    Contract.Assert(model != null);
                }

                // parses the query from request uri
                return new ODataQueryContext(model, entityClrType);
            }
        }

        /// <summary>
        /// Validates that the OData query parameters of the incoming request are supported.
        /// </summary>
        /// <remarks>
        /// Override this method to add support for new OData query parameters
        /// Throw <see cref="HttpResponseException"/> for unsupported query parameters.
        /// </remarks>
        /// <param name="request">The incoming request</param>
        /// <exception cref="HttpResponseException">The request contains unsupported OData query parameters.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed after being sent.")]
        public virtual void ValidateQuery(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

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
