// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IApplicationBuilder"/> to add OData routes.
    /// </summary>
    public static class ODataApplicationBuilderExtensions
    {
        /// <summary>
        /// USe OData batching middleware.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseODataBatching(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw Error.ArgumentNull("app");
            }

            return app.UseMiddleware<ODataBatchMiddleware>();
        }
    }
}
