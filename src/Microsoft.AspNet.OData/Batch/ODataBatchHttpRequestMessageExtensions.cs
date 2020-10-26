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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataBatchHttpRequestMessageExtensions
    {
        private const string BatchIdKey = "BatchId";
        private const string ChangeSetIdKey = "ChangesetId";
        private const string ContentIdKey = "ContentId";
        private const string ContentIdMappingKey = "ContentIdMapping";
        private const string BatchMediaTypeMime = "multipart/mixed";
        private const string BatchMediaTypeJson = "application/json";
        private const string Boundary = "boundary";
        private const string ContentType = "Content-Type";

        /// <summary>
        /// Retrieves the Batch ID associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Batch ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static Guid? GetODataBatchId(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object batchId;
            if (request.Properties.TryGetValue(BatchIdKey, out batchId))
            {
                return (Guid)batchId;
            }

            return null;
        }

        /// <summary>
        /// Associates a given Batch ID with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="batchId">The Batch ID.</param>
        public static void SetODataBatchId(this HttpRequestMessage request, Guid batchId)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[BatchIdKey] = batchId;
        }

        /// <summary>
        /// Retrieves the ChangeSet ID associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The ChangeSet ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static Guid? GetODataChangeSetId(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object changeSetId;
            if (request.Properties.TryGetValue(ChangeSetIdKey, out changeSetId))
            {
                return (Guid)changeSetId;
            }

            return null;
        }

        /// <summary>
        /// Associates a given ChangeSet ID with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="changeSetId">The ChangeSet ID.</param>
        public static void SetODataChangeSetId(this HttpRequestMessage request, Guid changeSetId)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[ChangeSetIdKey] = changeSetId;
        }

        /// <summary>
        /// Retrieves the Content-ID associated with the sub-request of a batch.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Content-ID associated with this request, or <c>null</c> if there isn't one.</returns>
        public static string GetODataContentId(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object contentId;
            if (request.Properties.TryGetValue(ContentIdKey, out contentId))
            {
                return (string)contentId;
            }

            return null;
        }

        /// <summary>
        /// Associates a given Content-ID with the sub-request of a batch.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="contentId">The Content-ID.</param>
        public static void SetODataContentId(this HttpRequestMessage request, string contentId)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[ContentIdKey] = contentId;
        }

        /// <summary>
        /// Retrieves the Content-ID to Location mapping associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The Content-ID to Location mapping associated with this request, or <c>null</c> if there isn't one.</returns>
        public static IDictionary<string, string> GetODataContentIdMapping(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object contentIdMapping;
            if (request.Properties.TryGetValue(ContentIdMappingKey, out contentIdMapping))
            {
                return contentIdMapping as IDictionary<string, string>;
            }

            return null;
        }

        /// <summary>
        /// Associates a given Content-ID to Location mapping with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="contentIdMapping">The Content-ID to Location mapping.</param>
        public static void SetODataContentIdMapping(this HttpRequestMessage request, IDictionary<string, string> contentIdMapping)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[ContentIdMappingKey] = contentIdMapping;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        internal static Task<HttpResponseMessage> CreateODataBatchResponseAsync(this HttpRequestMessage request, IEnumerable<ODataBatchResponseItem> responses, ODataMessageQuotas messageQuotas)
        {
            Contract.Assert(request != null);

            ODataVersion odataVersion = ResultHelpers.GetODataResponseVersion(request);
            IServiceProvider requestContainer = request.GetRequestContainer();
            ODataMessageWriterSettings writerSettings =
                requestContainer.GetRequiredService<ODataMessageWriterSettings>();
            writerSettings.Version = odataVersion;
            writerSettings.MessageQuotas = messageQuotas;
            HttpHeaderValueCollection<MediaTypeWithQualityHeaderValue> acceptHeaderValues = request.Headers.Accept;

            MediaTypeHeaderValue responseContentType = null;
            foreach (MediaTypeWithQualityHeaderValue acceptHeader in acceptHeaderValues.OrderByDescending(h => h.Quality == null ? 1 : h.Quality))
            {
                if (acceptHeader.MediaType.Equals(ODataBatchHttpRequestMessageExtensions.BatchMediaTypeMime, StringComparison.OrdinalIgnoreCase))
                {
                    responseContentType = MediaTypeHeaderValue.Parse(
                    String.Format(CultureInfo.InvariantCulture, "multipart/mixed;boundary=batchresponse_{0}",
                        Guid.NewGuid()));
                    break;
                }
                else if (acceptHeader.MediaType.Equals(ODataBatchHttpRequestMessageExtensions.BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase))
                {
                    responseContentType = MediaTypeHeaderValue.Parse(ODataBatchHttpRequestMessageExtensions.BatchMediaTypeJson);
                    break;
                }
            }

            if (responseContentType == null)
            {
                // In absence of accept, if request was JSON then default response to be JSON.
                // Note that, if responseContentType is not set, then it will default to multipart/mixed
                // when constructing the BatchContent, so we don't need to handle that case here
                if (request.Content != null && request.Content.Headers.Any(h => String.Equals(h.Key, ODataBatchHttpRequestMessageExtensions.ContentType, StringComparison.OrdinalIgnoreCase)
                                        && h.Value.Any(v => v.IndexOf(ODataBatchHttpRequestMessageExtensions.BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase) > -1)))
                {
                    responseContentType = MediaTypeHeaderValue.Parse(ODataBatchHttpRequestMessageExtensions.BatchMediaTypeJson);
                }
            }

            HttpResponseMessage response = request.CreateResponse(HttpStatusCode.OK);
            response.Content = new ODataBatchContent(responses, requestContainer, responseContentType);
            return Task.FromResult(response);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        internal static void ValidateODataBatchRequest(this HttpRequestMessage request)
        {
            Contract.Assert(request != null);

            if (request.Content == null)
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    SRResources.BatchRequestMissingContent));
            }

            MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;
            if (contentType == null)
            {
                throw new HttpResponseException(request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    SRResources.BatchRequestMissingContentType));
            }

            bool isMimeBatch = String.Equals(contentType.MediaType, BatchMediaTypeMime, StringComparison.OrdinalIgnoreCase);
            bool isJsonBatch = String.Equals(contentType.MediaType, BatchMediaTypeJson, StringComparison.OrdinalIgnoreCase);

            if (!isMimeBatch && !isJsonBatch)
            {
                throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Error.Format(
                            SRResources.BatchRequestInvalidMediaType,
                            BatchMediaTypeMime,
                            BatchMediaTypeJson)));
            }

            if (isMimeBatch)
            {
                NameValueHeaderValue boundary = contentType.Parameters.FirstOrDefault(
                        p => String.Equals(p.Name, Boundary, StringComparison.OrdinalIgnoreCase));

                if (boundary == null || String.IsNullOrEmpty(boundary.Value))
                {
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        SRResources.BatchRequestMissingBoundary));
                }
            }
        }

        internal static Uri GetODataBatchBaseUri(this HttpRequestMessage request, string oDataRouteName)
        {
            Contract.Assert(request != null);

            if (oDataRouteName == null)
            {
                // Return request's base address.
                return new Uri(request.RequestUri, new Uri("/", UriKind.Relative));
            }
            else
            {
                UrlHelper helper = request.GetUrlHelper() ?? new UrlHelper(request);
                string baseAddress = helper.Link(oDataRouteName, new HttpRouteValueDictionary() { { ODataRouteConstants.ODataPath, String.Empty } });
                if (baseAddress == null)
                {
                    throw new InvalidOperationException(SRResources.UnableToDetermineBaseUrl);
                }
                return new Uri(baseAddress);
            }
        }
    }
}