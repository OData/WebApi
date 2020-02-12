// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
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
        /// Writes the response as an Operation Synchronously.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        public override void WriteResponse(ODataBatchWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            WriteMessage(writer, Response);
        }

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

        /// <summary>
        /// Gets a value that indicates if the responses in this item are successful.
        /// </summary>
        internal override bool IsResponseSuccessful()
        {
            return Response.IsSuccessStatusCode;
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