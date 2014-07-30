// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.OData.Core;

namespace System.Web.OData.Batch
{
    /// <summary>
    /// Represents an OData batch response.
    /// </summary>
    public abstract class ODataBatchResponseItem : IDisposable
    {
        /// <summary>
        /// Writes a single OData batch response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="response">The response message.</param>
        /// <returns>A task object representing writing the given batch response using the given writer.</returns>
        public static Task WriteMessageAsync(ODataBatchWriter writer, HttpResponseMessage response)
        {
            return WriteMessageAsync(writer, response, CancellationToken.None);
        }

        /// <summary>
        /// Writes a single OData batch response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="response">The response message.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task object representing writing the given batch response using the given writer.</returns>
        public static async Task WriteMessageAsync(ODataBatchWriter writer, HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            HttpRequestMessage request = response.RequestMessage;
            string contentId = (request != null) ? request.GetODataContentId() : String.Empty;

            ODataBatchOperationResponseMessage batchResponse = writer.CreateOperationResponseMessage(contentId);

            batchResponse.StatusCode = (int)response.StatusCode;

            foreach (KeyValuePair<string, IEnumerable<string>> header in response.Headers)
            {
                batchResponse.SetHeader(header.Key, String.Join(",", header.Value));
            }

            if (response.Content != null)
            {
                foreach (KeyValuePair<string, IEnumerable<string>> header in response.Content.Headers)
                {
                    batchResponse.SetHeader(header.Key, String.Join(",", header.Value));
                }

                using (Stream stream = batchResponse.GetStream())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await response.Content.CopyToAsync(stream);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes the response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public abstract Task WriteResponseAsync(ODataBatchWriter writer, CancellationToken cancellationToken);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);
    }
}