// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a composite OData query options that can be used to perform query composition.
    /// Currently this only supports $filter, $orderby, $top, $skip.
    /// </summary>
    [ODataQueryParameterBinding]
    public partial class ODataQueryOptions<TEntity> : ODataQueryOptions
    {
        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query)
        {
            ValidateQuery(query);
            return base.ApplyTo(query);
        }

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The settings to use in query composition.</param>
        /// <returns>The new <see cref="IQueryable"/> after the query has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings)
        {
            ValidateQuery(query);
            return base.ApplyTo(query, querySettings);
        }

        private static void ValidateQuery(IQueryable query)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (!TypeHelper.IsTypeAssignableFrom(typeof(TEntity), query.ElementType))
            {
                throw Error.Argument("query", SRResources.CannotApplyODataQueryOptionsOfT, typeof(ODataQueryOptions).Name, typeof(TEntity).FullName, typeof(IQueryable).Name, query.ElementType.FullName);
            }
        }
    }
}
