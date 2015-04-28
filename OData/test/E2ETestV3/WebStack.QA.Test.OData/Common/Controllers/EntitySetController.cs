using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder.Conventions;
using Microsoft.Data.OData;

namespace WebStack.QA.Test.OData.Common.Controllers
{
    /// <summary>
    /// EntitySetController is a prototype of a convenient starting point for a controller that exposes an 
    /// OData entity set.
    /// 
    /// Key features:
    /// - overridable implementations of all the key operations on an EntitySet that return valid OData errors
    /// - plumbing to simplify dealing with standard OData'isms (for example picking the correct status codes).
    /// - protected inner methods that allow you to focus on the core common code you need to implement
    /// </summary>
    /// <remarks>
    /// Some variant of this base class (or something similar based on ActionFilters) will likely
    /// make it's way into System.Web.Http.OData.dll once the design has crystallized more.
    /// </remarks>
    /// <typeparam name="TEntity">The CLR type associated with the EntityType of the EntitySet</typeparam>
    /// <typeparam name="TKey">The CLR type of the EntityKey of the EntityType of the EntitySet.</typeparam>
    [ModelStateErrorHandling]
    public abstract class EntitySetController<TEntity, TKey> : ApiController where TEntity : class, new()
    {
        //TODO: there has to be a better way!!
        protected virtual string ControllerName
        {
            get
            {
                return this.GetType().Name.Replace("Controller", string.Empty);
            }
        }

        /// <summary>
        /// This method handles GET request that wish to retrieve entities from the EntitySet.
        /// It allows clients to use any of the following OData query options:
        ///     $filter
        ///     $orderby
        ///     $skip
        ///     $top
        /// <example>
        /// GET ~/ControllerName
        /// GET ~/ControllerName?$filter=Prop eq Value
        /// GET ~/ControllerName?$filter=Prop eq Value&$orderby=Prop2&$top=10&$skip=10 
        /// </example>
        /// </summary>
        /// <returns>The matching Entities. Until overridden this method responds with 501 Not Implemented</returns>
        [Queryable]
        public virtual IQueryable<TEntity> Get()
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = string.Format("GET '{0}' requests are not supported.", typeof(TEntity).FullName),
                        MessageLanguage = "en-US",
                        ErrorCode = "GET requests not supported."
                    }));
        }

        /// <summary>
        /// This method handles GET requests that try to retrieve a single entity by it's key
        /// <Example>
        /// GET ~/ControllerName(key)
        /// </Example>
        /// <remarks>
        /// Typically you don't need to override this method.
        /// To add support for Getting entity by key you should override GetEntityById(..).
        /// </remarks>
        /// </summary>
        /// <param name="id">Key value of the Entity to retrieve</param>
        /// <returns>The matching Entity.</returns>
        public virtual HttpResponseMessage GetById([FromUri]TKey id)
        {
            TEntity entity = GetEntityById(id);
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
        /// This method handles POST request that create new entities in the EntitySet.
        /// <remarks>
        /// Typically you don't need to override this method.
        /// To add support for creating new Entities you should override CreateEntity(..).
        /// </remarks>
        /// </summary>
        /// <param name="entity">The entity to insert</param>
        public virtual HttpResponseMessage Post(TEntity entity)
        {
            entity = CreateEntity(entity);

            HttpResponseMessage response = null;
            if (Request.WantsResponseToExcludeCreatedEntity())
            {
                response = Request.CreateResponse(HttpStatusCode.NoContent);
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.Created, entity);
            }

            response.Headers.Location = new Uri(Url.Link(ODataRouteNames.GetById, new { Controller = ControllerName, Id = GetId(entity) }));
            return response;
        }

        /// <summary>
        /// This method handles DELETE requests that delete existing entities from the EntitySet
        /// <remarks>
        /// Typically you don't need to override this method.
        /// To add support for deleting existing Entities you should override DeleteEntity(..).
        /// </remarks>
        /// </summary>
        /// <param name="id">The key of the entity you wish to delete.</param>
        public virtual HttpResponseMessage Delete([FromUri]TKey id)
        {
            DeleteEntity(id);
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// This method handles PATCH requests to partially update a single entity in the EntitySet
        /// </summary>
        /// <remarks>
        /// Typically you don't need to override this method.
        /// To add support for patching existing Entities you should override PatchEntity(..). 
        /// </remarks>
        /// <param name="id">The key of the entity that is being patched</param>
        /// <param name="patch">A structure that represents partial update</param>
        [AcceptVerbs("PATCH", "MERGE")]
        public virtual HttpResponseMessage Patch([FromUri]TKey id, Delta<TEntity> patch)
        {
            TEntity updated = PatchEntity(id, patch);
            if (Request.WantsResponseToIncludeUpdatedEntity())
            {
                return Request.CreateResponse(HttpStatusCode.Accepted, updated);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// This method handles PUT requests that attempt to replace a single entity in the EntitySet.
        /// </summary>
        /// <remarks>
        /// Typically you don't need to override this method, instead
        /// to add support for replacing existing Entities you should override UpdateEntity(..).
        /// </remarks>
        /// <param name="id">The key of the entity that is being patched</param>
        /// <param name="update">The new version of the entity</param>
        public virtual HttpResponseMessage Put([FromUri]TKey id, TEntity update)
        {
            TEntity updated = UpdateEntity(id, update);
            if (Request.WantsResponseToIncludeUpdatedEntity())
            {
                return Request.CreateResponse(HttpStatusCode.Accepted, updated);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// The method handles POST and PUT requests that attempt to create a link between two entities
        /// POST requests are used when the multiplicity of the navigationProperty is *
        /// PUT requests are used when the multiplicity of the navigationProperty is 1 or 0..1
        /// <example>
        /// This requests attempts to add the entity represented by http://server/service/AnotherController(key)
        /// to the "NavigationProperty" collection of the entity represented by http://server/service/Controller(key)
        /// 
        /// POST ~/Controller(key)/$links/NavigationProperty
        /// Content-Type: application/json;odata=verbose
        /// 
        /// {
        ///    "uri": "http://server/service/AnotherController(key)"
        /// }   
        /// </example>
        /// </summary>
        /// <param name="id">The key of the entity with the navigationProperty</param>
        /// <param name="navigationProperty">The name of the navigationProperty to be modified</param>
        /// <param name="link">The url of the entity to link</param>
        /// <returns>Until overridden this method will respond with 501 Not Implemented</returns>
        [AcceptVerbs("POST", "PUT")]
        public virtual HttpResponseMessage CreateLink([FromUri]TKey id, string navigationProperty, [FromBody] Uri link)
        {
            throw ODataErrors.CreatingLinkNotSupported(Request, navigationProperty);
        }

        /// <summary>
        /// The method handles DELETE requests that attempt to break the "navigationProperty" relationship between
        /// two entities.
        /// </summary>
        /// <param name="id">The key of the entity with the navigationProperty</param>
        /// <param name="navigationProperty">The name of the navigationProperty to be modified</param>
        /// <param name="link">The url of the entity to remove from the navigationProperty. 
        /// <remarks>
        /// For singleton navigationProperties this will be NULL because you are simply setting the navigationProperty to null (so you don't need url of the previously related entity).
        /// </remarks>
        /// </param>
        /// <returns>Until overridden this method will respond with 501 Not Implemented</returns>
        public virtual HttpResponseMessage DeleteLink([FromUri]TKey id, string navigationProperty, [FromBody] Uri link)
        {
            throw ODataErrors.DeletingLinkNotSupported(Request, navigationProperty);
        }

        public virtual HttpResponseMessage DeleteCollectionLink(TKey id, string navigationProperty, string navigationId)
        {
            throw ODataErrors.DeletingLinkNotSupported(Request, navigationProperty);
        }

        /// <summary>
        /// This method should be overridden to actually support fetching (GET) a single Entity by key.
        /// </summary>
        /// <remarks>
        /// Until overridden GET by key requests will result in a 501 Not Implemented response.
        /// </remarks>
        /// <param name="id">The key of the entity to retrieve</param>
        /// <returns>
        /// The matching entity or null. 
        /// </returns>
        protected virtual TEntity GetEntityById(TKey id)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = string.Format("GET '{0}' requests by key are not supported.", typeof(TEntity).FullName),
                        MessageLanguage = "en-US",
                        ErrorCode = "GET requests by key are not supported."
                    }));
        }

        /// <summary>
        /// This method should be overridden to actually support POST (or creating) entities in the EntitySet.
        /// </summary>
        /// <remarks>
        /// Until overridden POST requests will result in a 501 Not Implemented response.
        /// </remarks>
        /// <param name="entity">The entity to create</param>
        /// <returns>
        /// The entity after it has been created, i.e. with any server generated values included. 
        /// </returns>
        protected virtual TEntity CreateEntity(TEntity entity)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = string.Format("Creating '{0}' entities is not currently supported.", typeof(TEntity).FullName),
                        MessageLanguage = "en-US",
                        ErrorCode = "POST requests are not supported."
                    }));
        }

        /// <summary>
        /// This method should be overridden to support DELETE (or deleting) entities from the EntitySet.
        /// </summary>
        /// <remarks>
        /// Until overridden DELETE requests will result in a 501 Not Implemented response.
        /// </remarks>
        /// <param name="id">The key of the Entity to be deleted</param>
        protected virtual void DeleteEntity(TKey id)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = string.Format("Deleting '{0}' entities is not currently supported.", typeof(TEntity).FullName),
                        MessageLanguage = "en-US",
                        ErrorCode = "DELETE requests are not supported."
                    }));
        }

        /// <summary>
        /// This method must be overridden or self-link generation won't work,
        /// meaning no entities in this entity set can be returned from GET requests.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The key value</returns>
        protected virtual TKey GetId(TEntity entity)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = string.Format("Extracting the key from '{0}' is not currently supported.", typeof(TEntity).FullName),
                        MessageLanguage = "en-US",
                        ErrorCode = "POST requests not supported."
                    }));
        }

        /// <summary>
        /// This method should be overridden to support PATCH (or partial updates) to entities
        /// </summary>
        /// <param name="id">The key of the Entity to be patched</param>
        /// <param name="patch">The delta holding the changed to be applied</param>
        /// <returns>The updated Entity</returns>
        protected virtual TEntity PatchEntity(TKey id, Delta<TEntity> patch)
        {
            throw new HttpResponseException(
                    Request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = string.Format("Patching '{0}' entities is not currently supported.", typeof(TEntity).FullName),
                            MessageLanguage = "en-US",
                            ErrorCode = "PATCH requests are not supported."
                        }));
        }

        /// <summary>
        /// This method should be overridden to support PUT (or replace)
        /// </summary>
        /// <param name="id">The key of the Entity to be replaced</param>
        /// <param name="update">The entity containing all the values to be used in the replace</param>
        /// <returns>The updated entity</returns>
        protected virtual TEntity UpdateEntity(TKey id, TEntity update)
        {
            throw new HttpResponseException(
                        Request.CreateResponse(
                            HttpStatusCode.NotImplemented,
                            new ODataError
                            {
                                Message = string.Format("Replacing '{0}' entities is not currently supported.", typeof(TEntity).FullName),
                                MessageLanguage = "en-US",
                                ErrorCode = "PUT requests are not supported."
                            }));
        }
    }
}
