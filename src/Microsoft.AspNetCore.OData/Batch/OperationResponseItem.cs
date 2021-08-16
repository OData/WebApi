//-----------------------------------------------------------------------------
// <copyright file="OperationResponseItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Represents an Operation response.
    /// </summary>
    public class OperationResponseItem : ODataBatchResponseItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationResponseItem"/> class.
        /// </summary>
        /// <param name="context">The response context for the Operation request.</param>
        public OperationResponseItem(HttpContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            Context = context;
        }

        /// <summary>
        /// Gets the response messages for the Operation.
        /// </summary>
        public HttpContext Context { get; private set; }

        /// <summary>
        /// Writes the response as an Operation.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="asyncWriter">Whether or not the writer is in async mode. </param>
        public override Task WriteResponseAsync(ODataBatchWriter writer, bool asyncWriter)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            return WriteMessageAsync(writer, Context, asyncWriter);
        }

        /// <summary>
        /// Gets a value that indicates if the responses in this item are successful.
        /// </summary>
        internal override bool IsResponseSuccessful()
        {
            return Context.Response.IsSuccessStatusCode();
        }
    }
}
