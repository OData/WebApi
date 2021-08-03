// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="ODataBatchReader"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataBatchReaderExtensions
    {
        /// <summary>
        /// Reads a ChangeSet request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch Id.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A collection of <see cref="HttpRequest"/> in the ChangeSet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of request messages asynchronously.")]
        public static async Task<IList<HttpContext>> ReadChangeSetRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.ChangesetStart)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.ChangesetStart.ToString());
            }

            Guid changeSetId = Guid.NewGuid();
            List<HttpContext> contexts = new List<HttpContext>();
            while (await reader.ReadAsync() && reader.State != ODataBatchReaderState.ChangesetEnd)
            {
                if (reader.State == ODataBatchReaderState.Operation)
                {
                    contexts.Add(await ReadOperationInternalAsync(reader, context, batchId, changeSetId, cancellationToken));
                }
            }
            return contexts;
        }

        /// <summary>
        /// Reads an Operation request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpRequest"/> representing the operation.</returns>
        public static Task<HttpContext> ReadOperationRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, bool bufferContentStream, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.Operation)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.Operation.ToString());
            }

            return ReadOperationInternalAsync(reader, context, batchId, null, cancellationToken, bufferContentStream);
        }

        /// <summary>
        /// Reads an Operation request in a ChangeSet.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="changeSetId">The ChangeSet ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpRequest"/> representing a ChangeSet operation</returns>
        public static Task<HttpContext> ReadChangeSetOperationRequestAsync(
            this ODataBatchReader reader, HttpContext context, Guid batchId, Guid changeSetId, bool bufferContentStream, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw Error.ArgumentNull("reader");
            }
            if (reader.State != ODataBatchReaderState.Operation)
            {
                throw Error.InvalidOperation(
                    SRResources.InvalidBatchReaderState,
                    reader.State.ToString(),
                    ODataBatchReaderState.Operation.ToString());
            }

            return ReadOperationInternalAsync(reader, context, batchId, changeSetId, cancellationToken, bufferContentStream);
        }

        private static async Task<HttpContext> ReadOperationInternalAsync(
            ODataBatchReader reader, HttpContext originalContext, Guid batchId, Guid? changeSetId, CancellationToken cancellationToken, bool bufferContentStream = true)
        {
            // add log: batchRequest.Url
            ILoggerFactory loggeFactory = originalContext.RequestServices.GetService<ILoggerFactory>();
            ILogger logger = loggeFactory.CreateLogger<ODataBatchHandler>();
            string displayUrl = originalContext.Request.GetDisplayUrl();
            logger.LogInformation($"[ODataInfo:] ReadOperationInternalAsync 1, original-Request={displayUrl} ...");

            ODataBatchOperationRequestMessage batchRequest = await reader.CreateOperationRequestMessageAsync();

            string batchRequestUrl = batchRequest.Url.OriginalString;
            logger.LogInformation($"[ODataInfo:] ReadOperationInternalAsync 2, Read sub-Request={batchRequestUrl} ...");

            HttpContext context = CreateHttpContext(originalContext);
            HttpRequest request = context.Request;

            request.Method = batchRequest.Method;
            request.CopyAbsoluteUrl(batchRequest.Url);

            string newUrl = request.GetDisplayUrl();
            logger.LogInformation($"[ODataInfo:] ReadOperationInternalAsync 3, new-Request={newUrl} ...");

            // Not using bufferContentStream. Unlike AspNet, AspNetCore cannot guarantee the disposal
            // of the stream in the context of execution so there is no choice but to copy the stream
            // from the batch reader.
            using (Stream stream = batchRequest.GetStream())
            {
                MemoryStream bufferedStream = new MemoryStream();
                // Passing in the default buffer size of 81920 so that we can also pass in a cancellation token
                await stream.CopyToAsync(bufferedStream, bufferSize: 81920, cancellationToken: cancellationToken);
                bufferedStream.Position = 0;
                request.Body = bufferedStream;
            }

            foreach (var header in batchRequest.Headers)
            {
                // Copy headers from batch, overwriting any existing headers.
                string headerName = header.Key;
                string headerValue = header.Value;
                request.Headers[headerName] = headerValue;
            }

            request.SetODataBatchId(batchId);
            request.SetODataContentId(batchRequest.ContentId);

            if (changeSetId != null && changeSetId.HasValue)
            {
                request.SetODataChangeSetId(changeSetId.Value);
            }

            return context;
        }

        private static HttpContext CreateHttpContext(HttpContext originalContext)
        {
            // Clone the features so that a new set is used for each context.
            // The features themselves will be reused but not the collection. We
            // store the request container as a feature of the request and we don't want
            // the features added to one context/request to be visible on another.
            //
            // Note that just about everything inm the HttpContext and HttpRequest is
            // backed by one of these features. So reusing the features means the HttContext
            // and HttpRequests are the same without needing to copy properties. To make them
            // different, we need to avoid copying certain features to that the objects don't
            // share the same storage/
            IFeatureCollection features = new FeatureCollection();
            string pathBase = "";
            foreach (KeyValuePair<Type, object> kvp in originalContext.Features)
            {
                // Don't include the OData features. They may already
                // be present. This will get re-created later.
                //
                // Also, clear out the items feature, which is used
                // to store a few object, the one that is an issue here is the Url
                // helper, which has an affinity to the context. If we leave it,
                // the context of the helper no longer matches the new context and
                // the resulting url helper doesn't have access to the OData feature
                // because it's looking in the wrong context.
                //
                // Because we need a different request and response, leave those features
                // out as well.
                if (kvp.Key == typeof(IHttpRequestFeature))
                {
                    pathBase = ((IHttpRequestFeature)kvp.Value).PathBase;
                }

                if (kvp.Key == typeof(IODataBatchFeature) ||
                    kvp.Key == typeof(IODataFeature) ||
                    kvp.Key == typeof(IItemsFeature) ||
                    kvp.Key == typeof(IHttpRequestFeature) ||
                    kvp.Key == typeof(IHttpResponseFeature))
                {
                    continue;
                }

                features[kvp.Key] = kvp.Value;
            }

            // Add in an items, request and response feature.
            features[typeof(IItemsFeature)] = new ItemsFeature();
            features[typeof(IHttpRequestFeature)] = new HttpRequestFeature
            {
                PathBase = pathBase
            };
            features[typeof(IHttpResponseFeature)] = new HttpResponseFeature();

            // Create a context from the factory or use the default context.
            HttpContext context = null;
            IHttpContextFactory httpContextFactory = originalContext.RequestServices.GetRequiredService<IHttpContextFactory>();
            if (httpContextFactory != null)
            {
                context = httpContextFactory.Create(features);
            }
            else
            {
                context = new DefaultHttpContext(features);
            }

            // Clone parts of the request. All other parts of the request will be 
            // populated during batch processing.
            context.Request.Cookies = originalContext.Request.Cookies;
            foreach (KeyValuePair<string, StringValues> header in originalContext.Request.Headers)
            {
                context.Request.Headers.Add(header);
            }

            // Create a response body as the default response feature does not
            // have a valid stream.
            context.Response.Body = new MemoryStream();

            return context;
        }
    }
}