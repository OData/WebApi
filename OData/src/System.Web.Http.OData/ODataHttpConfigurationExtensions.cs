// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using Extensions = System.Web.Http.OData.Extensions;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataHttpConfigurationExtensions
    {
        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information,
        /// visit http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        [Obsolete("This method is obsolete; use the AddODataQueryFilter method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void EnableQuerySupport(this HttpConfiguration configuration)
        {
            Extensions.HttpConfigurationExtensions.AddODataQueryFilter(configuration);
        }

        /// <summary>
        /// Enables query support for actions with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return
        /// type. To avoid processing unexpected or malicious queries, use the validation settings on
        /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information,
        /// visit http://go.microsoft.com/fwlink/?LinkId=279712.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        [Obsolete("This method is obsolete; use the AddODataQueryFilter method from the " +
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static void EnableQuerySupport(this HttpConfiguration configuration, IActionFilter queryFilter)
        {
            Extensions.HttpConfigurationExtensions.AddODataQueryFilter(configuration, queryFilter);
        }
    }
}
