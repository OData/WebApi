// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// An implementation of <see cref="IFilterProvider" /> that applies an action filter to
    /// any action with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return type
    /// that doesn't bind a parameter of type <see cref="ODataQueryOptions" />.
    /// </summary>
    public class QueryFilterProvider : IFilterProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFilterProvider" /> class.
        /// </summary>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public QueryFilterProvider(IActionFilter queryFilter)
        {
            if (queryFilter == null)
            {
                throw Error.ArgumentNull("queryFilter");
            }

            QueryFilter = queryFilter;
        }

        /// <summary>
        /// Gets the action filter that executes the query.
        /// </summary>
        public IActionFilter QueryFilter { get; private set; }

        /// <summary>
        /// Provides filters to apply to the specified action.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="actionDescriptor">The action descriptor for the action to provide filters for.</param>
        /// <returns>
        /// The filters to apply to the specified action.
        /// </returns>
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            // Actions with a bound parameter of type ODataQueryOptions do not support the query filter
            // The assumption is that the action will handle the querying within the action implementation
            if (actionDescriptor != null &&
                (IsIQueryable(actionDescriptor.ReturnType) || typeof(SingleResult).IsAssignableFrom(actionDescriptor.ReturnType)) &&
                !actionDescriptor.GetParameters().Any(parameter => typeof(ODataQueryOptions).IsAssignableFrom(parameter.ParameterType)))
            {
                return new FilterInfo[] { new FilterInfo(QueryFilter, FilterScope.Global) };
            }

            return Enumerable.Empty<FilterInfo>();
        }

        internal static bool IsIQueryable(Type type)
        {
            return type == typeof(IQueryable) ||
                (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>));
        }
    }
}
