// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Web.OData.Query.Validators;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace System.Web.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip, and $count.
    /// </summary>
    [ODataQueryParameterBinding]
    public class ODataQueryOptions
    {
        private static readonly MethodInfo _limitResultsGenericMethod = typeof(ODataQueryOptions).GetMethod("LimitResults");

        private IAssembliesResolver _assembliesResolver;

        private ETag _etagIfMatch;

        private bool _etagIfMatchChecked;

        private ETag _etagIfNoneMatch;

        private bool _etagIfNoneMatchChecked;

        private ODataQueryOptionParser _queryOptionParser;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from
        /// the <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information.</param>
        /// <param name="request">The incoming request message.</param>
        public ODataQueryOptions(ODataQueryContext context, HttpRequestMessage request)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (request.GetConfiguration() != null)
            {
                _assembliesResolver = request.GetConfiguration().Services.GetAssembliesResolver();
            }

            // fallback to the default assemblies resolver if none available.
            _assembliesResolver = _assembliesResolver ?? new DefaultAssembliesResolver();

            // remember the context and request
            Context = context;
            Request = request;

            // Parse the query from request Uri
            RawValues = new ODataRawQueryOptions();
            IEnumerable<KeyValuePair<string, string>> queryParameters = request.GetQueryNameValuePairs();

            IDictionary<string, string> queryOptions = queryParameters.ToDictionary(p => p.Key, p => p.Value);
            _queryOptionParser = new ODataQueryOptionParser(context.Model, context.ElementType,
                context.NavigationSource, queryOptions);

            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                switch (kvp.Key)
                {
                    case "$filter":
                        ThrowIfEmpty(kvp.Value, "$filter");
                        RawValues.Filter = kvp.Value;
                        Filter = new FilterQueryOption(kvp.Value, context, _queryOptionParser);
                        break;
                    case "$orderby":
                        ThrowIfEmpty(kvp.Value, "$orderby");
                        RawValues.OrderBy = kvp.Value;
                        OrderBy = new OrderByQueryOption(kvp.Value, context, _queryOptionParser);
                        break;
                    case "$top":
                        ThrowIfEmpty(kvp.Value, "$top");
                        RawValues.Top = kvp.Value;
                        Top = new TopQueryOption(kvp.Value, context, _queryOptionParser);
                        break;
                    case "$skip":
                        ThrowIfEmpty(kvp.Value, "$skip");
                        RawValues.Skip = kvp.Value;
                        Skip = new SkipQueryOption(kvp.Value, context, _queryOptionParser);
                        break;
                    case "$select":
                        RawValues.Select = kvp.Value;
                        break;
                    case "$count":
                        ThrowIfEmpty(kvp.Value, "$count");
                        RawValues.Count = kvp.Value;
                        Count = new CountQueryOption(kvp.Value, context, _queryOptionParser);
                        break;
                    case "$expand":
                        RawValues.Expand = kvp.Value;
                        break;
                    case "$format":
                        RawValues.Format = kvp.Value;
                        break;
                    case "$skiptoken":
                        RawValues.SkipToken = kvp.Value;
                        break;
                    default:
                        // we don't throw if we can't recognize the query
                        break;
                }
            }

            if (RawValues.Select != null || RawValues.Expand != null)
            {
                SelectExpand = new SelectExpandQueryOption(RawValues.Select, RawValues.Expand,
                    context, _queryOptionParser);
            }

            Validator = new ODataQueryValidator();
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the request message associated with this instance.
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// Gets the raw string of all the OData query options
        /// </summary>
        public ODataRawQueryOptions RawValues { get; private set; }

        /// <summary>
        /// Gets the <see cref="SelectExpandQueryOption"/>.
        /// </summary>
        public SelectExpandQueryOption SelectExpand { get; private set; }

        /// <summary>
        /// Gets the <see cref="FilterQueryOption"/>.
        /// </summary>
        public FilterQueryOption Filter { get; private set; }

        /// <summary>
        /// Gets the <see cref="OrderByQueryOption"/>.
        /// </summary>
        public OrderByQueryOption OrderBy { get; private set; }

        /// <summary>
        /// Gets the <see cref="SkipQueryOption"/>.
        /// </summary>
        public SkipQueryOption Skip { get; private set; }

        /// <summary>
        /// Gets the <see cref="TopQueryOption"/>.
        /// </summary>
        public TopQueryOption Top { get; private set; }

        /// <summary>
        /// Gets the <see cref="CountQueryOption"/>.
        /// </summary>
        public CountQueryOption Count { get; private set; }

        /// <summary>
        /// Gets or sets the query validator.
        /// </summary>
        public ODataQueryValidator Validator { get; set; }

        /// <summary>
        /// Gets the <see cref="ETag"/> from IfMatch header.
        /// </summary>
        public virtual ETag IfMatch
        {
            get
            {
                if (!_etagIfMatchChecked && _etagIfMatch == null)
                {
                    EntityTagHeaderValue etagHeaderValue = Request.Headers.IfMatch.SingleOrDefault();
                    _etagIfMatch = GetETag(etagHeaderValue);
                    _etagIfMatchChecked = true;
                }

                return _etagIfMatch;
            }
        }

        /// <summary>
        /// Gets the <see cref="ETag"/> from IfNoneMatch header.
        /// </summary>
        public virtual ETag IfNoneMatch
        {
            get
            {
                if (!_etagIfNoneMatchChecked && _etagIfNoneMatch == null)
                {
                    EntityTagHeaderValue etagHeaderValue = Request.Headers.IfNoneMatch.SingleOrDefault();
                    _etagIfNoneMatch = GetETag(etagHeaderValue);
                    if (_etagIfNoneMatch != null)
                    {
                        _etagIfNoneMatch.IsIfNoneMatch = true;
                    }
                    _etagIfNoneMatchChecked = true;
                }

                return _etagIfNoneMatch;
            }
        }

        /// <summary>
        /// Check if the given query option is an OData system query option.
        /// </summary>
        /// <param name="queryOptionName">The name of the query option.</param>
        /// <returns>Returns <c>true</c> if the query option is an OData system query option.</returns>
        public static bool IsSystemQueryOption(string queryOptionName)
        {
            return queryOptionName == "$orderby" ||
                 queryOptionName == "$filter" ||
                 queryOptionName == "$top" ||
                 queryOptionName == "$skip" ||
                 queryOptionName == "$count" ||
                 queryOptionName == "$expand" ||
                 queryOptionName == "$select" ||
                 queryOptionName == "$format" ||
                 queryOptionName == "$skiptoken";
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public virtual IQueryable ApplyTo(IQueryable query)
        {
            return ApplyTo(query, new ODataQuerySettings());
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public virtual IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull("querySettings");
            }

            IQueryable result = query;

            // Construct the actual query and apply them in the following order: filter, orderby, skip, top
            if (Filter != null)
            {
                result = Filter.ApplyTo(result, querySettings, _assembliesResolver);
            }

            if (Count != null && Request.ODataProperties().TotalCount == null)
            {
                long? count = Count.GetEntityCount(result);
                if (count.HasValue)
                {
                    Request.ODataProperties().TotalCount = count.Value;
                }
            }

            OrderByQueryOption orderBy = OrderBy;

            // $skip or $top require a stable sort for predictable results.
            // Result limits require a stable sort to be able to generate a next page link.
            // If either is present in the query and we have permission,
            // generate an $orderby that will produce a stable sort.
            if (querySettings.EnsureStableOrdering &&
                (Skip != null || Top != null || querySettings.PageSize.HasValue))
            {
                // If there is no OrderBy present, we manufacture a default.
                // If an OrderBy is already present, we add any missing
                // properties necessary to make a stable sort.
                // Instead of failing early here if we cannot generate the OrderBy,
                // let the IQueryable backend fail (if it has to).
                orderBy = orderBy == null
                            ? GenerateDefaultOrderBy(Context)
                            : EnsureStableSortOrderBy(orderBy, Context);
            }

            if (orderBy != null)
            {
                result = orderBy.ApplyTo(result, querySettings);
            }

            if (Skip != null)
            {
                result = Skip.ApplyTo(result, querySettings);
            }

            if (Top != null)
            {
                result = Top.ApplyTo(result, querySettings);
            }

            if (SelectExpand != null)
            {
                Request.ODataProperties().SelectExpandClause = SelectExpand.SelectExpandClause;
                result = SelectExpand.ApplyTo(result, querySettings);
            }

            if (querySettings.PageSize.HasValue)
            {
                bool resultsLimited;
                result = LimitResults(result, querySettings.PageSize.Value, out resultsLimited);
                if (resultsLimited && Request.RequestUri != null && Request.RequestUri.IsAbsoluteUri && Request.ODataProperties().NextLink == null)
                {
                    Uri nextPageLink = GetNextPageLink(Request, querySettings.PageSize.Value);
                    Request.ODataProperties().NextLink = nextPageLink;
                }
            }

            return result;
        }

        /// <summary>
        /// Applies the query to the given entity using the given <see cref="ODataQuerySettings"/>.
        /// </summary>
        /// <param name="entity">The original entity.</param>
        /// <param name="querySettings">The <see cref="ODataQuerySettings"/> that contains all the query application related settings.</param>
        /// <returns>The new entity after the $select and $expand query has been applied to.</returns>
        /// <remarks>Only $select and $expand query options can be applied on single entities. This method throws if the query contains any other
        /// query options.</remarks>
        public virtual object ApplyTo(object entity, ODataQuerySettings querySettings)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (querySettings == null)
            {
                throw Error.ArgumentNull("querySettings");
            }

            if (Filter != null || OrderBy != null || Top != null || Skip != null || Count != null)
            {
                throw Error.InvalidOperation(SRResources.NonSelectExpandOnSingleEntity);
            }

            if (SelectExpand != null)
            {
                Request.ODataProperties().SelectExpandClause = SelectExpand.SelectExpandClause;
                return SelectExpand.ApplyTo(entity, querySettings);
            }

            return entity;
        }

        /// <summary>
        /// Validate all OData queries, including $skip, $top, $orderby and $filter, based on the given <paramref name="validationSettings"/>.
        /// It throws an ODataException if validation failed.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
        public virtual void Validate(ODataValidationSettings validationSettings)
        {
            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            if (Validator != null)
            {
                Validator.Validate(this, validationSettings);
            }
        }

        private static void ThrowIfEmpty(string queryValue, string queryName)
        {
            if (String.IsNullOrWhiteSpace(queryValue))
            {
                throw new ODataException(Error.Format(SRResources.QueryCannotBeEmpty, queryName));
            }
        }

        // Returns a sorted list of all properties that may legally appear
        // in an OrderBy.  If the entity type has keys, all are returned.
        // Otherwise, when no keys are present, all primitive properties are returned.
        private static IEnumerable<IEdmStructuralProperty> GetAvailableOrderByProperties(ODataQueryContext context)
        {
            Contract.Assert(context != null);

            IEdmEntityType entityType = context.ElementType as IEdmEntityType;
            if (entityType != null)
            {
                IEnumerable<IEdmStructuralProperty> properties =
                    entityType.Key().Any()
                        ? entityType.Key()
                        : entityType
                            .StructuralProperties()
                            .Where(property => property.Type.IsPrimitive());

                // Sort properties alphabetically for stable sort
                return properties.OrderBy(property => property.Name);
            }
            else
            {
                return Enumerable.Empty<IEdmStructuralProperty>();
            }
        }

        // Generates the OrderByQueryOption to use by default for $skip or $top
        // when no other $orderby is available.  It will produce a stable sort.
        // This may return a null if there are no available properties.
        private static OrderByQueryOption GenerateDefaultOrderBy(ODataQueryContext context)
        {
            string orderByRaw = String.Join(",",
                                    GetAvailableOrderByProperties(context)
                                        .Select(property => property.Name));

            return String.IsNullOrEmpty(orderByRaw)
                    ? null
                    : new OrderByQueryOption(orderByRaw, context);
        }

        /// <summary>
        /// Ensures the given <see cref="OrderByQueryOption"/> will produce a stable sort.
        /// If it will, the input <paramref name="orderBy"/> will be returned
        /// unmodified.  If the given <see cref="OrderByQueryOption"/> will not produce a
        /// stable sort, a new <see cref="OrderByQueryOption"/> instance will be created
        /// and returned.
        /// </summary>
        /// <param name="orderBy">The <see cref="OrderByQueryOption"/> to evaluate.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/>.</param>
        /// <returns>An <see cref="OrderByQueryOption"/> that will produce a stable sort.</returns>
        private static OrderByQueryOption EnsureStableSortOrderBy(OrderByQueryOption orderBy, ODataQueryContext context)
        {
            Contract.Assert(orderBy != null);
            Contract.Assert(context != null);

            // Strategy: create a hash of all properties already used in the given OrderBy
            // and remove them from the list of properties we need to add to make the sort stable.
            HashSet<string> usedPropertyNames =
                new HashSet<string>(orderBy.OrderByNodes.OfType<OrderByPropertyNode>().Select(node => node.Property.Name));

            IEnumerable<IEdmStructuralProperty> propertiesToAdd = GetAvailableOrderByProperties(context).Where(prop => !usedPropertyNames.Contains(prop.Name));

            if (propertiesToAdd.Any())
            {
                // The existing query options has too few properties to create a stable sort.
                // Clone the given one and add the remaining properties to end, thereby making
                // the sort stable but preserving the user's original intent for the major
                // sort order.
                orderBy = new OrderByQueryOption(orderBy.RawValue, context);

                foreach (IEdmStructuralProperty property in propertiesToAdd)
                {
                    orderBy.OrderByNodes.Add(new OrderByPropertyNode(property, OrderByDirection.Ascending));
                }
            }

            return orderBy;
        }

        internal static IQueryable LimitResults(IQueryable queryable, int limit, out bool resultsLimited)
        {
            MethodInfo genericMethod = _limitResultsGenericMethod.MakeGenericMethod(queryable.ElementType);
            object[] args = new object[] { queryable, limit, null };
            IQueryable results = genericMethod.Invoke(null, args) as IQueryable;
            resultsLimited = (bool)args[2];
            return results;
        }

        /// <summary>
        /// Limits the query results to a maximum number of results.
        /// </summary>
        /// <typeparam name="T">The entity CLR type</typeparam>
        /// <param name="queryable">The queryable to limit.</param>
        /// <param name="limit">The query result limit.</param>
        /// <param name="resultsLimited"><c>true</c> if the query results were limited; <c>false</c> otherwise</param>
        /// <returns>The limited query results.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "Not intended for public use, only public to enable invokation without security issues.")]
        public static IQueryable<T> LimitResults<T>(IQueryable<T> queryable, int limit, out bool resultsLimited)
        {
            TruncatedCollection<T> truncatedCollection = new TruncatedCollection<T>(queryable, limit);
            resultsLimited = truncatedCollection.IsTruncated;
            return truncatedCollection.AsQueryable();
        }

        internal static Uri GetNextPageLink(HttpRequestMessage request, int pageSize)
        {
            Contract.Assert(request != null);
            Contract.Assert(request.RequestUri != null);
            Contract.Assert(request.RequestUri.IsAbsoluteUri);

            return GetNextPageLink(request.RequestUri, request.GetQueryNameValuePairs(), pageSize);
        }

        internal static Uri GetNextPageLink(Uri requestUri, int pageSize)
        {
            Contract.Assert(requestUri != null);
            Contract.Assert(requestUri.IsAbsoluteUri);

            return GetNextPageLink(requestUri, new FormDataCollection(requestUri), pageSize);
        }

        internal static Uri GetNextPageLink(Uri requestUri, IEnumerable<KeyValuePair<string, string>> queryParameters, int pageSize)
        {
            Contract.Assert(requestUri != null);
            Contract.Assert(queryParameters != null);
            Contract.Assert(requestUri.IsAbsoluteUri);

            StringBuilder queryBuilder = new StringBuilder();

            int nextPageSkip = pageSize;

            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                string key = kvp.Key;
                string value = kvp.Value;
                switch (key)
                {
                    case "$top":
                        int top;
                        if (Int32.TryParse(value, out top))
                        {
                            // There is no next page if the $top query option's value is less than or equal to the page size.
                            Contract.Assert(top > pageSize);
                            // We decrease top by the pageSize because that's the number of results we're returning in the current page
                            value = (top - pageSize).ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    case "$skip":
                        int skip;
                        if (Int32.TryParse(value, out skip))
                        {
                            // We increase skip by the pageSize because that's the number of results we're returning in the current page
                            nextPageSkip += skip;
                        }
                        continue;
                    default:
                        break;
                }

                if (key.Length > 0 && key[0] == '$')
                {
                    // $ is a legal first character in query keys
                    key = '$' + Uri.EscapeDataString(key.Substring(1));
                }
                else
                {
                    key = Uri.EscapeDataString(key);
                }
                value = Uri.EscapeDataString(value);

                queryBuilder.Append(key);
                queryBuilder.Append('=');
                queryBuilder.Append(value);
                queryBuilder.Append('&');
            }

            queryBuilder.AppendFormat("$skip={0}", nextPageSkip);

            UriBuilder uriBuilder = new UriBuilder(requestUri)
            {
                Query = queryBuilder.ToString()
            };
            return uriBuilder.Uri;
        }

        internal virtual ETag GetETag(EntityTagHeaderValue etagHeaderValue)
        {
            return Request.GetETag(etagHeaderValue);
        }
    }
}
