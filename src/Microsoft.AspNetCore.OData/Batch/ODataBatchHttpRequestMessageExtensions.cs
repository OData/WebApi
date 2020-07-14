// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
#if !NETSTANDARD2_0
using Microsoft.AspNetCore.Http.Features;
#endif
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequest"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataBatchHttpRequestExtensions
    {
        private const string BatchIdKey = "BatchId";
        private const string ChangeSetIdKey = "ChangesetId";
        private const string ContentIdKey = "ContentId";
        private const string ContentIdMappingKey = "ContentIdMapping";
        private const string BatchMediaTypeMime = "multipart/mixed";
        private const string BatchMediaTypeJson = "application/json";
        private const string Boundary = "boundary";

        /// <summary>
        /// Determine if the request is a batch request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsODataBatchRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return (request.ContentType != null &&
                (request.ContentType.StartsWith(BatchMediaTypeMime) || request.ContentType.StartsWith(BatchMediaTypeJson)));
        }

        /// <summary>
        /// Retrieves the Batch ID associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Batch ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static Guid? GetODataBatchId(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.ODataBatchFeature().BatchId;
        }

        /// <summary>
        /// Associates a given Batch ID with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="batchId">The Batch ID.</param>
        public static void SetODataBatchId(this HttpRequest request, Guid batchId)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.ODataBatchFeature().BatchId = batchId;
        }

        /// <summary>
        /// Retrieves the ChangeSet ID associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The ChangeSet ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static Guid? GetODataChangeSetId(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.ODataBatchFeature().ChangeSetId;
        }

        /// <summary>
        /// Associates a given ChangeSet ID with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="changeSetId">The ChangeSet ID.</param>
        public static void SetODataChangeSetId(this HttpRequest request, Guid changeSetId)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.ODataBatchFeature().ChangeSetId = changeSetId;
        }

        /// <summary>
        /// Retrieves the Content-ID associated with the sub-request of a batch.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Content-ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static string GetODataContentId(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.ODataBatchFeature().ContentId;
        }

        /// <summary>
        /// Associates a given Content-ID with the sub-request of a batch.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="contentId">The Content-ID.</param>
        public static void SetODataContentId(this HttpRequest request, string contentId)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.ODataBatchFeature().ContentId = contentId;
        }

        /// <summary>
        /// Retrieves the Content-ID to Location mapping associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Content-ID to Location mapping associated with this request, or <c>null</c> if there isn't one.</returns>
        public static IDictionary<string, string> GetODataContentIdMapping(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.ODataBatchFeature().ContentIdMapping;
        }

        /// <summary>
        /// Associates a given Content-ID to Location mapping with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="contentIdMapping">The Content-ID to Location mapping.</param>
        public static void SetODataContentIdMapping(this HttpRequest request, IDictionary<string, string> contentIdMapping)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.ODataBatchFeature().ContentIdMapping = contentIdMapping;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        internal static Task CreateODataBatchResponseAsync(this HttpRequest request, IEnumerable<ODataBatchResponseItem> responses, ODataMessageQuotas messageQuotas)
        {
            Contract.Assert(request != null);

            ODataVersion odataVersion = ODataInputFormatter.GetODataResponseVersion(request);

            IServiceProvider requestContainer = request.GetRequestContainer();
            ODataMessageWriterSettings writerSettings = requestContainer.GetRequiredService<ODataMessageWriterSettings>();
            writerSettings.Version = odataVersion;
            writerSettings.MessageQuotas = messageQuotas;

            HttpResponse response = request.HttpContext.Response;

            IEnumerable<MediaTypeHeaderValue> acceptHeaders = MediaTypeHeaderValue.ParseList(request.Headers.GetCommaSeparatedValues("Accept"));
            string responseContentType = null;

            foreach (var acceptHeader in acceptHeaders.OrderByDescending(h => h.Quality))
            {
                if (acceptHeader.MediaType.Equals(ODataBatchHttpRequestExtensions.BatchMediaTypeMime, StringComparison.OrdinalIgnoreCase))
                {
                    responseContentType = String.Format(CultureInfo.InvariantCulture, "multipart/mixed;boundary=batchresponse_{0}", Guid.NewGuid());
                    break;
                }
                else if (acceptHeader.MediaType.Equals(ODataBatchHttpRequestExtensions.BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase))
                {
                    responseContentType = ODataBatchHttpRequestExtensions.BatchMediaTypeJson;
                    break;
                }
            }
            if (responseContentType == null)
            {
                // In absence of accept, if request was JSON then default response to be JSON.
                // Note that, if responseContentType is not set, then it will default to multipart/mixed
                // when constructing the BatchContent, so we don't need to handle that case here
                if (!String.IsNullOrEmpty(request.ContentType)
                && request.ContentType.IndexOf(ODataBatchHttpRequestExtensions.BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    responseContentType = ODataBatchHttpRequestExtensions.BatchMediaTypeJson;
                }
            }

            response.StatusCode = (int)HttpStatusCode.OK;
            ODataBatchContent batchContent = new ODataBatchContent(responses, requestContainer, responseContentType);
            foreach (var header in batchContent.Headers)
            {
                // Copy headers from batch content, overwriting any existing headers.
                response.Headers[header.Key] = header.Value;
            }

            return batchContent.SerializeToStreamAsync(response.Body);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        internal static async Task<bool> ValidateODataBatchRequest(this HttpRequest request)
        {
            Contract.Assert(request != null);

            HttpResponse response = request.HttpContext.Response;

            if (request.Body == null)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await response.WriteAsync(SRResources.BatchRequestMissingContent);
                return false;
            }

            RequestHeaders headers = request.GetTypedHeaders();
            MediaTypeHeaderValue contentType = headers.ContentType;

            if (contentType == null)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await response.WriteAsync(SRResources.BatchRequestMissingContentType);
                return false;
            }

            string mediaType = contentType.MediaType.ToString();
            bool isMimeBatch = String.Equals(mediaType, BatchMediaTypeMime, StringComparison.OrdinalIgnoreCase);
            bool isJsonBatch = String.Equals(mediaType, BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase);

            if (!isMimeBatch && !isJsonBatch)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await response.WriteAsync(Error.Format(SRResources.BatchRequestInvalidMediaType,
                    BatchMediaTypeMime, BatchMediaTypeJson));
                return false;
            }

            if (isMimeBatch)
            {
                NameValueHeaderValue boundary = contentType.Parameters.FirstOrDefault(p => String.Equals(p.Name.ToString(), Boundary, StringComparison.OrdinalIgnoreCase));
                if (boundary == null || String.IsNullOrEmpty(boundary.Value.ToString()))
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await response.WriteAsync(SRResources.BatchRequestMissingBoundary);
                    return false;
                }
            }

            return true;
        }

        internal static Uri GetODataBatchBaseUri(this HttpRequest request, string oDataRouteName, IRouter route)
        {
            Contract.Assert(request != null);

            if (oDataRouteName == null)
            {
                // Return request's base address.
                return new Uri(request.GetDisplayUrl());
            }

            HttpContext context = request.HttpContext;

#if !NETSTANDARD2_0
            // Here's workaround to help "EndpointLinkGenerator" to generator
            ODataBatchPathMapping batchMapping = request.HttpContext.RequestServices.GetRequiredService<ODataBatchPathMapping>();
            if (batchMapping.IsEndpointRouting)
            {
                context = new DefaultHttpContext
                {
                    RequestServices = request.HttpContext.RequestServices,
                };

                IEndpointFeature endpointFeature = new ODataEndpointFeature();
                endpointFeature.Endpoint = new Endpoint((d) => null, null, "anything");
                context.Features.Set(endpointFeature);

                context.Request.Scheme = request.Scheme;
                context.Request.Host = request.Host;
            }

            context.Request.ODataFeature().RouteName = oDataRouteName;
#endif

            // The IActionContextAccessor and ActionContext will be present after routing but not before
            // GetUrlHelper only uses the HttpContext and the Router, which we have so construct a dummy
            // action context.
            ActionContext actionContext = new ActionContext
            {
                HttpContext = context,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            actionContext.RouteData.Routers.Add(route);
            IUrlHelperFactory factory = request.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
            IUrlHelper helper = factory.GetUrlHelper(actionContext);

            RouteValueDictionary routeData = new RouteValueDictionary() { { ODataRouteConstants.ODataPath, String.Empty } };
            RouteValueDictionary batchRouteData = request.ODataFeature().BatchRouteData;
            if (batchRouteData != null && batchRouteData.Any())
            {
                foreach (var data in batchRouteData)
                {
                    routeData.Add(data.Key, data.Value);
                }
            }

            string baseAddress = helper.Link(oDataRouteName, routeData);
            if (baseAddress == null)
            {
                throw new InvalidOperationException(SRResources.UnableToDetermineBaseUrl);
            }
            return new Uri(baseAddress);
        }

#if !NETSTANDARD2_0
        internal class ODataEndpointFeature : IEndpointFeature
        {
            public Endpoint Endpoint { get; set; }
        }
#endif
    }
}
