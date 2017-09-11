// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Web.OData.Query;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable querying using the OData query
    /// syntax. To avoid processing unexpected or malicious queries, use the validation settings on
    /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
    /// http://go.microsoft.com/fwlink/?LinkId=279712.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "We want to be able to subclass this type.")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class EnableQueryAttribute : ActionFilterAttribute
    {
        private const char CommaSeparator = ',';

        // validation settings
        private ODataValidationSettings _validationSettings;
        private string _allowedOrderByProperties;

        // query settings
        private ODataQuerySettings _querySettings;

        /// <summary>
        /// Enables a controller action to support OData query parameters.
        /// </summary>
        public EnableQueryAttribute()
        {
            _validationSettings = new ODataValidationSettings();
            _querySettings = new ODataQuerySettings();
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
        public bool EnsureStableOrdering
        {
            get
            {
                return _querySettings.EnsureStableOrdering;
            }
            set
            {
                _querySettings.EnsureStableOrdering = value;
            }
        }

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
                return _querySettings.HandleNullPropagation;
            }
            set
            {
                _querySettings.HandleNullPropagation = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether constants should be parameterized. Parameterizing constants
        /// would result in better performance with Entity framework.
        /// </summary>
        /// <value>The default value is <c>true</c>.</value>
        public bool EnableConstantParameterization
        {
            get
            {
                return _querySettings.EnableConstantParameterization;
            }
            set
            {
                _querySettings.EnableConstantParameterization = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum depth of the Any or All elements nested inside the query. This limit helps prevent
        /// Denial of Service attacks.
        /// </summary>
        /// <value>
        /// The maximum depth of the Any or All elements nested inside the query. The default value is 1.
        /// </value>
        public int MaxAnyAllExpressionDepth
        {
            get
            {
                return _validationSettings.MaxAnyAllExpressionDepth;
            }
            set
            {
                _validationSettings.MaxAnyAllExpressionDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of nodes inside the $filter syntax tree.
        /// </summary>
        /// <value>The default value is 100.</value>
        public int MaxNodeCount
        {
            get
            {
                return _validationSettings.MaxNodeCount;
            }
            set
            {
                _validationSettings.MaxNodeCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of query results to send back to clients.
        /// </summary>
        /// <value>
        /// The maximum number of query results to send back to clients.
        /// </value>
        public int PageSize
        {
            get
            {
                return _querySettings.PageSize ?? default(int);
            }
            set
            {
                _querySettings.PageSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the query parameters that are allowed in queries.
        /// </summary>
        /// <value>The default includes all query options: $filter, $skip, $top, $orderby, $expand, $select, $count,
        /// $format, $skiptoken and $deltatoken.</value>
        public AllowedQueryOptions AllowedQueryOptions
        {
            get
            {
                return _validationSettings.AllowedQueryOptions;
            }
            set
            {
                _validationSettings.AllowedQueryOptions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed functions used in the $filter query. Supported
        /// functions include the following:
        /// <list type="definition">
        /// <item>
        /// <term>String related:</term>
        /// <description>contains, endswith, startswith, length, indexof, substring, tolower, toupper, trim,
        /// concat e.g. ~/Customers?$filter=length(CompanyName) eq 19</description>
        /// </item>
        /// <item>
        /// <term>DateTime related:</term>
        /// <description>year, month, day, hour, minute, second, fractionalseconds, date, time
        /// e.g. ~/Employees?$filter=year(BirthDate) eq 1971</description>
        /// </item>
        /// <item>
        /// <term>Math related:</term>
        /// <description>round, floor, ceiling</description>
        /// </item>
        /// <item>
        /// <term>Type related:</term>
        /// <description>isof, cast</description>
        /// </item>
        /// <item>
        /// <term>Collection related:</term>
        /// <description>any, all</description>
        /// </item>
        /// </list>
        /// </summary>
        public AllowedFunctions AllowedFunctions
        {
            get
            {
                return _validationSettings.AllowedFunctions;
            }
            set
            {
                _validationSettings.AllowedFunctions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed arithmetic operators including 'add', 'sub', 'mul',
        /// 'div', 'mod'.
        /// </summary>
        public AllowedArithmeticOperators AllowedArithmeticOperators
        {
            get
            {
                return _validationSettings.AllowedArithmeticOperators;
            }
            set
            {
                _validationSettings.AllowedArithmeticOperators = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that represents a list of allowed logical Operators such as 'eq', 'ne', 'gt', 'ge',
        /// 'lt', 'le', 'and', 'or', 'not'.
        /// </summary>
        public AllowedLogicalOperators AllowedLogicalOperators
        {
            get
            {
                return _validationSettings.AllowedLogicalOperators;
            }
            set
            {
                _validationSettings.AllowedLogicalOperators = value;
            }
        }

        /// <summary>
        /// <para>Gets or sets a string with comma separated list of property names. The queryable result can only be
        /// ordered by those properties defined in this list.</para>
        ///
        /// <para>Note, by default this string is null, which means it can be ordered by any property.</para>
        ///
        /// <para>For example, setting this value to null or empty string means that we allow ordering the queryable
        /// result by any properties. Setting this value to "Name" means we only allow queryable result to be ordered
        /// by Name property.</para>
        /// </summary>
        public string AllowedOrderByProperties
        {
            get
            {
                return _allowedOrderByProperties;
            }
            set
            {
                _allowedOrderByProperties = value;

                if (String.IsNullOrEmpty(value))
                {
                    _validationSettings.AllowedOrderByProperties.Clear();
                }
                else
                {
                    // now parse the value and set it to validationSettings
                    string[] properties = _allowedOrderByProperties.Split(CommaSeparator);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        _validationSettings.AllowedOrderByProperties.Add(properties[i].Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the max value of $skip that a client can request.
        /// </summary>
        public int MaxSkip
        {
            get
            {
                return _validationSettings.MaxSkip ?? default(int);
            }
            set
            {
                _validationSettings.MaxSkip = value;
            }
        }

        /// <summary>
        /// Gets or sets the max value of $top that a client can request.
        /// </summary>
        public int MaxTop
        {
            get
            {
                return _validationSettings.MaxTop ?? default(int);
            }
            set
            {
                _validationSettings.MaxTop = value;
            }
        }

        /// <summary>
        /// Gets or sets the max expansion depth for the $expand query option. To disable the maximum expansion depth
        /// check, set this property to 0.
        /// </summary>
        public int MaxExpansionDepth
        {
            get { return _validationSettings.MaxExpansionDepth; }
            set
            {
                _validationSettings.MaxExpansionDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of expressions that can be present in the $orderby.
        /// </summary>
        public int MaxOrderByNodeCount
        {
            get { return _validationSettings.MaxOrderByNodeCount; }
            set
            {
                _validationSettings.MaxOrderByNodeCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum amount of time $select and $expand evaluations will be cached. The value may be
        /// set to <c>0</c> to disable caching completely.
        /// </summary>
        public int SelectExpandEvaluationCacheLifetimeSeconds
        {
            get { return _querySettings.SelectExpandCacheExpirationTimeSeconds; }
            set
            {
                _querySettings.SelectExpandCacheExpirationTimeSeconds = value;
            }
        }

        /// <summary>
        /// Performs the query composition after action is executed. It first tries to retrieve the IQueryable from the
        /// returning response message. It then validates the query from uri based on the validation settings on
        /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
        /// the response message.
        /// </summary>
        /// <param name="actionExecutedContext">The context related to this action, including the response message,
        /// request message and HttpConfiguration etc.</param>
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

            if (response != null && response.IsSuccessStatusCode && response.Content != null)
            {
                ObjectContent responseContent = response.Content as ObjectContent;
                if (responseContent == null)
                {
                    throw Error.Argument("actionExecutedContext", SRResources.QueryingRequiresObjectContent,
                        response.Content.GetType().FullName);
                }

                ODataQueryContext queryContext = null;

                if (!_querySettings.PageSize.HasValue && responseContent.Value != null)
                {
                    GetModelBoundPageSize(queryContext, responseContent, request, actionDescriptor, actionExecutedContext);
                }

                // Apply the query if there are any query options, if there is a page size set, in the case of
                // SingleResult or in the case of $count request.
                bool shouldApplyQuery = responseContent.Value != null &&
                    request.RequestUri != null &&
                    (!String.IsNullOrWhiteSpace(request.RequestUri.Query) ||
                    _querySettings.PageSize.HasValue ||
                    _querySettings.ModelBoundPageSize.HasValue ||
                    responseContent.Value is SingleResult ||
                    ODataCountMediaTypeMapping.IsCountRequest(request) ||
                    ContainsAutoSelectExpandProperty(responseContent.Value, request, actionDescriptor));

                if (shouldApplyQuery)
                {
                    try
                    {
                        object queryResult = ExecuteQuery(responseContent.Value, request, actionDescriptor, queryContext);
                        if (queryResult == null && IsNonODataPathOrSingleResult(request, responseContent.Value))
                        {
                            // This is the case in which a regular OData service uses the EnableQuery attribute.
                            // For OData services ODataNullValueMessageHandler should be plugged in for the service
                            // if this behavior is desired.
                            // For non OData services this behavior is equivalent as the one in the v3 version in order
                            // to reduce the friction when they decide to move to use the v4 EnableQueryAttribute.
                            actionExecutedContext.Response = request.CreateResponse(HttpStatusCode.NotFound);
                        }
                        else
                        {
                            responseContent.Value = queryResult;
                        }
                    }
                    catch (NotImplementedException e)
                    {
                        actionExecutedContext.Response = request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                            e);
                    }
                    catch (NotSupportedException e)
                    {
                        actionExecutedContext.Response = request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                            e);
                    }
                    catch (InvalidOperationException e)
                    {
                        // Will also catch ODataException here because ODataException derives from InvalidOperationException.
                        actionExecutedContext.Response = request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                            e);
                    }
                }
            }
        }

        /// <summary>
        /// Validates the OData query in the incoming request. By default, the implementation throws an exception if
        /// the query contains unsupported query parameters. Override this method to perform additional validation of
        /// the query.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="queryOptions">
        /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Response disposed after being sent.")]
        public virtual void ValidateQuery(HttpRequestMessage request, ODataQueryOptions queryOptions)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }

            IEnumerable<KeyValuePair<string, string>> queryParameters = request.GetQueryNameValuePairs();
            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                if (!queryOptions.IsSupportedQueryOption(kvp.Key) &&
                     kvp.Key.StartsWith("$", StringComparison.Ordinal))
                {
                    // we don't support any custom query options that start with $
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Error.Format(SRResources.QueryParameterNotSupported, kvp.Key)));
                }
            }

            queryOptions.Validate(_validationSettings);
        }

        /// <summary>
        /// Applies the query to the given IQueryable based on incoming query from uri and query settings. By default,
        /// the implementation supports $top, $skip, $orderby and $filter. Override this method to perform additional
        /// query composition of the query.
        /// </summary>
        /// <param name="queryable">The original queryable instance from the response message.</param>
        /// <param name="queryOptions">
        /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
        /// </param>
        public virtual IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
        {
            if (queryable == null)
            {
                throw Error.ArgumentNull("queryable");
            }
            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }

            return queryOptions.ApplyTo(queryable, _querySettings);
        }

        /// <summary>
        /// Applies the query to the given entity based on incoming query from uri and query settings.
        /// </summary>
        /// <param name="entity">The original entity from the response message.</param>
        /// <param name="queryOptions">
        /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
        /// </param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        public virtual object ApplyQuery(object entity, ODataQueryOptions queryOptions)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }

            return queryOptions.ApplyTo(entity, _querySettings);
        }

        private ODataQueryContext GetODataQueryContext(object response, HttpRequestMessage request, HttpActionDescriptor actionDescriptor)
        {
            Type elementClrType = GetElementType(response, actionDescriptor);

            IEdmModel model = GetModel(elementClrType, request, actionDescriptor);
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
            }

            return new ODataQueryContext(model, elementClrType, request.ODataProperties().Path);
        }

        private void GetModelBoundPageSize(ODataQueryContext queryContext, ObjectContent responseContent,
            HttpRequestMessage request, HttpActionDescriptor actionDescriptor,
            HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                queryContext = GetODataQueryContext(responseContent.Value, request, actionDescriptor);
            }
            catch (InvalidOperationException e)
            {
                actionExecutedContext.Response = request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                    e);
                return;
            }

            ModelBoundQuerySettings querySettings = EdmLibHelpers.GetModelBoundQuerySettings(queryContext.TargetProperty,
                queryContext.TargetStructuredType,
                queryContext.Model);
            if (querySettings != null && querySettings.PageSize.HasValue)
            {
                _querySettings.ModelBoundPageSize = querySettings.PageSize;
            }
        }

        private object ExecuteQuery(object response, HttpRequestMessage request, HttpActionDescriptor actionDescriptor, ODataQueryContext queryContext)
        {
            if (queryContext == null)
            {
                queryContext = GetODataQueryContext(response, request, actionDescriptor);
            }

            ODataQueryOptions queryOptions = new ODataQueryOptions(queryContext, request);

            ValidateQuery(request, queryOptions);

            // apply the query
            IEnumerable enumerable = response as IEnumerable;
            if (enumerable == null || response is string || response is byte[])
            {
                // response is not a collection; we only support $select and $expand on single entities.
                ValidateSelectExpandOnly(queryOptions);

                SingleResult singleResult = response as SingleResult;
                if (singleResult == null)
                {
                    // response is a single entity.
                    return ApplyQuery(entity: response, queryOptions: queryOptions);
                }
                else
                {
                    // response is a composable SingleResult. ApplyQuery and call SingleOrDefault.
                    IQueryable queryable = singleResult.Queryable;
                    queryable = ApplyQuery(queryable, queryOptions);
                    return SingleOrDefault(queryable, actionDescriptor);
                }
            }
            else
            {
                // response is a collection.
                IQueryable queryable = (enumerable as IQueryable) ?? enumerable.AsQueryable();
                queryable = ApplyQuery(queryable, queryOptions);

                if (ODataCountMediaTypeMapping.IsCountRequest(request))
                {
                    long? count = request.ODataProperties().TotalCount;

                    if (count.HasValue)
                    {
                        // Return the count value if it is a $count request.
                        return count.Value;
                    }
                }

                return queryable;
            }
        }

        /// <summary>
        /// Gets the EDM model for the given type and request. Override this method to customize the EDM model used for
        /// querying.
        /// </summary>
        /// <param name="elementClrType">The CLR type to retrieve a model for.</param>
        /// <param name="request">The request message to retrieve a model for.</param>
        /// <param name="actionDescriptor">The action descriptor for the action being queried on.</param>
        /// <returns>The EDM model for the given type and request.</returns>
        public virtual IEdmModel GetModel(Type elementClrType, HttpRequestMessage request,
            HttpActionDescriptor actionDescriptor)
        {
            // Get model for the request
            IEdmModel model = request.GetModel();

            if (model == EdmCoreModel.Instance || model.GetEdmType(elementClrType) == null)
            {
                // user has not configured anything or has registered a model without the element type
                // let's create one just for this type and cache it in the action descriptor
                model = actionDescriptor.GetEdmModel(elementClrType);
            }

            Contract.Assert(model != null);
            return model;
        }

        internal static Type GetElementType(object response, HttpActionDescriptor actionDescriptor)
        {
            Contract.Assert(response != null);

            IEnumerable enumerable = response as IEnumerable;
            if (enumerable == null)
            {
                SingleResult singleResult = response as SingleResult;
                if (singleResult == null)
                {
                    return response.GetType();
                }

                enumerable = singleResult.Queryable;
            }

            Type elementClrType = TypeHelper.GetImplementedIEnumerableType(enumerable.GetType());
            if (elementClrType == null)
            {
                // The element type cannot be determined because the type of the content
                // is not IEnumerable<T> or IQueryable<T>.
                throw Error.InvalidOperation(
                    SRResources.FailedToRetrieveTypeToBuildEdmModel,
                    typeof(EnableQueryAttribute).Name,
                    actionDescriptor.ActionName,
                    actionDescriptor.ControllerDescriptor.ControllerName,
                    response.GetType().FullName);
            }

            return elementClrType;
        }

        internal static object SingleOrDefault(IQueryable queryable, HttpActionDescriptor actionDescriptor)
        {
            var enumerator = queryable.GetEnumerator();
            try
            {
                var result = enumerator.MoveNext() ? enumerator.Current : null;

                if (enumerator.MoveNext())
                {
                    throw new InvalidOperationException(Error.Format(
                        SRResources.SingleResultHasMoreThanOneEntity,
                        actionDescriptor.ActionName,
                        actionDescriptor.ControllerDescriptor.ControllerName,
                        typeof(SingleResult).Name));
                }

                return result;
            }
            finally
            {
                // Fix for Issue #2097
                // Ensure any active/open database objects that were created
                // iterating over the IQueryable object are properly closed.
                var disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        internal static void ValidateSelectExpandOnly(ODataQueryOptions queryOptions)
        {
            if (queryOptions.Filter != null || queryOptions.Count != null || queryOptions.OrderBy != null
                || queryOptions.Skip != null || queryOptions.Top != null)
            {
                throw new ODataException(Error.Format(SRResources.NonSelectExpandOnSingleEntity));
            }
        }

        private bool ContainsAutoSelectExpandProperty(object response, HttpRequestMessage request,
            HttpActionDescriptor actionDescriptor)
        {
            Type elementClrType = GetElementType(response, actionDescriptor);

            IEdmModel model = GetModel(elementClrType, request, actionDescriptor);
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
            }

            IEdmEntityType baseEntityType = model.GetEdmType(elementClrType) as IEdmEntityType;
            IEdmStructuredType structuredType = model.GetEdmType(elementClrType) as IEdmStructuredType;
            IEdmProperty property = null;
            if (request.ODataProperties().Path != null)
            {
                string name;
                EdmLibHelpers.GetPropertyAndStructuredTypeFromPath(request.ODataProperties().Path.Segments, out property,
                    out structuredType,
                    out name);
            }

            if (baseEntityType != null)
            {
                List<IEdmEntityType> entityTypes = new List<IEdmEntityType>();
                entityTypes.Add(baseEntityType);
                entityTypes.AddRange(EdmLibHelpers.GetAllDerivedEntityTypes(baseEntityType, model));
                foreach (var entityType in entityTypes)
                {
                    IEnumerable<IEdmNavigationProperty> navigationProperties = entityType == baseEntityType
                        ? entityType.NavigationProperties()
                        : entityType.DeclaredNavigationProperties();
                    if (navigationProperties != null)
                    {
                        if (navigationProperties.Any(
                                navigationProperty =>
                                    EdmLibHelpers.IsAutoExpand(navigationProperty, property, entityType, model)))
                        {
                            return true;
                        }
                    }

                    IEnumerable<IEdmStructuralProperty> properties = entityType == baseEntityType
                        ? entityType.StructuralProperties()
                        : entityType.DeclaredStructuralProperties();
                    if (properties != null)
                    {
                        foreach (var edmProperty in properties)
                        {
                            if (EdmLibHelpers.IsAutoSelect(edmProperty, property, entityType, model))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else if (structuredType != null)
            {
                IEnumerable<IEdmStructuralProperty> properties = structuredType.StructuralProperties();
                if (properties != null)
                {
                    foreach (var edmProperty in properties)
                    {
                        if (EdmLibHelpers.IsAutoSelect(edmProperty, property, structuredType, model))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsNonODataPathOrSingleResult(HttpRequestMessage request, object value)
        {
            return request.ODataProperties().Path == null || value is SingleResult;
        }
    }
}