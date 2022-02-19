//-----------------------------------------------------------------------------
// <copyright file="DefaultODataBatchHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// Default implementation of <see cref="ODataBatchHandler"/> for handling OData batch request.
    /// </summary>
    /// <remarks>
    /// By default, it buffers the request content stream.
    /// </remarks>
    public class DefaultODataBatchHandler : ODataBatchHandler
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

            IList<ODataBatchRequestItem> subRequests = await ParseBatchRequestsAsync(context);

            ODataOptions options = context.RequestServices.GetRequiredService<ODataOptions>();
            bool enableContinueOnErrorHeader = (options != null)
                ? options.EnableContinueOnErrorHeader
                : false;

            SetContinueOnError(new WebApiRequestHeaders(context.Request.Headers), enableContinueOnErrorHeader);

            IList<ODataBatchResponseItem> responses = await ExecuteRequestMessagesAsync(subRequests, nextHandler);
            await CreateResponseMessageAsync(responses, context.Request);
        }

        /// <summary>
        /// Executes the OData batch requests.
        /// </summary>
        /// <param name="requests">The collection of OData batch requests.</param>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>A collection of <see cref="ODataBatchResponseItem"/> for the batch requests.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of response messages asynchronously.")]
        public virtual async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
        {
            if (requests == null)
            {
                throw Error.ArgumentNull("requests");
            }
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            IList<ODataBatchResponseItem> responses = new List<ODataBatchResponseItem>();

            foreach (ODataBatchRequestItem request in requests)
            {
                ODataBatchResponseItem responseItem = await request.SendRequestAsync(handler);
                responses.Add(responseItem);

                if (responseItem != null && responseItem.IsResponseSuccessful() == false && ContinueOnError == false)
                {
                    break;
                }
            }

            return responses;
        }

        /// <summary>
        /// Converts the incoming OData batch request into a collection of request messages.
        /// </summary>
        /// <param name="context">The context containing the batch request messages.</param>
        /// <returns>A collection of <see cref="ODataBatchRequestItem"/>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "We need to return a collection of request messages asynchronously.")]
        public virtual async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            HttpRequest request = context.Request;
            IServiceProvider requestContainer = request.CreateRequestContainer(ODataRouteName);
            requestContainer.GetRequiredService<ODataMessageReaderSettings>().BaseUri = GetBaseUri(request);

            ODataMessageReader reader = request.GetODataMessageReader(requestContainer);

            CancellationToken cancellationToken = context.RequestAborted;
            List<ODataBatchRequestItem> requests = new List<ODataBatchRequestItem>();
            ODataBatchReader batchReader = await reader.CreateODataBatchReaderAsync();
            Guid batchId = Guid.NewGuid();
            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();

            while (await batchReader.ReadAsync())
            {
                if (batchReader.State == ODataBatchReaderState.ChangesetStart)
                {
                    IList<HttpContext> changeSetContexts = await batchReader.ReadChangeSetRequestAsync(context, batchId, cancellationToken);
                    foreach (HttpContext changeSetContext in changeSetContexts)
                    {
                        changeSetContext.Request.CopyBatchRequestProperties(request);
                        changeSetContext.Request.DeleteRequestContainer(false);
                    }

                    ChangeSetRequestItem requestItem = new ChangeSetRequestItem(changeSetContexts);
                    requestItem.ContentIdToLocationMapping = contentIdToLocationMapping;
                    requests.Add(requestItem);
                }
                else if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    HttpContext operationContext = await batchReader.ReadOperationRequestAsync(context, batchId, true, cancellationToken);
                    operationContext.Request.CopyBatchRequestProperties(request);
                    operationContext.Request.DeleteRequestContainer(false);
                    OperationRequestItem requestItem = new OperationRequestItem(operationContext);
                    requestItem.ContentIdToLocationMapping = contentIdToLocationMapping;
                    requests.Add(requestItem);
                }
            }

            return requests;
        }
    }
}
