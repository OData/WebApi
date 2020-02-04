// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Partial implementation of the EnableQueryAttribute.
    /// </summary>
    public partial class EnableQueryAttribute
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
        /// Gets or sets a value indicating whether queries with expanded navigations should be formulated
        /// to encourage correlated subquery results to be buffered.
        /// Buffering correlated subquery results can reduce the number of queries from N + 1 to 2
        /// by buffering results from the subquery.
        /// </summary>
        /// <value>The default value is <c>false</c>.</value>
        public bool EnableCorrelatedSubqueryBuffering
        {
            get
            {
                return _querySettings.EnableCorrelatedSubqueryBuffering;
            }
            set
            {
                _querySettings.EnableCorrelatedSubqueryBuffering = value;
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
        /// Honor $filter inside $expand of non-collection navigation property.
        /// The expanded property is only populated when the filter evaluates to true.
        /// This setting is false by default.
        /// </summary>
        public bool HandleReferenceNavigationPropertyExpandFilter
        {
            get
            {
                return _querySettings.HandleReferenceNavigationPropertyExpandFilter;
            }
            set
            {
                _querySettings.HandleReferenceNavigationPropertyExpandFilter = value;
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
        /// Performs the query composition after action is executed. It first tries to retrieve the IQueryable from the
        /// returning response message. It then validates the query from uri based on the validation settings on
        /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
        /// the response message.
        /// </summary>
        /// <param name="responseValue">The response content value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <param name="request">The internal request.</param>
        /// <param name="modelFunction">A function to get the model.</param>
        /// <param name="createQueryOptionFunction">A function used to create and validate query options.</param>
        /// <param name="createResponseAction">An action used to create a response.</param>
        /// <param name="createErrorAction">A function used to generate error response.</param>
        private object OnActionExecuted(
            object responseValue,
            IQueryable singleResultCollection,
            IWebApiActionDescriptor actionDescriptor,
            IWebApiRequestMessage request,
            Func<Type, IEdmModel> modelFunction,
            Func<ODataQueryContext, ODataQueryOptions> createQueryOptionFunction,
            Action<HttpStatusCode> createResponseAction,
            Action<HttpStatusCode, string, Exception> createErrorAction)
        {
            if (!_querySettings.PageSize.HasValue && responseValue != null)
            {
                GetModelBoundPageSize(responseValue, singleResultCollection, actionDescriptor, modelFunction, request.Context.Path, createErrorAction);
            }

            // Apply the query if there are any query options, if there is a page size set, in the case of
            // SingleResult or in the case of $count request.
            bool shouldApplyQuery = responseValue != null &&
                request.RequestUri != null &&
                (!String.IsNullOrWhiteSpace(request.RequestUri.Query) ||
                _querySettings.PageSize.HasValue ||
                _querySettings.ModelBoundPageSize.HasValue ||
                singleResultCollection != null ||
                request.IsCountRequest() ||
                ContainsAutoSelectExpandProperty(responseValue, singleResultCollection, actionDescriptor, modelFunction, request.Context.Path));

            object returnValue = null;
            if (shouldApplyQuery)
            {
                try
                {
                    object queryResult = ExecuteQuery(responseValue, singleResultCollection, actionDescriptor, modelFunction, request, createQueryOptionFunction);
                    if (queryResult == null && (request.Context.Path == null || singleResultCollection != null))
                    {
                        // This is the case in which a regular OData service uses the EnableQuery attribute.
                        // For OData services ODataNullValueMessageHandler should be plugged in for the service
                        // if this behavior is desired.
                        // For non OData services this behavior is equivalent as the one in the v3 version in order
                        // to reduce the friction when they decide to move to use the v4 EnableQueryAttribute.
                        createResponseAction(HttpStatusCode.NotFound);
                    }

                    returnValue = queryResult;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    createErrorAction(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.QueryParameterNotSupported, e.Message),
                        e);
                }
                catch (NotImplementedException e)
                {
                    createErrorAction(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                        e);
                }
                catch (NotSupportedException e)
                {
                    createErrorAction(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                        e);
                }
                catch (InvalidOperationException e)
                {
                    // Will also catch ODataException here because ODataException derives from InvalidOperationException.
                    createErrorAction(
                        HttpStatusCode.BadRequest,
                        Error.Format(SRResources.UriQueryStringInvalid, e.Message),
                        e);
                }
            }

            return returnValue;
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

        /// <summary>
        /// Get the ODaya query context.
        /// </summary>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <param name="modelFunction">A function to get the model.</param>
        /// <param name="path">The OData path.</param>
        /// <returns></returns>
        private static ODataQueryContext GetODataQueryContext(
            object responseValue,
            IQueryable singleResultCollection,
            IWebApiActionDescriptor actionDescriptor,
            Func<Type, IEdmModel> modelFunction,
            ODataPath path)
        {
            Type elementClrType = GetElementType(responseValue, singleResultCollection, actionDescriptor);

            IEdmModel model = modelFunction(elementClrType);
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
            }

            return new ODataQueryContext(model, elementClrType, path);
        }

        /// <summary>
        /// Get the page size.
        /// </summary>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <param name="modelFunction">A function to get the model.</param>
        /// <param name="path">The OData path.</param>
        /// <param name="createErrorAction">A function used to generate error response.</param>
        private void GetModelBoundPageSize(
            object responseValue,
            IQueryable singleResultCollection,
            IWebApiActionDescriptor actionDescriptor,
            Func<Type, IEdmModel> modelFunction,
            ODataPath path,
            Action<HttpStatusCode, string, Exception> createErrorAction)
        {
            ODataQueryContext queryContext = null;

            try
            {
                queryContext = GetODataQueryContext(responseValue, singleResultCollection, actionDescriptor, modelFunction, path);
            }
            catch (InvalidOperationException e)
            {
                createErrorAction(
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

        /// <summary>
        /// Execute the query.
        /// </summary>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <param name="modelFunction">A function to get the model.</param>
        /// <param name="request">The internal request.</param>
        /// <param name="createQueryOptionFunction">A function used to create and validate query options.</param>
        /// <returns></returns>
        private object ExecuteQuery(
            object responseValue,
            IQueryable singleResultCollection,
            IWebApiActionDescriptor actionDescriptor,
            Func<Type, IEdmModel> modelFunction,
            IWebApiRequestMessage request,
            Func<ODataQueryContext, ODataQueryOptions> createQueryOptionFunction)
        {
            ODataQueryContext queryContext = GetODataQueryContext(responseValue, singleResultCollection, actionDescriptor, modelFunction, request.Context.Path);

            // Create and validate the query options.
            ODataQueryOptions queryOptions = createQueryOptionFunction(queryContext);

            // apply the query
            IEnumerable enumerable = responseValue as IEnumerable;
            if (enumerable == null || responseValue is string || responseValue is byte[])
            {
                // response is not a collection; we only support $select and $expand on single entities.
                ValidateSelectExpandOnly(queryOptions);

                if (singleResultCollection == null)
                {
                    // response is a single entity.
                    return ApplyQuery(entity: responseValue, queryOptions: queryOptions);
                }
                else
                {
                    IQueryable queryable = singleResultCollection as IQueryable;
                    queryable = ApplyQuery(queryable, queryOptions);
                    return SingleOrDefault(queryable, actionDescriptor);
                }
            }
            else
            {
                // response is a collection.
                IQueryable queryable = (enumerable as IQueryable) ?? enumerable.AsQueryable();
                queryable = ApplyQuery(queryable, queryOptions);

                if (request.IsCountRequest())
                {
                    long? count = request.Context.TotalCount;

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
        /// Get the element type.
        /// </summary>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <returns></returns>
        internal static Type GetElementType(
            object responseValue,
            IQueryable singleResultCollection,
            IWebApiActionDescriptor actionDescriptor)
        {
            Contract.Assert(responseValue != null);

            IEnumerable enumerable = responseValue as IEnumerable;
            if (enumerable == null)
            {
                if (singleResultCollection == null)
                {
                    return responseValue.GetType();
                }

                enumerable = singleResultCollection as IEnumerable;
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
                    actionDescriptor.ControllerName,
                    responseValue.GetType().FullName);
            }

            return elementClrType;
        }

        /// <summary>
        /// Get a single or default value from a collection.
        /// </summary>
        /// <param name="queryable">The response value as <see cref="IQueryable"/>.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <returns></returns>
        internal static object SingleOrDefault(
            IQueryable queryable,
            IWebApiActionDescriptor actionDescriptor)
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
                        actionDescriptor.ControllerName,
                        "SingleResult"));
                }

                return result;
            }
            finally
            {
                // Ensure any active/open database objects that were created
                // iterating over the IQueryable object are properly closed.
                var disposable = enumerator as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Validate the select and expand options.
        /// </summary>
        /// <param name="queryOptions">The query options.</param>
        internal static void ValidateSelectExpandOnly(ODataQueryOptions queryOptions)
        {
            if (queryOptions.Filter != null || queryOptions.Count != null || queryOptions.OrderBy != null
                || queryOptions.Skip != null || queryOptions.Top != null)
            {
                throw new ODataException(Error.Format(SRResources.NonSelectExpandOnSingleEntity));
            }
        }

        /// <summary>
        /// Determine if the 
        /// </summary>
        /// <param name="responseValue">The response value.</param>
        /// <param name="singleResultCollection">The content as SingleResult.Queryable.</param>
        /// <param name="actionDescriptor">The action context, i.e. action and controller name.</param>
        /// <param name="modelFunction">A function to get the model.</param>
        /// <param name="path">The OData path.</param>
        /// <returns></returns>
        private static bool ContainsAutoSelectExpandProperty(
            object responseValue,
            IQueryable singleResultCollection,
            IWebApiActionDescriptor actionDescriptor,
            Func<Type, IEdmModel> modelFunction,
            ODataPath path)
        {
            Type elementClrType = GetElementType(responseValue, singleResultCollection, actionDescriptor);

            IEdmModel model = modelFunction(elementClrType);
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
            }

            IEdmType edmType = model.GetTypeMappingCache().GetEdmType(elementClrType, model)?.Definition;
            IEdmEntityType baseEntityType = edmType as IEdmEntityType;
            IEdmStructuredType structuredType = edmType as IEdmStructuredType;
            IEdmProperty property = null;
            if (path != null)
            {
                string name;
                EdmLibHelpers.GetPropertyAndStructuredTypeFromPath(path.Segments, out property,
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
   }
}