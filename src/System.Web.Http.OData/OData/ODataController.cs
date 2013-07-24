// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Description;
using System.Web.Http.OData.Results;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Defines a base class for OData controllers that support writing and reading data using the OData formats.
    /// </summary>
    [ODataFormatting]
    [ODataRouting]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class ODataController : ApiController
    {
        /// <summary>
        /// Creates an action result with the specified values that is a response to a POST operation with an entity 
        /// to an entity set.
        /// </summary>
        /// <typeparam name="TEntity">The created entity type.</typeparam>
        /// <param name="entity">The created entity.</param>
        /// <returns>A <see cref="CreatedODataResult{TEntity}"/> with the specified values.</returns>
        protected virtual CreatedODataResult<TEntity> Created<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            return new CreatedODataResult<TEntity>(entity, this);
        }

        /// <summary>
        /// Creates an action result with the specified values that is a response to a PUT, PATCH, or a MERGE operation 
        /// on an OData entity.
        /// </summary>
        /// <typeparam name="TEntity">The updated entity type.</typeparam>
        /// <param name="entity">The updated entity.</param>
        /// <returns>An <see cref="UpdatedODataResult{TEntity}"/> with the specified values.</returns>
        protected virtual UpdatedODataResult<TEntity> Updated<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            return new UpdatedODataResult<TEntity>(entity, this);
        }
    }
}
