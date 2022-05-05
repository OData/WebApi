//-----------------------------------------------------------------------------
// <copyright file="UnbufferedODataBatchHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// An implementation of <see cref="ODataBatchHandler"/> that doesn't buffer the request content stream.
    /// </summary>
    public class UnbufferedODataBatchHandler : ODataBatchHandler
    {
        /// <inheritdoc/>
        public override async Task ProcessBatchAsync(HttpContext context, RequestDelegate nextHandler)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }
            if (nextHandler == null)
            {
                throw Error.ArgumentNull("nextHandler");
            }

            if (!await ValidateRequest(context.Request))
            {
                return;
            }

            // This container is for the overall batch request.
            HttpRequest request = context.Request;
            IServiceProvider requestContainer = request.CreateRequestContainer(ODataRouteName);
            requestContainer.GetRequiredService<ODataMessageReaderSettings>().BaseUri = GetBaseUri(request);
            List<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();

            using (ODataMessageReader reader = request.GetODataMessageReader(requestContainer))
            {
                ODataBatchReader batchReader = await reader.CreateODataBatchReaderAsync();
                Guid batchId = Guid.NewGuid();

                ODataOptions options = context.RequestServices.GetRequiredService<ODataOptions>();
                bool enableContinueOnErrorHeader = (options != null)
                    ? options.EnableContinueOnErrorHeader
                    : false;

                SetContinueOnError(new WebApiRequestHeaders(request.Headers), enableContinueOnErrorHeader);

                while (await batchReader.ReadAsync())
                {
                    ODataBatchResponseItem responseItem = null;
                    if (batchReader.State == ODataBatchReaderState.ChangesetStart)
                    {
                        responseItem = await ExecuteChangeSetAsync(batchReader, batchId, request, nextHandler);
                    }
                    else if (batchReader.State == ODataBatchReaderState.Operation)
                    {
                        responseItem = await ExecuteOperationAsync(batchReader, batchId, request, nextHandler);
                    }
                    if (responseItem != null)
                    {
                        responses.Add(responseItem);
                        if (responseItem.IsResponseSuccessful() == false && ContinueOnError == false)
                        {
                            break;
                        }
                    }
                }
            }

            await CreateResponseMessageAsync(responses, request);
        }

        /// <summary>
        /// Executes the operation.
        /// </summary>
        /// <param name="batchReader">The batch reader.</param>
        /// <param name="batchId">The batch id.</param>
        /// <param name="originalRequest">The original request containing all the batch requests.</param>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>The response for the operation.</returns>
        public virtual async Task<ODataBatchResponseItem> ExecuteOperationAsync(ODataBatchReader batchReader, Guid batchId, HttpRequest originalRequest, RequestDelegate handler)
        {
            if (batchReader == null)
            {
                throw Error.ArgumentNull("batchReader");
            }
            if (originalRequest == null)
            {
                throw Error.ArgumentNull("originalRequest");
            }
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            CancellationToken cancellationToken = originalRequest.HttpContext.RequestAborted;
            cancellationToken.ThrowIfCancellationRequested();
            HttpContext operationContext = await batchReader.ReadOperationRequestAsync(originalRequest.HttpContext, batchId, false, cancellationToken);

            operationContext.Request.CopyBatchRequestProperties(originalRequest);
            operationContext.Request.DeleteRequestContainer(false);
            OperationRequestItem operation = new OperationRequestItem(operationContext);

            IDictionary<string, string> contentIdToLocationMapping = originalRequest.HttpContext.ODataBatchFeature().ContentIdMapping;
            if (contentIdToLocationMapping == null)
            {
                contentIdToLocationMapping = new Dictionary<string, string>();
                originalRequest.HttpContext.ODataBatchFeature().ContentIdMapping = contentIdToLocationMapping;
            }

            operation.ContentIdToLocationMapping = contentIdToLocationMapping;
            ODataBatchResponseItem responseItem = await operation.SendRequestAsync(handler);

            return responseItem;
        }

        /// <summary>
        /// Executes the ChangeSet.
        /// </summary>
        /// <param name="batchReader">The batch reader.</param>
        /// <param name="batchId">The batch id.</param>
        /// <param name="originalRequest">The original request containing all the batch requests.</param>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>The response for the ChangeSet.</returns>
        public virtual async Task<ODataBatchResponseItem> ExecuteChangeSetAsync(ODataBatchReader batchReader, Guid batchId, HttpRequest originalRequest, RequestDelegate handler)
        {
            if (batchReader == null)
            {
                throw Error.ArgumentNull("batchReader");
            }
            if (originalRequest == null)
            {
                throw Error.ArgumentNull("originalRequest");
            }
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            Guid changeSetId = Guid.NewGuid();
            List<HttpContext> changeSetResponse = new List<HttpContext>();
            IDictionary<string, string> contentIdToLocationMapping = originalRequest.HttpContext.ODataBatchFeature().ContentIdMapping;

            if (contentIdToLocationMapping == null)
            {
                contentIdToLocationMapping = new Dictionary<string, string>();
                originalRequest.HttpContext.ODataBatchFeature().ContentIdMapping = contentIdToLocationMapping;
            }


            while (await batchReader.ReadAsync() && batchReader.State != ODataBatchReaderState.ChangesetEnd)
            {
                if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    CancellationToken cancellationToken = originalRequest.HttpContext.RequestAborted;
                    HttpContext changeSetOperationContext = await batchReader.ReadChangeSetOperationRequestAsync(originalRequest.HttpContext, batchId, changeSetId, false, cancellationToken);
                    changeSetOperationContext.Request.CopyBatchRequestProperties(originalRequest);
                    changeSetOperationContext.Request.DeleteRequestContainer(false);

                    await ODataBatchRequestItem.SendRequestAsync(handler, changeSetOperationContext, contentIdToLocationMapping);
                    if (changeSetOperationContext.Response.IsSuccessStatusCode())
                    {
                        changeSetResponse.Add(changeSetOperationContext);
                    }
                    else
                    {
                        changeSetResponse.Clear();
                        changeSetResponse.Add(changeSetOperationContext);
                        return new ChangeSetResponseItem(changeSetResponse);
                    }
                }
            }

            return new ChangeSetResponseItem(changeSetResponse);
        }
    }
}
