// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.OData.Core;

namespace System.Web.OData.Batch
{
    /// <summary>
    /// Represents an Operation response.
    /// </summary>
    public class OperationResponseItem : ODataBatchResponseItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResponseItem"/> class.
        /// </summary>
        /// <param name="response">The response messages for the Operation request.</param>
        public OperationResponseItem(HttpResponseMessage response)
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            Response = response;
        }

        /// <summary>
        /// Gets the response messages for the Operation.
        /// </summary>
        public HttpResponseMessage Response { get; private set; }

        /// <summary>
        /// Writes the response as an Operation.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public override Task WriteResponseAsync(ODataBatchWriter writer, CancellationToken cancellationToken)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            return WriteMessageAsync(writer, Response, cancellationToken);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Response.Dispose();
            }
        }
    }
}