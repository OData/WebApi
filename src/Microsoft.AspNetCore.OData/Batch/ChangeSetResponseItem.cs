// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
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
        /// <param name="contexts">The response contexts for the ChangeSet requests.</param>
        public ChangeSetResponseItem(IEnumerable<HttpContext> contexts)
        {
            if (contexts == null)
            {
                throw Error.ArgumentNull("contexts");
            }

            Contexts = contexts;
        }

        /// <summary>
        /// Gets the response contexts for the ChangeSet.
        /// </summary>
        public IEnumerable<HttpContext> Contexts { get; private set; }

        /// <summary>
        /// Writes the responses as a ChangeSet.
        /// </summary>
        /// <param name="writer">The <see cref="ODataBatchWriter"/>.</param>
        public override async Task WriteResponseAsync(ODataBatchWriter writer)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            await writer.WriteStartChangesetAsync();

            foreach (HttpContext context in Contexts)
            {
                await WriteMessageAsync(writer, context);
            }

            await writer.WriteEndChangesetAsync();
        }

        /// <summary>
        /// Gets a value that indicates if the responses in this item are successful.
        /// </summary>
        internal override bool IsResponseSuccessful()
        {
            return Contexts.All(c => c.Response.IsSuccessStatusCode());
        }
    }
}