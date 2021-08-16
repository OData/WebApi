//-----------------------------------------------------------------------------
// <copyright file="ChangeSetResponseItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Represents a ChangeSet response.
    /// </summary>
    public class ChangeSetResponseItem : ODataBatchResponseItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSetResponseItem"/> class.
        /// </summary>
        /// <param name="responses">The response messages for the ChangeSet requests.</param>
        public ChangeSetResponseItem(IEnumerable<HttpResponseMessage> responses)
        {
            if (responses == null)
            {
                throw Error.ArgumentNull("responses");
            }

            Responses = responses;
        }

        /// <summary>
        /// Gets the response messages for the ChangeSet.
        /// </summary>
        public IEnumerable<HttpResponseMessage> Responses { get; private set; }

        /// <summary>
        /// Writes the responses as a ChangeSet.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="asyncWriter">Whether or not the writer is writing asynchronously.</param>
        public override async Task WriteResponseAsync(ODataBatchWriter writer, CancellationToken cancellationToken, bool asyncWriter)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (asyncWriter)
            {
                await writer.WriteStartChangesetAsync();

                foreach (HttpResponseMessage responseMessage in Responses)
                {
                    await WriteMessageAsync(writer, responseMessage, cancellationToken, asyncWriter);
                }

                await writer.WriteEndChangesetAsync();
            }
            else
            {
                writer.WriteStartChangeset();

                foreach (HttpResponseMessage responseMessage in Responses)
                {
                    await WriteMessageAsync(writer, responseMessage, cancellationToken, asyncWriter);
                }

                writer.WriteEndChangeset();
            }
        }
        /// <summary>
        /// Gets a value that indicates if the responses in this item are successful.
        /// </summary>
        internal override bool IsResponseSuccessful()
        {
            return Responses.All(r => r.IsSuccessStatusCode);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (HttpResponseMessage response in Responses)
                {
                    if (response != null)
                    {
                        response.Dispose();
                    }
                }
            }
        }
    }
}
