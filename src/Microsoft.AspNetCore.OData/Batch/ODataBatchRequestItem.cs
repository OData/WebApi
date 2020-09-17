// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Represents an OData batch request.
    /// </summary>
    public abstract class ODataBatchRequestItem
    {
        /// <summary>
        /// Routes a single OData batch request.
        /// </summary>
        /// <param name="handler">The handler for processing a message.</param>
        /// <param name="context">The context.</param>
        /// <param name="contentIdToLocationMapping">The Content-ID to Location mapping.</param>
        /// <returns></returns>
        public static async Task SendRequestAsync(RequestDelegate handler, HttpContext context, Dictionary<string, string> contentIdToLocationMapping)
        {
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (contentIdToLocationMapping != null)
            {
                string encodedUrl = context.Request.GetEncodedUrl();
                string resolvedRequestUrl = ContentIdHelpers.ResolveContentId(encodedUrl, contentIdToLocationMapping);
                Uri resolvedUri;
                if (!string.IsNullOrEmpty(resolvedRequestUrl)
                    && Uri.TryCreate(resolvedRequestUrl, UriKind.Absolute, out resolvedUri))
                { 
                    context.Request.CopyAbsoluteUrl(resolvedUri);
                }

                context.Request.SetODataContentIdMapping(contentIdToLocationMapping);
            }

            try
            {
                await handler(context);

                string contentId = context.Request.GetODataContentId();

                if (contentIdToLocationMapping != null && contentId != null)
                {
                    AddLocationHeaderToMapping(context.Response, contentIdToLocationMapping, contentId);
                }
            }
            catch (Exception)
            {
                // Unlike AspNet, the exception handling is (by default) upstream of this middleware
                // so we need to trap exceptions on our own. This code is similar to the
                // ExceptionHandlerMiddleware class in AspNetCore.
                context.Response.Clear();
                context.Response.StatusCode = 500;
            }
        }

        private static void AddLocationHeaderToMapping(
            HttpResponse response,
            IDictionary<string, string> contentIdToLocationMapping,
            string contentId)
        {
            Contract.Assert(response != null);
            Contract.Assert(response.Headers != null);
            Contract.Assert(contentIdToLocationMapping != null);
            Contract.Assert(contentId != null);

            var headers = response.GetTypedHeaders();
            if (headers.Location != null)
            {
                contentIdToLocationMapping.Add(contentId, headers.Location.AbsoluteUri);
            }
        }

        /// <summary>
        /// Routes the request.
        /// </summary>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>A <see cref="ODataBatchResponseItem"/>.</returns>
        public abstract Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler);
    }
}