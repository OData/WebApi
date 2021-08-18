//-----------------------------------------------------------------------------
// <copyright file="ChangeSetRequestItem.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Represents a ChangeSet request.
    /// </summary>
    public class ChangeSetRequestItem : ODataBatchRequestItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSetRequestItem"/> class.
        /// </summary>
        /// <param name="contexts">The request contexts in the ChangeSet.</param>
        public ChangeSetRequestItem(IEnumerable<HttpContext> contexts)
        {
            if (contexts == null)
            {
                throw Error.ArgumentNull("contexts");
            }

            Contexts = contexts;
        }

        /// <summary>
        /// Gets the request contexts in the ChangeSet.
        /// </summary>
        public IEnumerable<HttpContext> Contexts { get; private set; }

        /// <summary>
        /// Sends the ChangeSet request.
        /// </summary>
        /// <param name="handler">The handler for processing a message.</param>
        /// <returns>A <see cref="ChangeSetResponseItem"/>.</returns>
        public override async Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler)
        {
            if (handler == null)
            {
                throw Error.ArgumentNull("handler");
            }

            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            List<HttpContext> responseContexts = new List<HttpContext>();

            foreach (HttpContext context in Contexts)
            {
                await SendRequestAsync(handler, context, contentIdToLocationMapping);

                HttpResponse response = context.Response;
                if (response.IsSuccessStatusCode())
                {
                    responseContexts.Add(context);
                }
                else
                {
                    responseContexts.Clear();
                    responseContexts.Add(context);
                    return new ChangeSetResponseItem(responseContexts);
                }
            }

            return new ChangeSetResponseItem(responseContexts);
        }
    }
}
