// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using System.Web.Http.Filters;
using System.Web.Http.OData.Query;

namespace System.Web.Http.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static void AddODataQueryFilter(this HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Services.Add(
                typeof(IFilterProvider),
                new QueryFilterProvider(new EnableQueryAttribute(), skipQueryableAttribute: true));
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
        /// http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public static void AddODataQueryFilter(this HttpConfiguration configuration, IActionFilter queryFilter)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            configuration.Services.Add(typeof(IFilterProvider), new QueryFilterProvider(queryFilter));
        }
    }
}
