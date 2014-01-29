// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Query;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Helper class for <see cref="EntitySetController{TEntity, TKey}"/> and <see cref="AsyncEntitySetController{TEntity, TKey}"/> that contains shared logic.
    /// </summary>
    internal static class EntitySetControllerHelpers
    {
        private const string PreferHeaderName = "Prefer";
        private const string PreferenceAppliedHeaderName = "Preference-Applied";
        private const string ReturnContentHeaderValue = "return-content";
        private const string ReturnNoContentHeaderValue = "return-no-content";

        public static ODataPath GetODataPath(ApiController controller)
        {
            return controller.Request.ODataProperties().Path;
        }

        public static ODataQueryOptions<TEntity> CreateQueryOptions<TEntity>(ApiController controller)
        {
            ODataQueryContext context = new ODataQueryContext(controller.Request.ODataProperties().Model, typeof(TEntity));
            return new ODataQueryOptions<TEntity>(context, controller.Request);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException GetNotImplementedResponse(HttpRequestMessage request)
        {
            return new HttpResponseException(
                request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedGet,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "GET")
                    }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseMessage GetByKeyResponse<TEntity>(HttpRequestMessage request, TEntity entity)
        {
            if (entity == null)
            {
                return request.CreateResponse(HttpStatusCode.NotFound);
            }
            else
            {
                return request.CreateResponse(HttpStatusCode.OK, entity);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseMessage PostResponse<TEntity, TKey>(ApiController controller, TEntity createdEntity, TKey entityKey)
        {
            HttpResponseMessage response = null;
            HttpRequestMessage request = controller.Request;
            if (RequestPrefersReturnNoContent(request))
            {
                response = request.CreateResponse(HttpStatusCode.NoContent);
                response.Headers.Add(PreferenceAppliedHeaderName, ReturnNoContentHeaderValue);
            }
            else
            {
                response = request.CreateResponse(HttpStatusCode.Created, createdEntity);
            }

            ODataPath odataPath = request.ODataProperties().Path;
            if (odataPath == null)
            {
                throw Error.InvalidOperation(SRResources.LocationHeaderMissingODataPath);
            }

            EntitySetPathSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetPathSegment;
            if (entitySetSegment == null)
            {
                throw Error.InvalidOperation(SRResources.LocationHeaderDoesNotStartWithEntitySet);
            }

            UrlHelper urlHelper = controller.Url ?? new UrlHelper(request);
            response.Headers.Location = new Uri(urlHelper.CreateODataLink(entitySetSegment,
                new KeyValuePathSegment(ODataUriUtils.ConvertToUriLiteral(entityKey, ODataVersion.V3))));
            return response;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseMessage PutResponse<TEntity>(HttpRequestMessage request, TEntity updatedEntity)
        {
            if (RequestPrefersReturnContent(request))
            {
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, updatedEntity);
                response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
                return response;
            }
            else
            {
                return request.CreateResponse(HttpStatusCode.NoContent);
            }
        }
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseMessage PatchResponse<TEntity>(HttpRequestMessage request, TEntity patchedEntity)
        {
            if (RequestPrefersReturnContent(request))
            {
                HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK, patchedEntity);
                response.Headers.Add(PreferenceAppliedHeaderName, ReturnContentHeaderValue);
                return response;
            }
            else
            {
                return request.CreateResponse(HttpStatusCode.NoContent);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException CreateLinkNotImplementedResponse(HttpRequestMessage request, string navigationProperty)
        {
            return new HttpResponseException(
                    request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = Error.Format(SRResources.EntitySetControllerUnsupportedCreateLink, navigationProperty),
                            MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                            ErrorCode = SRResources.EntitySetControllerUnsupportedCreateLinkErrorCode
                        }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException DeleteLinkNotImplementedResponse(HttpRequestMessage request, string navigationProperty)
        {
            return new HttpResponseException(
                    request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = Error.Format(SRResources.EntitySetControllerUnsupportedDeleteLink, navigationProperty),
                            MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                            ErrorCode = SRResources.EntitySetControllerUnsupportedDeleteLinkErrorCode
                        }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException UnmappedRequestResponse(HttpRequestMessage request, ODataPath odataPath)
        {
            return new HttpResponseException(
                request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = Error.Format(SRResources.EntitySetControllerUnmappedRequest, odataPath.PathTemplate),
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = SRResources.EntitySetControllerUnmappedRequestErrorCode
                    }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException GetKeyNotImplementedResponse(HttpRequestMessage request)
        {
            return new HttpResponseException(
                request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedGetKey,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "POST")
                    }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException GetEntityByKeyNotImplementedResponse(HttpRequestMessage request)
        {
            return new HttpResponseException(
                request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedGetByKey,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = SRResources.EntitySetControllerUnsupportedGetByKeyErrorCode
                    }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException CreateEntityNotImplementedResponse(HttpRequestMessage request)
        {
            return new HttpResponseException(
                request.CreateResponse(
                    HttpStatusCode.NotImplemented,
                    new ODataError
                    {
                        Message = SRResources.EntitySetControllerUnsupportedCreate,
                        MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                        ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "POST")
                    }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException UpdateEntityNotImplementedResponse(HttpRequestMessage request)
        {
            return new HttpResponseException(
                        request.CreateResponse(
                            HttpStatusCode.NotImplemented,
                            new ODataError
                            {
                                Message = SRResources.EntitySetControllerUnsupportedUpdate,
                                MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                                ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "PUT")
                            }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException PatchEntityNotImplementedResponse(HttpRequestMessage request)
        {
            return new HttpResponseException(
                    request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = SRResources.EntitySetControllerUnsupportedPatch,
                            MessageLanguage = SRResources.EntitySetControllerErrorMessageLanguage,
                            ErrorCode = Error.Format(SRResources.EntitySetControllerUnsupportedMethodErrorCode, "PATCH")
                        }));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response disposed later")]
        public static HttpResponseException DeleteEntityNotImplementedResponse(HttpRequestMessage request)
        {
            return new HttpResponseException(
                request.CreateResponse(
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
        internal static bool RequestPrefersReturnContent(HttpRequestMessage request)
        {
            IEnumerable<string> preferences = null;
            if (request.Headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnContentHeaderValue);
            }
            return false;
        }

        /// <summary>
        /// Returns whether or not the request prefers no content to be returned.
        /// </summary>
        /// <returns><c>true</c> if the request has a Prefer header value for "return-no-content", <c>false</c> otherwise</returns>
        internal static bool RequestPrefersReturnNoContent(HttpRequestMessage request)
        {
            IEnumerable<string> preferences = null;
            if (request.Headers.TryGetValues(PreferHeaderName, out preferences))
            {
                return preferences.Contains(ReturnNoContentHeaderValue);
            }
            return false;
        }
    }
}
