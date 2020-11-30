// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Defines a base class for OData controllers that support writing and reading data using the OData formats.
    /// </summary>
    /// <remarks>These attributes and signature uses types that are AspNetCore-specific.</remarks>
    [ODataFormatting]
    [ODataRouting]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract partial class ODataController : ControllerBase
    {
        /// <summary>
        /// Creates an action result with the specified values that is a response to a POST operation with an entity
        /// to an entity set.
        /// </summary>
        /// <typeparam name="TEntity">The created entity type.</typeparam>
        /// <param name="entity">The created entity.</param>
        /// <returns>A <see cref="CreatedODataResult{TEntity}"/> with the specified values.</returns>
        /// <remarks>These function uses types that are AspNetCore-specific.</remarks>
        protected virtual CreatedODataResult<TEntity> Created<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            return new CreatedODataResult<TEntity>(entity);
        }

        /// <summary>
        /// Creates an action result with the specified values that is a response to a PUT, PATCH, or a MERGE operation
        /// on an OData entity.
        /// </summary>
        /// <typeparam name="TEntity">The updated entity type.</typeparam>
        /// <param name="entity">The updated entity.</param>
        /// <returns>An <see cref="UpdatedODataResult{TEntity}"/> with the specified values.</returns>
        /// <remarks>These function uses types that are AspNetCore-specific.</remarks>
        protected virtual UpdatedODataResult<TEntity> Updated<TEntity>(TEntity entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            return new UpdatedODataResult<TEntity>(entity);
        }

        /// <summary>
        /// Creates a <see cref="StatusCodeResult"/> that when executed will produce a Bad Request (400) response.
        /// </summary>
        /// <returns>A <see cref="BadRequestResult"/> with the specified values.</returns>
        public override BadRequestResult BadRequest()
        {
            return new BadRequestODataResult("Bad request");
        }

        /// <summary>
        /// Creates a <see cref="StatusCodeResult"/> that when executed will produce a Bad Request (400) response.
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <returns>A <see cref="BadRequestODataResult"/> with the specified values.</returns>
        protected virtual BadRequestODataResult BadRequest(string message)
        {
            return new BadRequestODataResult(message);
        }

        /// <summary>
        /// Creates a <see cref="StatusCodeResult"/> that when executed will produce a Not Found (404) response.
        /// </summary>
        /// <returns>A <see cref="NotFoundResult"/> with the specified values.</returns>
        public override NotFoundResult NotFound()
        {
            return new NotFoundODataResult("Not found");
        }

        /// <summary>
        /// Creates a <see cref="StatusCodeResult"/> that when executed will produce a Not Found (404) response.
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <returns>A <see cref="NotFoundODataResult"/> with the specified values.</returns>
        protected virtual NotFoundODataResult NotFound(string message)
        {
            return new NotFoundODataResult(message);
        }

        /// <summary>
        /// Creates a <see cref="StatusCodeResult"/> that when executed will produce a Unauthorized (401) response.
        /// </summary>
        /// <returns>A <see cref="UnauthorizedODataResult"/> with the specified values.</returns>
        public override UnauthorizedResult Unauthorized()
        {
            return new UnauthorizedODataResult("Unauthorized");
        }

        /// <summary>
        /// Creates a <see cref="StatusCodeResult"/> that when executed will produce a Unauthorized (401) response.
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <returns>A <see cref="UnauthorizedODataResult"/> with the specified values.</returns>
        protected virtual UnauthorizedODataResult Unauthorized(string message)
        {
            return new UnauthorizedODataResult(message);
        }
    }
}
