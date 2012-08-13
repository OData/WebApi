// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition. 
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    [ODataQueryParameterBinding]
    public class ODataQueryOptions
    {
        private const string EntityFrameworkQueryProviderAssemblyName = "EntityFramework";
        private const string Linq2SqlQueryProviderAssemblyName = "System.Data.Linq";
        private const string Linq2ObjectsQueryProviderAssemblyName = "System.Core";

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

        public FilterQueryOption Filter { get; private set; }

        public OrderByQueryOption OrderBy { get; private set; }

        public SkipQueryOption Skip { get; private set; }

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
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            bool handleNullPropagation;

            string queryProviderAssemblyName = query.Provider.GetType().Assembly.GetName().Name;
            switch (queryProviderAssemblyName)
            {
                case EntityFrameworkQueryProviderAssemblyName:
                    handleNullPropagation = false;
                    break;

                case Linq2SqlQueryProviderAssemblyName:
                    handleNullPropagation = false;
                    break;

                case Linq2ObjectsQueryProviderAssemblyName:
                    handleNullPropagation = true;
                    break;

                default:
                    handleNullPropagation = true;
                    break;
            }

            return ApplyTo(query, handleNullPropagation);
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The IQueryable that we are applying query against.</param>
        /// <param name="handleNullPropagation">Specifies if we need to handle null propagation. Pass false if the underlying query provider handles null propagation. Otherwise pass true.</param>
        /// <returns>The query that the query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, bool handleNullPropagation)
        {
            return ApplyTo(query, handleNullPropagation, canUseDefaultOrderBy: true);
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The IQueryable that we are applying query against.</param>
        /// <param name="handleNullPropagation">Specifies if we need to handle null propagation. Pass false if the underlying query provider handles null propagation. Otherwise pass true.</param>
        /// <param name="canUseDefaultOrderBy">If a default ordering can be used if the query doesn't specify one and has a $skip or $top.</param>
        /// <returns>The query that the query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, bool handleNullPropagation, bool canUseDefaultOrderBy)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            IQueryable result = query;

            // Construct the actual query and apply them in the following order: filter, orderby, skip, top
            if (Filter != null)
            {
                result = Filter.ApplyTo(result, handleNullPropagation);
            }

            OrderByQueryOption orderBy = OrderBy;
            if (orderBy == null && (Skip != null || Top != null) && canUseDefaultOrderBy)
            {
                // Instead of failing early here if we cannot generate a default OrderBy,
                // let the IQueryable backend fail (if it has to).
                string orderByRaw = GenerateDefaultOrderBy(Context);
                if (!String.IsNullOrEmpty(orderByRaw))
                {
                    orderBy = new OrderByQueryOption(orderByRaw, Context);
                }
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

        private static void ThrowIfEmpty(string queryValue, string queryName)
        {
            if (String.IsNullOrWhiteSpace(queryValue))
            {
                throw new ODataException(Error.Format(SRResources.QueryCannotBeEmpty, queryName));
            }
        }

        private static string GenerateDefaultOrderBy(ODataQueryContext context)
        {
            Contract.Assert(context != null && context.EntitySet != null);

            IEdmEntityType entityType = context.EntitySet.ElementType;

            // choose the keys alphabetically. This would return a stable sort.
            string sortOrder = String.Join(",", entityType
                                                .Key()
                                                .OrderBy(property => property.Name)
                                                .Select(property => property.Name));
            if (String.IsNullOrEmpty(sortOrder))
            {
                // If there are no keys, choose the primitive properties alphabetically. This 
                // might not result in a stable sort especially if there are duplicates and is only
                // a best effort solution.
                sortOrder = String.Join(",", entityType
                            .StructuralProperties()
                            .Where(property => property.Type.IsPrimitive())
                            .OrderBy(property => property.Name)
                            .Select(property => property.Name));
            }

            return sortOrder;
        }
    }
}
