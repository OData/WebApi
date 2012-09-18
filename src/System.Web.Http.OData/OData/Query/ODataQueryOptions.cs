// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition. 
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    [ODataQueryParameterBinding]
    public class ODataQueryOptions
    {
        private const string EntityFrameworkQueryProviderNamespace = "System.Data.Entity.Internal.Linq";
        private const string Linq2SqlQueryProviderNamespace = "System.Data.Linq";
        private const string Linq2ObjectsQueryProviderNamespace = "System.Linq";

        private IAssembliesResolver _assembliesResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryOptions"/> class based on the incoming request and some metadata information from 
        /// the <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="request">The incoming request message</param>
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

            // remember the context
            Context = context;

            // Parse the query from request Uri
            RawValues = new ODataRawQueryOptions();
            IEnumerable<KeyValuePair<string, string>> queryParameters = request.GetQueryNameValuePairs();
            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                switch (kvp.Key)
                {
                    case "$filter":
                        RawValues.Filter = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$filter");
                        Filter = new FilterQueryOption(kvp.Value, context);
                        break;
                    case "$orderby":
                        RawValues.OrderBy = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$orderby");
                        OrderBy = new OrderByQueryOption(kvp.Value, context);
                        break;
                    case "$top":
                        RawValues.Top = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$top");
                        Top = new TopQueryOption(kvp.Value, context);
                        break;
                    case "$skip":
                        RawValues.Skip = kvp.Value;
                        ThrowIfEmpty(kvp.Value, "$skip");
                        Skip = new SkipQueryOption(kvp.Value, context);
                        break;
                    case "$select":
                        RawValues.Select = kvp.Value;
                        break;
                    case "$inlinecount":
                        RawValues.InlineCount = kvp.Value;
                        break;
                    case "$expand":
                        RawValues.Expand = kvp.Value;
                        break;
                    case "$skiptoken":
                        RawValues.SkipToken = kvp.Value;
                        break;
                    default:
                        // we don't throw if we can't recognize the query
                        break;
                }
            }
        }

        /// <summary>
        ///  Gets the given <see cref="ODataQueryContext"/>
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the raw string of all the OData query options
        /// </summary>
        public ODataRawQueryOptions RawValues { get; private set; }

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
        /// Check if the given query is supported by the built in ODataQueryOptions.
        /// </summary>
        /// <param name="queryName">The name of the given query parameter.</param>
        /// <returns>returns true if the query parameter is one of the four that we support out of box.</returns>
        public static bool IsSupported(string queryName)
        {
            return (queryName == "$orderby" ||
                 queryName == "$filter" ||
                 queryName == "$top" ||
                 queryName == "$skip");
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The IQueryable that we are applying query against.</param>
        /// <returns>The query that the query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query)
        {
            return ApplyTo(query, new ODataQuerySettings());
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The IQueryable that we are applying query against.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The query that the query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (querySettings == null)
            {
                throw Error.ArgumentNull("querySettings");
            }

            // Ensure we have decided how to handle null propagation
            if (querySettings.HandleNullPropagation == HandleNullPropagationOption.Default)
            {
                querySettings.HandleNullPropagation = GetDefaultHandleNullPropagationOption(query);
            }

            IQueryable result = query;

            // Construct the actual query and apply them in the following order: filter, orderby, skip, top
            if (Filter != null)
            {
                result = Filter.ApplyTo(result, querySettings, _assembliesResolver);
            }

            OrderByQueryOption orderBy = OrderBy;

            // $skip or $top require a stable sort for predictable results.
            // If either is present in the query and we have permission,
            // generate an $orderby that will produce a stable sort.
            if ((Skip != null || Top != null) && querySettings.EnsureStableOrdering && !Context.IsPrimitiveClrType)
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
                result = orderBy.ApplyTo(result);
            }

            if (Skip != null)
            {
                result = Skip.ApplyTo(result);
            }

            if (Top != null)
            {
                result = Top.ApplyTo(result);
            }

            return result;
        }

        private static HandleNullPropagationOption GetDefaultHandleNullPropagationOption(IQueryable query)
        {
            Contract.Assert(query != null);

            HandleNullPropagationOption options;

            string queryProviderNamespace = query.Provider.GetType().Namespace;
            switch (queryProviderNamespace)
            {
                case EntityFrameworkQueryProviderNamespace:
                    options = HandleNullPropagationOption.False;
                    break;

                case Linq2SqlQueryProviderNamespace:
                    options = HandleNullPropagationOption.False;
                    break;

                case Linq2ObjectsQueryProviderNamespace:
                    options = HandleNullPropagationOption.True;
                    break;

                default:
                    options = HandleNullPropagationOption.True;
                    break;
            }

            return options;
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
            Contract.Assert(context != null && context.EntitySet != null);

            IEdmEntityType entityType = context.EntitySet.ElementType;
            IEnumerable<IEdmStructuralProperty> properties = 
                entityType.Key().Any()
                    ? entityType.Key()
                    : entityType
                        .StructuralProperties()
                        .Where(property => property.Type.IsPrimitive());

            // Sort properties alphabetically for stable sort
            return properties.OrderBy(property => property.Name);
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
            Contract.Assert(context != null && context.EntitySet != null);

            // Strategy: create a hash of all properties already used in the given OrderBy
            // and remove them from the list of properties we need to add to make the sort stable.
            HashSet<string> usedPropertyNames = 
                new HashSet<string>(
                    orderBy.PropertyNodes
                        .Select<OrderByPropertyNode, string>(node => node.Property.Name));

            IEnumerable<IEdmStructuralProperty> propertiesToAdd = 
                GetAvailableOrderByProperties(context)
                    .Where(prop => !usedPropertyNames.Contains(prop.Name));

            if (propertiesToAdd.Any())
            {
                // The existing query options has too few properties to create a stable sort.
                // Clone the given one and add the remaining properties to end, thereby making
                // the sort stable but preserving the user's original intent for the major
                // sort order.
                orderBy = new OrderByQueryOption(orderBy.RawValue, context);
                foreach (IEdmStructuralProperty property in propertiesToAdd)
                {
                    orderBy.PropertyNodes.Add(new OrderByPropertyNode(property, OrderByDirection.Ascending));
                }
            }

            return orderBy;
        }
    }
}
