// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Batch
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
        public override async Task WriteResponseAsync(ODataBatchWriter writer, CancellationToken cancellationToken)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            writer.WriteStartChangeset();

            foreach (HttpResponseMessage responseMessage in Responses)
            {
                await WriteMessageAsync(writer, responseMessage, cancellationToken);
            }

            writer.WriteEndChangeset();
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