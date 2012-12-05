// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Provides a convenient starting point for a controller that exposes an OData entity set.
    /// </summary>
    /// <typeparam name="TEntity">The type associated with the exposed entity set's entity type.</typeparam>
    /// <typeparam name="TKey">The type associated with the entity key of the exposed entity set's entity type.</typeparam>
    [CLSCompliant(false)]
    public abstract class EntitySetController<TEntity, TKey> : ApiController where TEntity : class
    {
        private const string PreferHeaderName = "Prefer";
        private const string PreferenceAppliedHeaderName = "Preference-Applied";
        private const string ReturnContentHeaderValue = "return-content";
        private const string ReturnNoContentHeaderValue = "return-no-content";

        /// <summary>
        /// Gets the OData path of the current request.
        /// </summary>
        public ODataPath ODataPath
        {
            get
            {
                return Request.GetODataPath();
            }
        }

        /// <summary>
        /// Gets the OData query options of the current request.
        /// </summary>
        public ODataQueryOptions<TEntity> QueryOptions
        {
            get
            {
                ODataQueryContext context = new ODataQueryContext(Configuration.GetEdmModel(), typeof(TEntity));
                return new ODataQueryOptions<TEntity>(context, Request);
            }
        }

        /// <summary>
        /// This method should be overriden to handle GET requests that attempt to retrieve entities from the entity set.
        /// </summary>
        /// <returns>The matching entities from the entity set.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Needs to be this name to follow routing conventions.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual IQueryable<TEntity> Get()
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedGet,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "GET")
                    }));
        }

        /// <summary>
        /// Handles GET requests that attempt to retrieve an individual entity by key from the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to retrieve.</param>
        /// <returns>The response message to send back to the client.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Needs to be this name to follow routing conventions.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage Get([FromODataUri] TKey key)
        {
            TEntity entity = GetEntityByKey(key);
            if (entity == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.OK, entity);
            }
        }

        /// <summary>
        /// Handles POST requests that create new entities in the entity set.
        /// </summary>
        /// <param name="entity">The entity to insert into the entity set.</param>
        /// <returns>The response message to send back to the client.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage Post([FromBody] TEntity entity)
        {
            entity = CreateEntity(entity);

            HttpResponseMessage response = null;
            if (RequestPrefersReturnNoContent())
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent);
                response.Headers.Add(PreferenceAppliedHeaderName, ReturnNoContentHeaderValue);
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.Created, entity);
            }

            string controllerName = ControllerContext.ControllerDescriptor.ControllerName;
            response.Headers.Location = new Uri(Url.ODataLink(
                                                    Configuration.GetODataPathHandler(),
                                                    new EntitySetPathSegment(controllerName),
                                                    new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(GetKey(entity), ODataVersion.V3))));
            return response;
        }

        /// <summary>
        /// Handles PUT requests that attempt to replace a single entity in the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to replace.</param>
        /// <param name="update">The updated entity.</param>
        /// <returns>The response message to send back to the client.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage Put([FromODataUri] TKey key, [FromBody] TEntity update)
        {
            TEntity updated = UpdateEntity(key, update);
            if (RequestPrefersReturnContent())
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, updated);
                response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Handles PATCH and MERGE requests to partially update a single entity in the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to update.</param>
        /// <param name="patch">The patch representing the partial update.</param>
        /// <returns>The response message to send back to the client.</returns>
        [AcceptVerbs("PATCH", "MERGE")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "Patch is the action name by WebAPI convention.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage Patch([FromODataUri] TKey key, Delta<TEntity> patch)
        {
            TEntity updated = PatchEntity(key, patch);

            if (RequestPrefersReturnContent())
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, updated);
                response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Handles DELETE requests for deleting existing entities from the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to delete.</param>
        /// <returns>The response message to send back to the client.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual void Delete([FromODataUri] TKey key)
        {
            DeleteEntity(key);
        }

        /// <summary>
        /// This method should be overriden to handle POST and PUT requests that attempt to create a link between two entities.
        /// </summary>
        /// <param name="key">The key of the entity with the navigation property.</param>
        /// <param name="navigationProperty">The name of the navigation property.</param>
        /// <param name="link">The URI of the entity to link.</param>
        [AcceptVerbs("POST", "PUT")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual void CreateLink([FromODataUri] TKey key, string navigationProperty, [FromBody] Uri link)
        {
            throw new HttpResponseException(
                    Request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = Error.Format(SRResources.EntitySetControllerUnsupportedCreateLink, navigationProperty),
                            MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                            ErrorCode = SRResources.EntitySetControllerUnsupportedCreateLinkErrorCode
                        }));
        }

        /// <summary>
        /// This method should be overriden to handle DELETE requests that attempt to break a relationship between two entities.
        /// </summary>
        /// <param name="key">The key of the entity with the navigation property.</param>
        /// <param name="navigationProperty">The name of the navigation property.</param>
        /// <param name="link">The URI of the entity to remove from the navigation property.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual void DeleteLink([FromODataUri] TKey key, string navigationProperty, [FromBody] Uri link)
        {
            throw new HttpResponseException(
                    Request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = Error.Format(SRResources.EntitySetControllerUnsupportedDeleteLink, navigationProperty),
                            MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                            ErrorCode = SRResources.EntitySetControllerUnsupportedDeleteLinkErrorCode
                        }));
        }

        /// <summary>
        /// This method should be overriden to handle DELETE requests that attempt to break a relationship between two entities.
        /// </summary>
        /// <param name="key">The key of the entity with the navigation property.</param>
        /// <param name="relatedKey">The key of the related entity.</param>
        /// <param name="navigationProperty">The name of the navigation property.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual void DeleteLink([FromODataUri] TKey key, string relatedKey, string navigationProperty)
        {
            throw new HttpResponseException(
                    Request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = Error.Format(SRResources.EntitySetControllerUnsupportedDeleteLink, navigationProperty),
                            MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                            ErrorCode = SRResources.EntitySetControllerUnsupportedDeleteLinkErrorCode
                        }));
        }

        /// <summary>
        /// This method should be overriden to handle all unmapped OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path of the request.</param>
        /// <returns>The response message to send back to the client.</returns>
        [AcceptVerbs("GET", "POST", "PUT", "PATCH", "MERGE", "DELETE")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage HandleUnmappedRequest(ODataPath odataPath)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = Error.Format(SRResources.EntitySetControllerUnmappedRequest, odataPath.PathTemplate),
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = SRResources.EntitySetControllerUnmappedRequestErrorCode
                    }));
        }

        /// <summary>
        /// This method should be overridden to get the entity key of the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity key value</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected internal virtual TKey GetKey(TEntity entity)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedGetKey,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "POST")
                    }));
        }

        /// <summary>
        /// This method should be overridden to retrieve an entity by key from the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to retrieve.</param>
        /// <returns>The retrieved entity, or <c>null</c> if an entity with the specified entity key cannot be found in the entity set.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected internal virtual TEntity GetEntityByKey(TKey key)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedGetByKey,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = SRResources.EntitySetControllerUnsupportedGetByKeyErrorCode
                    }));
        }

        /// <summary>
        /// This method should be overriden to create a new entity in the entity set.
        /// </summary>
        /// <param name="entity">The entity to add to the entity set.</param>
        /// <returns>The created entity.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected internal virtual TEntity CreateEntity(TEntity entity)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedCreate,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "POST")
                    }));
        }

        /// <summary>
        /// This method should be overriden to update an existing entity in the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to update.</param>
        /// <param name="update">The updated entity.</param>
        /// <returns>The updated entity.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected internal virtual TEntity UpdateEntity(TKey key, TEntity update)
        {
            throw new HttpResponseException(
                        Request.CreateResponse(
                            HttpStatusCode.NotImplemented,
                            new ODataError
                            {
                                Message = SRResources.EntitySetControllerUnsupportedUpdate,
                                MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                                ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "PUT")
                            }));
        }

        /// <summary>
        /// This method should be overriden to apply a partial update to an existing entity in the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to update.</param>
        /// <param name="patch">The patch representing the partial update.</param>
        /// <returns>The updated entity.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected internal virtual TEntity PatchEntity(TKey key, Delta<TEntity> patch)
        {
            throw new HttpResponseException(
                    Request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = SRResources.EntitySetControllerUnsupportedPatch,
                            MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                            ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "PATCH")
                        }));
        }

        /// <summary>
        /// This method should be overriden to delete an existing entity from the entity set.
        /// </summary>
        /// <param name="key">The entity key of the entity to delete.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected internal virtual void DeleteEntity(TKey key)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedDelete,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "DELETE")
                    }));
        }

        /// <summary>
        /// Returns whether or not the request prefers content to be returned.
        /// </summary>
        /// <returns><c>true</c> if the request has a Prefer header value for "return-content", <c>false</c> otherwise</returns>
        protected bool RequestPrefersReturnContent()
        {
            IEnumerable<string> preferences = null;
            if (Request.Headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnContentHeaderValue);
            }
            return false;
        }

        /// <summary>
        /// Returns whether or not the request prefers no content to be returned.
        /// </summary>
        /// <returns><c>true</c> if the request has a Prefer header value for "return-no-content", <c>false</c> otherwise</returns>
        protected bool RequestPrefersReturnNoContent()
        {
            IEnumerable<string> preferences = null;
            if (Request.Headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnNoContentHeaderValue);
            }
            return false;
        }
    }
}