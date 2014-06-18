// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Batch;
using Microsoft.OData.Core;

namespace System.Web.OData.Batch
{
    /// <summary>
    /// An implementation of <see cref="ODataBatchHandler"/> that doesn't buffer the request content stream.
    /// </summary>
    public class UnbufferedODataBatchHandler : ODataBatchHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnbufferedODataBatchHandler"/> class.
        /// </summary>
        /// <param name="httpServer">The <see cref="HttpServer"/> for handling the individual batch requests.</param>
        public UnbufferedODataBatchHandler(HttpServer httpServer)
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

            ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings
            {
                DisableMessageStreamDisposal = true,
                MessageQuotas = MessageQuotas,
                BaseUri = GetBaseUri(request)
            };

            ODataMessageReader reader = await request.Content.GetODataMessageReaderAsync(oDataReaderSettings, cancellationToken);
            request.RegisterForDispose(reader);

            ODataBatchReader batchReader = reader.CreateODataBatchReader();
            List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();
            Guid batchId = Guid.NewGuid();
            List<IDisposable> resourcesToDispose = new List<IDisposable>();
            try
            {
                while (batchReader.Read())
                {
                    ODataBatchResponseItem responseItem = null;
                    if (batchReader.State == ODataBatchReaderState.ChangesetStart)
                    {
                        responseItem = await ExecuteChangeSetAsync(batchReader, batchId, request, cancellationToken);
                    }
                    else if (batchReader.State == ODataBatchReaderState.Operation)
                    {
                        responseItem = await ExecuteOperationAsync(batchReader, batchId, request, cancellationToken);
                    }
                    if (responseItem != null)
                    {
                        responses.Add(responseItem);
                    }
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

            return await CreateResponseMessageAsync(responses, request, cancellationToken);
        }

        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="batchReader">The batch reader.</param>
        /// <param name="batchId">The batch id.</param>
        /// <param name="originalRequest">The original request containing all the batch requests.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The response for the operation.</returns>
        public virtual async Task<ODataBatchResponseItem> ExecuteOperationAsync(ODataBatchReader batchReader, Guid batchId, HttpRequestMessage originalRequest, CancellationToken cancellationToken)
        {
            if (batchReader == null)
            {
                throw Error.ArgumentNull("batchReader");
            }
            if (originalRequest == null)
            {
                throw Error.ArgumentNull("originalRequest");
            }

            cancellationToken.ThrowIfCancellationRequested();
            HttpRequestMessage operationRequest = await batchReader.ReadOperationRequestAsync(batchId, bufferContentStream: false);

            operationRequest.CopyBatchRequestProperties(originalRequest);
            OperationRequestItem operation = new OperationRequestItem(operationRequest);
            try
            {
                ODataBatchResponseItem response = await operation.SendRequestAsync(Invoker, cancellationToken);
                return response;
            }
            finally
            {
                originalRequest.RegisterForDispose(operation.GetResourcesForDisposal());
                originalRequest.RegisterForDispose(operation);
            }
        }

        /// <summary>
        /// Executes the ChangeSet.
        /// </summary>
        /// <param name="batchReader">The batch reader.</param>
        /// <param name="batchId">The batch id.</param>
        /// <param name="originalRequest">The original request containing all the batch requests.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>The response for the ChangeSet.</returns>
        public virtual async Task<ODataBatchResponseItem> ExecuteChangeSetAsync(ODataBatchReader batchReader, Guid batchId, HttpRequestMessage originalRequest, CancellationToken cancellationToken)
        {
            if (batchReader == null)
            {
                throw Error.ArgumentNull("batchReader");
            }
            if (originalRequest == null)
            {
                throw Error.ArgumentNull("originalRequest");
            }

            Guid changeSetId = Guid.NewGuid();
            List<HttpResponseMessage> changeSetResponse = new List<HttpResponseMessage>();
            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            try
            {
                while (batchReader.Read() && batchReader.State != ODataBatchReaderState.ChangesetEnd)
                {
                    if (batchReader.State == ODataBatchReaderState.Operation)
                    {
                        HttpRequestMessage changeSetOperationRequest = await batchReader.ReadChangeSetOperationRequestAsync(batchId, changeSetId, bufferContentStream: false);
                        changeSetOperationRequest.CopyBatchRequestProperties(originalRequest);
                        try
                        {
                            HttpResponseMessage response = await ODataBatchRequestItem.SendMessageAsync(Invoker, changeSetOperationRequest, cancellationToken, contentIdToLocationMapping);
                            if (response.IsSuccessStatusCode)
                            {
                                changeSetResponse.Add(response);
                            }
                            else
                            {
                                ChangeSetRequestItem.DisposeResponses(changeSetResponse);
                                changeSetResponse.Clear();
                                changeSetResponse.Add(response);
                                return new ChangeSetResponseItem(changeSetResponse);
                            }
                        }
                        finally
                        {
                            originalRequest.RegisterForDispose(changeSetOperationRequest.GetResourcesForDisposal());
                            originalRequest.RegisterForDispose(changeSetOperationRequest);
                        }
                    }
                }
            }
            catch
            {
                ChangeSetRequestItem.DisposeResponses(changeSetResponse);
                throw;
            }

            return new ChangeSetResponseItem(changeSetResponse);
        }
    }
}