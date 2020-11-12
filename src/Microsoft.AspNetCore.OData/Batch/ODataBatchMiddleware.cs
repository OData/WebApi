// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Batch
{
    /// <summary>
    /// Defines the middleware for handling OData batch requests. This middleware essentially
    /// acts like branching middleware, <see cref="MapExtensions "/>, and redirects OData batch
    /// requests to the appropriate ODataBatchHandler.
    /// </summary>
    public class ODataBatchMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Instantiates a new instance of <see cref="ODataBatchMiddleware"/>.
        /// </summary>
        public ODataBatchMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invoke the middleware.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <returns>A task that can be awaited.</returns>
        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var isPreFlight = request.Method.Equals("Options", StringComparison.OrdinalIgnoreCase);
            if (isPreFlight)
            {
                // if the request is a preflight request let the next handler be called as trying to handle the request will result in preflight requests failing.
                await this.next(context);
                return;
            }

            // Attempt to match the path to a bach route.
            ODataBatchPathMapping batchMapping = context.RequestServices.GetRequiredService<ODataBatchPathMapping>();

            if (batchMapping.TryGetRouteName(context, out var routeName))
            {
                // Get the per-route container and retrieve the batch handler.
                IPerRouteContainer perRouteContainer = context.RequestServices.GetRequiredService<IPerRouteContainer>();
                if (perRouteContainer == null)
                {
                    throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
                }

                IServiceProvider rootContainer = perRouteContainer.GetODataRootContainer(routeName);
                ODataBatchHandler batchHandler = rootContainer.GetRequiredService<ODataBatchHandler>();

                await batchHandler.ProcessBatchAsync(context, next);
            }
            else
            {
                await this.next(context);
            }
        }
    }
}
