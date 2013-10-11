// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Batch;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Batch
{
    /// <summary>
    /// Default implementation of <see cref="ODataBatchHandler"/> for handling OData batch request.
    /// </summary>
    /// <remarks>
    /// By default, it buffers the request content stream.
    /// </remarks>
    public class DefaultODataBatchHandler : ODataBatchHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultODataBatchHandler"/> class.
        /// </summary>
        /// <param name="httpServer">The <see cref="HttpServer"/> for handling the individual batch requests.</param>
        public DefaultODataBatchHandler(HttpServer httpServer)
            : base(httpServer)
        {
        }

        /// <inheritdoc/>
        public override async Task<HttpResponseMessage> ProcessBatchAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            ValidateRequest(request);

            IList<ODataBatchRequestItem> subRequests = await ParseBatchRequestsAsync(request, cancellationToken);

            try
            {
                IList<ODataBatchResponseItem> responses = await ExecuteRequestMessagesAsync(subRequests, cancellationToken);
                return await CreateResponseMessageAsync(responses, request, cancellationToken);
            }
            finally
            {
                foreach (ODataBatchRequestItem subRequest in subRequests)
                {
                    request.RegisterForDispose(subRequest.GetResourcesForDisposal());
                    request.RegisterForDispose(subRequest);
                }
            }
        }

        /// <summary>
        /// Executes the OData batch requests.
        /// </summary>
        /// <param name="requests">The collection of OData batch requests.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A collection of <see cref="ODataBatchResponseItem"/> for the batch requests.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of response messages asynchronously.")]
        public virtual async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, CancellationToken cancellationToken)
        {
            if (requests == null)
            {
                throw Error.ArgumentNull("requests");
            }

            IList<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();

            try
            {
                foreach (ODataBatchRequestItem request in requests)
                {
                    responses.Add(await request.SendRequestAsync(Invoker, cancellationToken));
                }
            }
            catch
            {
                foreach (ODataBatchResponseItem response in responses)
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }
                throw;
            }

            return responses;
        }

        /// <summary>
        /// Converts the incoming OData batch request into a collection of request messages.
        /// </summary>
        /// <param name="request">The request containing the batch request messages.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A collection of <see cref="ODataBatchRequestItem"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of request messages asynchronously.")]
        public virtual async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings
            {
                DisableMessageStreamDisposal = true,
                MessageQuotas = MessageQuotas,
                BaseUri = GetBaseUri(request)
            };

            ODataMessageReader reader = await request.Content.GetODataMessageReaderAsync(oDataReaderSettings, cancellationToken);
            request.RegisterForDispose(reader);

            List<ODataBatchRequestItem> requests = new List<ODataBatchRequestItem>();
            ODataBatchReader batchReader = reader.CreateODataBatchReader();
            Guid batchId = Guid.NewGuid();
            while (batchReader.Read())
            {
                if (batchReader.State == ODataBatchReaderState.ChangesetStart)
                {
                    IList<HttpRequestMessage> changeSetRequests = await batchReader.ReadChangeSetRequestAsync(batchId, cancellationToken);
                    foreach (HttpRequestMessage changeSetRequest in changeSetRequests)
                    {
                        changeSetRequest.CopyBatchRequestProperties(request);
                    }
                    requests.Add(new ChangeSetRequestItem(changeSetRequests));
                }
                else if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    HttpRequestMessage operationRequest = await batchReader.ReadOperationRequestAsync(batchId, bufferContentStream: true, cancellationToken: cancellationToken);
                    operationRequest.CopyBatchRequestProperties(request);
                    requests.Add(new OperationRequestItem(operationRequest));
                }
            }

            return requests;
        }

        /// <summary>
        /// Creates the batch response message.
        /// </summary>
        /// <param name="responses">The responses for the batch requests.</param>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The batch response message.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        public virtual Task<HttpResponseMessage> CreateResponseMessageAsync(
            IEnumerable<ODataBatchResponseItem> responses, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.CreateODataBatchResponseAsync(responses, MessageQuotas);
        }

        /// <summary>
        /// Validates the incoming request that contains the batch request messages.
        /// </summary>
        /// <param name="request">The request containing the batch request messages.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing the object.")]
        public virtual void ValidateRequest(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.ValidateODataBatchRequest();
        }

        /// <summary>
        /// Gets the base URI for the batched requests.
        /// </summary>
        /// <param name="request">The original request containing all the batch requests.</param>
        /// <returns>The base URI.</returns>
        public virtual Uri GetBaseUri(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetODataBatchBaseUri(ODataRouteName);
        }
    }
}