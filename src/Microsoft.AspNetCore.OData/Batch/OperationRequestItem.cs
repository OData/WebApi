//-----------------------------------------------------------------------------
// <copyright file="OperationRequestItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Represents an Operation request.
    /// </summary>
    public class OperationRequestItem : ODataBatchRequestItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationRequestItem"/> class.
        /// </summary>
        /// <param name="context">The Operation request context.</param>
        public OperationRequestItem(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            Context = context;
        }

        /// <summary>
        /// Gets the Operation request context.
        /// </summary>
        public HttpContext Context { get; private set; }

        /// <summary>
        /// Sends the Operation request.
        /// </summary>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>A <see cref="OperationResponseItem"/>.</returns>
        public override async Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler)
        {
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            await SendRequestAsync(handler, Context, this.ContentIdToLocationMapping);
            return new OperationResponseItem(Context);
        }
    }
}
