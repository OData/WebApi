// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
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
        /// Writes a single OData batch response Synchronously.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="response">The response message.</param>
        public static void WriteMessage(ODataBatchWriter writer, HttpResponseMessage response)
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
                    response.Content.CopyToAsync(stream).Wait();
                }
            }
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

            ODataBatchOperationResponseMessage batchResponse = await writer.CreateOperationResponseMessageAsync(contentId);

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
        /// Writes the response Synchronously.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        public abstract void WriteResponse(ODataBatchWriter writer);

        /// <summary>
        /// Writes the response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        public abstract Task WriteResponseAsync(ODataBatchWriter writer, CancellationToken cancellationToken);

        /// <summary>
        /// Writes the response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <remarks>
        /// This method exists to provide a consistent API to <see cref="ODataBatchContent"/>.
        /// The AspNetCore call does not need the CancellationToken passed in and instead of
        /// adding an internal call on that side, I opted to add the internal call here since
        /// the AspNetCore call would ignore the parameter and this one just assumes one.
        /// </remarks>
        internal Task WriteResponseAsync(ODataBatchWriter writer)
        {
            return WriteResponseAsync(writer, CancellationToken.None);
        }

        /// <summary>
        /// Gets a value that indicates if the responses in this item are successful.
        /// </summary>
        internal virtual bool IsResponseSuccessful()
        {
            return false;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected abstract void Dispose(bool disposing);
    }
}