// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// An implementation of <see cref="IFilterProvider"/> that applies the <see cref="QueryableAttribute"/> to
    /// any action with an <see cref="IQueryable"/> or <see cref="IQueryable{T}"/> return type that doesn't bind
    /// a parameter of type <see cref="ODataQueryOptions"/>.
    /// </summary>
    public class QueryableFilterProvider : IFilterProvider
    {
        /// <summary>
        /// Gets or sets the maximum number of query results to return.
        /// </summary>
        /// <value>
        /// The maximum number of query results to return, or <c>null</c> if there is no limit.
        /// </value>
        public int? ResultLimit { get; set; }

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
            // Actions with a bound parameter of type ODataQueryOptions do not support the [Queryable] attribute
            // The assumption is that the action will handle the querying within the action implementation
            if (actionDescriptor != null && IsIQueryable(actionDescriptor.ReturnType) &&
                !actionDescriptor.GetParameters().Any(parameter => parameter.ParameterType == typeof(ODataQueryOptions)))
            {
                QueryableAttribute filter = new QueryableAttribute();
                if (ResultLimit.HasValue)
                {
                    filter.ResultLimit = ResultLimit.Value;
                }
                return new FilterInfo[] { new FilterInfo(filter, FilterScope.Global) };
            }

            return Enumerable.Empty<FilterInfo>();
        }

        private static bool IsIQueryable(Type type)
        {
            return type == typeof(IQueryable) ||
                (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>));
        }
    }
}
