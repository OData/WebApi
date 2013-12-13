// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// OData ETag of an entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">TEntity is the type of entity.</typeparam>
    public class ETag<TEntity> : ETag
    {
        /// <summary>
        /// Creates an instance of <see cref="ETag{TEntity}"/>.
        /// </summary>
        public ETag()
        {
            EntityType = typeof(TEntity);
        }

        /// <summary>
        /// Apply the ETag to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the ETag has been applied to.</returns>
        public override IQueryable ApplyTo(IQueryable query)
        {
            ValidateQuery(query);
            IQueryable<TEntity> queryOfTEntity = query as IQueryable<TEntity>;
            Contract.Assert(queryOfTEntity != null);
            return base.ApplyTo(queryOfTEntity);
        }

        private static void ValidateQuery(IQueryable query)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (!typeof(TEntity).IsAssignableFrom(query.ElementType))
            {
                throw Error.Argument(
                    "query",
                    SRResources.CannotApplyETagOfT,
                    typeof(ETag).Name,
                    typeof(TEntity).FullName,
                    typeof(IQueryable).Name,
                    query.ElementType.FullName);
            }
        }
    }
}
