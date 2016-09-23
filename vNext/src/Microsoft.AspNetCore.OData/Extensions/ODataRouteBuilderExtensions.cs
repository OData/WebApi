// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IRouteBuilder"/> to add OData routes.
    /// </summary>
    public static class ODataRouteBuilderExtensions
    {
        /// <summary>
        /// Adds an OData route to the <see cref="IRouteBuilder"/> with the specified the Edm model.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="model">The Edm Model.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IRouteBuilder MapODataRoute(this IRouteBuilder builder, IEdmModel model)
        {
            return builder.MapODataRoute(prefix: null, model: model);
        }

        /// <summary>
        /// Adds an OData route to the <see cref="IRouteBuilder"/> with the specified the route prefix and the Edm model.
        /// </summary>
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <param name="prefix">The route prefix.</param>
        /// <param name="model">The Edm Model.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IRouteBuilder MapODataRoute(this IRouteBuilder builder, string prefix, IEdmModel model)
        {
            IRouter target = builder.ServiceProvider.GetRequiredService<MvcRouteHandler>();

            var inlineConstraintResolver = builder
                .ServiceProvider
                .GetRequiredService<IInlineConstraintResolver>();

            ODataRouteConstraint constraint = new ODataRouteConstraint(prefix, model);
            builder.Routes.Add(new ODataRoute(target, prefix, constraint, inlineConstraintResolver));

            // add a mapping between the route prefix and the EDM model.
            ODataOptions options = builder.ServiceProvider.GetRequiredService<IOptions<ODataOptions>>().Value;
            options.ModelManager.AddModel(prefix, model);

            return builder;
        }
    }
}
