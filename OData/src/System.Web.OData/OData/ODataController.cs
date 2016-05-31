// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.OData.Extensions;
using System.Web.OData.Results;
using Microsoft.Extensions.DependencyInjection;

namespace System.Web.OData
{
    /// <summary>
    /// Defines a base class for OData controllers that support writing and reading data using the OData formats.
    /// </summary>
    [ODataFormatting]
    [ODataRouting]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class ODataController : ApiController
    {
        private IServiceScope requestScope;

        /// <summary>
        /// Initializes the System.Web.Http.ApiController instance with the specified controllerContext.
        /// </summary>
        /// <param name="controllerContext">
        /// The System.Web.Http.Controllers.HttpControllerContext object that is used for the initialization.
        /// </param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            IServiceProvider rootContainer = controllerContext.Configuration.GetRootContainer();
            this.requestScope = rootContainer.GetRequiredService<IServiceScopeFactory>().CreateScope();
            controllerContext.Request.ODataProperties().RequestContainer = this.requestScope.ServiceProvider;
        }

        /// <summary>
        /// Releases the unmanaged resources that are used by the object and, optionally,
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.requestScope.Dispose();
            }

            base.Dispose(disposing);
        }

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
