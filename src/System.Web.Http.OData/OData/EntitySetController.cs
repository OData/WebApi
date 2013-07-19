// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Provides a convenient starting point for a controller that exposes an OData entity set. This is the synchronous version of <see cref="AsyncEntitySetController{TEntity, TKey}"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type associated with the exposed entity set's entity type.</typeparam>
    /// <typeparam name="TKey">The type associated with the entity key of the exposed entity set's entity type.</typeparam>
    [CLSCompliant(false)]
    [ODataNullValue]
    public abstract class EntitySetController<TEntity, TKey> : ODataController where TEntity : class
    {
        /// <summary>
        /// Gets the OData path of the current request.
        /// </summary>
        public ODataPath ODataPath
        {
            get
            {
                return EntitySetControllerHelpers.GetODataPath(this);
            }
        }

        /// <summary>
        /// Gets the OData query options of the current request.
        /// </summary>
        public ODataQueryOptions<TEntity> QueryOptions
        {
            get
            {
                return EntitySetControllerHelpers.CreateQueryOptions<TEntity>(this);
            }
        }

        /// <summary>
        /// This method should be overridden to handle GET requests that attempt to retrieve entities from the entity set.
        /// </summary>
        /// <returns>The matching entities from the entity set.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Needs to be this name to follow routing conventions.")]
        public virtual IQueryable<TEntity> Get()
        {
            throw EntitySetControllerHelpers.GetNotImplementedResponse(Request);
        }

        /// <summary>
        /// Handles GET requests that attempt to retrieve an individual entity by key from the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to retrieve.</param>
        /// <returns>The response message to send back to the client.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Needs to be this name to follow routing conventions.")]
        public virtual HttpResponseMessage Get([FromODataUri] TKey key)
        {
            TEntity entity = GetEntityByKey(key);
            return EntitySetControllerHelpers.GetByKeyResponse(Request, entity);
        }

        /// <summary>
        /// Handles POST requests that create new entities in the entity set.
        /// </summary>
        /// <param name="entity">The entity to insert into the entity set.</param>
        /// <returns>The response message to send back to the client.</returns>
        public virtual HttpResponseMessage Post([FromBody] TEntity entity)
        {
            TEntity createdEntity = CreateEntity(entity);
            TKey entityKey = GetKey(entity);
            return EntitySetControllerHelpers.PostResponse(this, createdEntity, entityKey);
        }

        /// <summary>
        /// Handles PUT requests that attempt to replace a single entity in the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to replace.</param>
        /// <param name="update">The updated entity.</param>
        /// <returns>The response message to send back to the client.</returns>
        public virtual HttpResponseMessage Put([FromODataUri] TKey key, [FromBody] TEntity update)
        {
            TEntity updatedEntity = UpdateEntity(key, update);
            return EntitySetControllerHelpers.PutResponse(Request, updatedEntity);
        }

        /// <summary>
        /// Handles PATCH and MERGE requests to partially update a single entity in the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to update.</param>
        /// <param name="patch">The patch representing the partial update.</param>
        /// <returns>The response message to send back to the client.</returns>
        [AcceptVerbs("PATCH", "MERGE")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "Patch is the action name by WebAPI convention.")]
        public virtual HttpResponseMessage Patch([FromODataUri] TKey key, Delta<TEntity> patch)
        {
            TEntity patchedEntity = PatchEntity(key, patch);
            return EntitySetControllerHelpers.PatchResponse(Request, patchedEntity);
        }

        /// <summary>
        /// This method should be overriden to handle DELETE requests for deleting existing entities from the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to delete.</param>
        public virtual void Delete([FromODataUri] TKey key)
        {
            throw EntitySetControllerHelpers.DeleteEntityNotImplementedResponse(Request);
        }

        /// <summary>
        /// This method should be overridden to handle POST and PUT requests that attempt to create a link between two entities.
        /// </summary>
        /// <param name="key">The key of the entity with the navigation property.</param>
        /// <param name="navigationProperty">The name of the navigation property.</param>
        /// <param name="link">The URI of the entity to link.</param>
        [AcceptVerbs("POST", "PUT")]
        public virtual void CreateLink([FromODataUri] TKey key, string navigationProperty, [FromBody] Uri link)
        {
            throw EntitySetControllerHelpers.CreateLinkNotImplementedResponse(Request, navigationProperty);
        }

        /// <summary>
        /// This method should be overridden to handle DELETE requests that attempt to break a relationship between two entities.
        /// </summary>
        /// <param name="key">The key of the entity with the navigation property.</param>
        /// <param name="navigationProperty">The name of the navigation property.</param>
        /// <param name="link">The URI of the entity to remove from the navigation property.</param>
        public virtual void DeleteLink([FromODataUri] TKey key, string navigationProperty, [FromBody] Uri link)
        {
            throw EntitySetControllerHelpers.DeleteLinkNotImplementedResponse(Request, navigationProperty);
        }

        /// <summary>
        /// This method should be overridden to handle DELETE requests that attempt to break a relationship between two entities.
        /// </summary>
        /// <param name="key">The key of the entity with the navigation property.</param>
        /// <param name="relatedKey">The key of the related entity.</param>
        /// <param name="navigationProperty">The name of the navigation property.</param>
        public virtual void DeleteLink([FromODataUri] TKey key, string relatedKey, string navigationProperty)
        {
            throw EntitySetControllerHelpers.DeleteLinkNotImplementedResponse(Request, navigationProperty);
        }

        /// <summary>
        /// This method should be overridden to handle all unmapped OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path of the request.</param>
        /// <returns>The response message to send back to the client.</returns>
        [AcceptVerbs("GET", "POST", "PUT", "PATCH", "MERGE", "DELETE")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly.")]
        public virtual HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
        {
            throw EntitySetControllerHelpers.UnmappedRequestResponse(Request, odataPath);
        }

        /// <summary>
        /// This method should be overridden to get the entity key of the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity key value</returns>
        protected internal virtual TKey GetKey(TEntity entity)
        {
            throw EntitySetControllerHelpers.GetKeyNotImplementedResponse(Request);
        }

        /// <summary>
        /// This method should be overridden to retrieve an entity by key from the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to retrieve.</param>
        /// <returns>The retrieved entity, or <c>null</c> if an entity with the specified entity key cannot be found in the entity set.</returns>
        protected internal virtual TEntity GetEntityByKey(TKey key)
        {
            throw EntitySetControllerHelpers.GetEntityByKeyNotImplementedResponse(Request);
        }

        /// <summary>
        /// This method should be overridden to create a new entity in the entity set.
        /// </summary>
        /// <remarks>When overriding this method, the GetKey method should also be overridden so that the location header can be generated.</remarks>
        /// <param name="entity">The entity to add to the entity set.</param>
        /// <returns>The created entity.</returns>
        protected internal virtual TEntity CreateEntity(TEntity entity)
        {
            throw EntitySetControllerHelpers.CreateEntityNotImplementedResponse(Request);
        }

        /// <summary>
        /// This method should be overridden to update an existing entity in the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to update.</param>
        /// <param name="update">The updated entity.</param>
        /// <returns>The updated entity.</returns>
        protected internal virtual TEntity UpdateEntity(TKey key, TEntity update)
        {
            throw EntitySetControllerHelpers.UpdateEntityNotImplementedResponse(Request);
        }

        /// <summary>
        /// This method should be overridden to apply a partial update to an existing entity in the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to update.</param>
        /// <param name="patch">The patch representing the partial update.</param>
        /// <returns>The updated entity.</returns>
        protected internal virtual TEntity PatchEntity(TKey key, Delta<TEntity> patch)
        {
            throw EntitySetControllerHelpers.PatchEntityNotImplementedResponse(Request);
        }
    }
}