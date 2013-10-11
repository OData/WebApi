// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.OData.Properties;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Batch
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
        /// <param name="batchId">The Batch Id.</param>
        /// <returns>A collection of <see cref="HttpRequestMessage"/> in the ChangeSet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of request messages asynchronously.")]
        public static Task<IList<HttpRequestMessage>> ReadChangeSetRequestAsync(this ODataBatchReader reader, Guid batchId)
        {
            return reader.ReadChangeSetRequestAsync(batchId, CancellationToken.None);
        }

        /// <summary>
        /// Reads a ChangeSet request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="batchId">The Batch Id.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A collection of <see cref="HttpRequestMessage"/> in the ChangeSet.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of request messages asynchronously.")]
        public static async Task<IList<HttpRequestMessage>> ReadChangeSetRequestAsync(this ODataBatchReader reader, Guid batchId, CancellationToken cancellationToken)
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
            List<HttpRequestMessage> requests = new List<HttpRequestMessage>();
            while (reader.Read() && reader.State != ODataBatchReaderState.ChangesetEnd)
            {
                if (reader.State == ODataBatchReaderState.Operation)
                {
                    requests.Add(await ReadOperationInternalAsync(reader, batchId, changeSetId, cancellationToken));
                }
            }
            return requests;
        }

        /// <summary>
        /// Reads an Operation request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <returns>A <see cref="HttpRequestMessage"/> representing the operation.</returns>
        public static Task<HttpRequestMessage> ReadOperationRequestAsync(this ODataBatchReader reader, Guid batchId, bool bufferContentStream)
        {
            return reader.ReadOperationRequestAsync(batchId, bufferContentStream, CancellationToken.None);
        }

        /// <summary>
        /// Reads an Operation request.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpRequestMessage"/> representing the operation.</returns>
        public static Task<HttpRequestMessage> ReadOperationRequestAsync(this ODataBatchReader reader, Guid batchId, bool bufferContentStream, CancellationToken cancellationToken)
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

            return ReadOperationInternalAsync(reader, batchId, changeSetId: null, cancellationToken: cancellationToken, bufferContentStream: bufferContentStream);
        }

        /// <summary>
        /// Reads an Operation request in a ChangeSet.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="changeSetId">The ChangeSet ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <returns>A <see cref="HttpRequestMessage"/> representing a ChangeSet operation</returns>
        public static Task<HttpRequestMessage> ReadChangeSetOperationRequestAsync(this ODataBatchReader reader, Guid batchId, Guid changeSetId, bool bufferContentStream)
        {
            return reader.ReadChangeSetOperationRequestAsync(batchId, changeSetId, bufferContentStream, CancellationToken.None);
        }

        /// <summary>
        /// Reads an Operation request in a ChangeSet.
        /// </summary>
        /// <param name="reader">The <see cref="ODataBatchReader"/>.</param>
        /// <param name="batchId">The Batch ID.</param>
        /// <param name="changeSetId">The ChangeSet ID.</param>
        /// <param name="bufferContentStream">if set to <c>true</c> then the request content stream will be buffered.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpRequestMessage"/> representing a ChangeSet operation</returns>
        public static Task<HttpRequestMessage> ReadChangeSetOperationRequestAsync(
            this ODataBatchReader reader, Guid batchId, Guid changeSetId, bool bufferContentStream, CancellationToken cancellationToken)
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

            return ReadOperationInternalAsync(reader, batchId, changeSetId, cancellationToken, bufferContentStream);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        private static async Task<HttpRequestMessage> ReadOperationInternalAsync(
            ODataBatchReader reader, Guid batchId, Guid? changeSetId, CancellationToken cancellationToken, bool bufferContentStream = true)
        {
            ODataBatchOperationRequestMessage batchRequest = reader.CreateOperationRequestMessage();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Method = new HttpMethod(batchRequest.Method);
            request.RequestUri = batchRequest.Url;

            if (bufferContentStream)
            {
                using (Stream stream = batchRequest.GetStream())
                {
                    MemoryStream bufferedStream = new MemoryStream();
                    // Passing in the default buffer size of 81920 so that we can also pass in a cancellation token
                    await stream.CopyToAsync(bufferedStream, bufferSize: 81920, cancellationToken: cancellationToken);
                    bufferedStream.Position = 0;
                    request.Content = new StreamContent(bufferedStream);
                }
            }
            else
            {
                request.Content = new LazyStreamContent(() => batchRequest.GetStream());
            }

            foreach (var header in batchRequest.Headers)
            {
                string headerName = header.Key;
                string headerValue = header.Value;
                if (!request.Headers.TryAddWithoutValidation(headerName, headerValue))
                {
                    request.Content.Headers.TryAddWithoutValidation(headerName, headerValue);
                }
            }

            request.SetODataBatchId(batchId);

            if (changeSetId != null && changeSetId.HasValue)
            {
                request.SetODataChangeSetId(changeSetId.Value);
            }

            return request;
        }
    }
}