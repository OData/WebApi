// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace System.Web.OData.Batch
{
    /// <summary>
    /// Represents an Operation request.
    /// </summary>
    public class OperationRequestItem : ODataBatchRequestItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationRequestItem"/> class.
        /// </summary>
        /// <param name="request">The Operation request.</param>
        public OperationRequestItem(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Request = request;
        }

        /// <summary>
        /// Gets the Operation request.
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// Sends the Operation request.
        /// </summary>
        /// <param name="invoker">The invoker.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="OperationResponseItem"/>.</returns>
        public override async Task<ODataBatchResponseItem> SendRequestAsync(HttpMessageInvoker invoker, CancellationToken cancellationToken)
        {
            if (invoker == null)
            {
                throw Error.ArgumentNull("invoker");
            }

            HttpResponseMessage response = await SendMessageAsync(invoker, Request, cancellationToken, null);
            return new OperationResponseItem(response);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request.Dispose();
            }
        }

        /// <summary>
        /// Gets the resources registered for dispose on the Operation request message.
        /// </summary>
        /// <returns>A collection of resources registered for dispose.</returns>
        public override IEnumerable<IDisposable> GetResourcesForDisposal()
        {
            return Request.GetResourcesForDisposal();
        }
    }
}