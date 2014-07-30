// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Web.OData.Batch
{
    /// <summary>
    /// Represents an OData batch request.
    /// </summary>
    public abstract class ODataBatchRequestItem : IDisposable
    {
        /// <summary>
        /// Sends a single OData batch request.
        /// </summary>
        /// <param name="invoker">The invoker.</param>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="contentIdToLocationMapping">The Content-ID to Location mapping.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> SendMessageAsync(HttpMessageInvoker invoker, HttpRequestMessage request, CancellationToken cancellationToken, Dictionary<string, string> contentIdToLocationMapping)
        {
            if (invoker == null)
            {
                throw Error.ArgumentNull("invoker");
            }
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (contentIdToLocationMapping != null)
            {
                string resolvedRequestUrl = ContentIdHelpers.ResolveContentId(request.RequestUri.OriginalString, contentIdToLocationMapping);
                request.RequestUri = new Uri(resolvedRequestUrl);

                request.SetODataContentIdMapping(contentIdToLocationMapping);
            }

            HttpResponseMessage response = await invoker.SendAsync(request, cancellationToken);
            string contentId = request.GetODataContentId();

            if (contentIdToLocationMapping != null && contentId != null)
            {
                ContentIdHelpers.AddLocationHeaderToMapping(response, contentIdToLocationMapping, contentId);
            }

            return response;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the resources for disposal.
        /// </summary>
        /// <returns>A collection of resources for disposal.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "The order of execution matters. The result can be different after calling SendMessageAsync.")]
        public abstract IEnumerable<IDisposable> GetResourcesForDisposal();

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="invoker">The invoker.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="ODataBatchResponseItem"/>.</returns>
        public abstract Task<ODataBatchResponseItem> SendRequestAsync(HttpMessageInvoker invoker, CancellationToken cancellationToken);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);
    }
}