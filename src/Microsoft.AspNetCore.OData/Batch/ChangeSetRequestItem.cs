// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.Extensions;

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
                ILoggerFactory loggeFactory = context.RequestServices.GetService<ILoggerFactory>();
                ILogger logger = loggeFactory.CreateLogger<ODataBatchHandler>();
                string requestUri = context.Request.GetDisplayUrl();
                logger.LogInformation($"[ODataInfo:] SendSubRequestAsync to requestUri='{requestUri}' starting ...");

                await SendRequestAsync(handler, context, contentIdToLocationMapping);

                logger.LogInformation($"[ODataInfo:] SendSubRequestAsync to requestUri='{requestUri}' End");

                HttpResponse response = context.Response;
                if (response.IsSuccessStatusCode())
                {
                    logger.LogInformation($"[ODataInfo:] SendSubRequestAsync, requestUri='{requestUri}' Successes ...");

                    responseContexts.Add(context);
                }
                else
                {
                    logger.LogInformation($"[ODataInfo:] SendSubRequestAsync, requestUri='{requestUri}' Failed ...");

                    responseContexts.Clear();
                    responseContexts.Add(context);
                    return new ChangeSetResponseItem(responseContexts);
                }
            }

            return new ChangeSetResponseItem(responseContexts);
        }
    }
}