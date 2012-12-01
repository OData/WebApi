// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData
{
    [CLSCompliant(false)]
    public abstract class EntitySetController<TEntity, TKey> : ApiController where TEntity : class
    {
        private const string PreferHeaderName = "Prefer";
        private const string PreferenceAppliedHeaderName = "Preference-Applied";
        private const string ReturnContentHeaderValue = "return-content";
        private const string ReturnNoContentHeaderValue = "return-no-content";

        public ODataPath ODataPath
        {
            get
            {
                return Request.GetODataPath();
            }
        }

        public ODataQueryOptions<TEntity> QueryOptions
        {
            get
            {
                ODataQueryContext context = new ODataQueryContext(Configuration.GetEdmModel(), typeof(TEntity));
                return new ODataQueryOptions<TEntity>(context, Request);
            }
        }

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
                        ErrorCode = SRResources.EntitySetControllerUnsupportedGetErrorCode
                    }));
        }

        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get", Justification = "Needs to be this name to follow routing conventions.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage Get([FromUri] TKey key)
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage Put([FromUri] TKey key, [FromBody] TEntity update)
        {
            TEntity updated = UpdateEntity(key, update);
            if (RequestPrefersReturnContent())
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Accepted, updated);
                response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        [AcceptVerbs("PATCH", "MERGE")]
        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "1#", Justification = "Patch is the action name by WebAPI convention.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage Patch([FromUri] TKey key, [FromBody] Delta<TEntity> patch)
        {
            TEntity updated = PatchEntity(key, patch);

            if (RequestPrefersReturnContent())
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Accepted, updated);
                response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage Delete([FromUri] TKey key)
        {
            DeleteEntity(key);
            return Request.CreateResponse(HttpStatusCode.Accepted);
        }

        [AcceptVerbs("POST", "PUT")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage CreateLink([FromUri] TKey key, string navigationProperty, [FromBody] Uri link)
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage DeleteLink([FromUri] TKey key, string navigationProperty, [FromBody] Uri link)
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public virtual HttpResponseMessage DeleteLink([FromUri] TKey key, string relatedKey, string navigationProperty)
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected virtual TKey GetKey(TEntity entity)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedGetKey,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = SRResources.EntitySetControllerUnsupportedPostErrorCode
                    }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected virtual TEntity GetEntityByKey(TKey key)
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

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected virtual TEntity CreateEntity(TEntity entity)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedCreate,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = SRResources.EntitySetControllerUnsupportedPostErrorCode
                    }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected virtual TEntity UpdateEntity(TKey key, TEntity update)
        {
            throw new HttpResponseException(
                        Request.CreateResponse(
                            HttpStatusCode.NotImplemented,
                            new ODataError
                            {
                                Message = SRResources.EntitySetControllerUnsupportedUpdate,
                                MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                                ErrorCode = SRResources.EntitySetControllerUnsupportedUpdateErrorCode
                            }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected virtual TEntity PatchEntity(TKey key, Delta<TEntity> patch)
        {
            throw new HttpResponseException(
                    Request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = SRResources.EntitySetControllerUnsupportedPatch,
                            MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                            ErrorCode = SRResources.EntitySetControllerUnsupportedPatchErrorCode
                        }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        protected virtual void DeleteEntity(TKey key)
        {
            throw new HttpResponseException(
                Request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedDelete,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = SRResources.EntitySetControllerUnsupportedDeleteErrorCode
                    }));
        }

        protected bool RequestPrefersReturnContent()
        {
            IEnumerable<string> preferences = null;
            if (Request.Headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnContentHeaderValue);
            }
            return false;
        }

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