//-----------------------------------------------------------------------------
// <copyright file="ODataApplicationBuilderExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IApplicationBuilder"/> to add OData routes.
    /// </summary>
    public static class ODataApplicationBuilderExtensions
    {
        private static readonly int MaxTop = 100;

        /// <summary>
        /// The default OData route name.
        /// </summary>
        public readonly static string DefaultRouteName = "odata";

        /// <summary>
        /// The default OData route prefix.
        /// </summary>
        public readonly static string DefaultRoutePrefix = "odata";

        /// <summary>
        /// Use OData batching middleware.
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

        /// <summary>
        /// Use OData route with default route name and route prefix.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <param name="model">The <see cref="IEdmModel"/> to use.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseOData(this IApplicationBuilder app, IEdmModel model)
        {
            return app.UseOData(DefaultRouteName, DefaultRoutePrefix, model);
        }

        /// <summary>
        /// Use OData route with given route name and route prefix.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder "/> to use.</param>
        /// <param name="routeName">The given OData route name.</param>
        /// <param name="routePrefix">The given OData route prefix.</param>
        /// <param name="model">The <see cref="IEdmModel"/> to use.</param>
        /// <returns>The <see cref="IApplicationBuilder "/>.</returns>
        public static IApplicationBuilder UseOData(this IApplicationBuilder app, string routeName, string routePrefix, IEdmModel model)
        {
            if (app == null)
            {
                throw Error.ArgumentNull("app");
            }

            VerifyODataIsRegistered(app);

            return app.UseMvc(b =>
            {
                b.Select().Expand().Filter().OrderBy().MaxTop(MaxTop).Count();

                b.MapODataServiceRoute(routeName, routePrefix, model);
            });
        }

        private static void VerifyODataIsRegistered(IApplicationBuilder app)
        {
            // We use the IPerRouteContainer to verify if AddOData() was called before calling UseOData
            if (app.ApplicationServices.GetService(typeof(IPerRouteContainer)) == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
            }
        }
    }
}
