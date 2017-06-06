// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.OData.Properties;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// ETag of an entity type <typeparamref name="TEntity"/>.
    /// </summary>
    /// <typeparam name="TEntity">TEntity is the type of entity.</typeparam>
    public class ETag<TEntity> : ETag
    {
        /// <summary>
        /// Instantiates an instance of the <see cref="ETag{TEntity}"/> class.
        /// </summary>
        public ETag()
        {
            EntityType = typeof(TEntity);
        }

        /// <inheritdoc />
        public override IQueryable ApplyTo(IQueryable query)
        {
            ValidateQuery(query);
            return base.ApplyTo(query);
        }

        /// <summary>
        /// Apply the ETag to the given <see cref="IQueryable{T}"/>.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable{T}"/>.</param>
        /// <returns>The new <see cref="IQueryable{T}"/> after the ETag has been applied.</returns>
        public IQueryable<TEntity> ApplyTo(IQueryable<TEntity> query)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            return (IQueryable<TEntity>)base.ApplyTo(query);
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
