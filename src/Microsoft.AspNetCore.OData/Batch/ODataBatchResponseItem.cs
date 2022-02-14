//-----------------------------------------------------------------------------
// <copyright file="ODataBatchResponseItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Represents an OData batch response.
    /// </summary>
    public abstract class ODataBatchResponseItem
    {
        /// <summary>
        /// Writes a single OData batch response using a synchronous writer.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="context">The message context.</param>
        public static async Task WriteMessageAsync(ODataBatchWriter writer, HttpContext context)
        {
            await WriteMessageAsync(writer, context, false);
        }
        
        /// <summary>
        /// Writes a single OData batch response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="context">The message context.</param>
        /// <param name="asyncWriter">Whether or not the writer is in async mode. </param>
        public static async Task WriteMessageAsync(ODataBatchWriter writer, HttpContext context, bool asyncWriter)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            string contentId = (context.Request != null) ? context.Request.GetODataContentId() : String.Empty;

            ODataBatchOperationResponseMessage batchResponse = asyncWriter ?
                await writer.CreateOperationResponseMessageAsync(contentId) :
                writer.CreateOperationResponseMessage(contentId);

            batchResponse.StatusCode = context.Response.StatusCode;

            foreach (KeyValuePair<string, StringValues> header in context.Response.Headers)
            {
                batchResponse.SetHeader(header.Key, String.Join(",", header.Value.ToArray()));
            }

            if (context.Response.Body != null && context.Response.Body.Length != 0)
            {
                using (Stream stream = asyncWriter ? await batchResponse.GetStreamAsync() : batchResponse.GetStream())
                {
                    context.RequestAborted.ThrowIfCancellationRequested();
                    context.Response.Body.Seek(0L, SeekOrigin.Begin);
                    await context.Response.Body.CopyToAsync(stream);

                    // Close and release the stream for the individual response
                    ODataBatchStream batchStream = context.Response.Body as ODataBatchStream;
                    if (batchStream != null)
                    {
                        if (asyncWriter)
                        {
                            await batchStream.InternalDisposeAsync();
                        }
                        else
                        {
                            batchStream.InternalDispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes the response to a synchronous writer.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        public Task WriteResponseAsync(ODataBatchWriter writer)
        {
            return WriteResponseAsync(writer, false);
        }

        /// <summary>
        /// Writes the response.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="asyncWriter">Whether or not the writer is writing asynchronously.</param>
        public abstract Task WriteResponseAsync(ODataBatchWriter writer, bool asyncWriter);

        /// <summary>
        /// Gets a value that indicates if the responses in this item are successful.
        /// </summary>
        internal virtual bool IsResponseSuccessful()
        {
            return false;
        }
    }
}
