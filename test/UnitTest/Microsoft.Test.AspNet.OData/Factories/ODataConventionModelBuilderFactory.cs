// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Globalization;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;
#else
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
{
    /// <summary>
    /// A class to create ODataConventionModelBuilder.
    /// </summary>
    public class ODataConventionModelBuilderFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder Create()
        {
#if NETCORE
            // Create an application part manager with both the product and test assemblies.
            ApplicationPartManager applicationPartManager = new ApplicationPartManager();
            applicationPartManager.ApplicationParts.Add(new AssemblyPart(typeof(ODataConventionModelBuilder).Assembly));
            applicationPartManager.ApplicationParts.Add(new AssemblyPart(typeof(ODataConventionModelBuilderFactory).Assembly));

            // Also, a few tests are built on CultureInfo so include it as well.
            applicationPartManager.ApplicationParts.Add(new AssemblyPart(typeof(CultureInfo).Assembly));

            IContainerBuilder container = new DefaultContainerBuilder();
            container.AddService(ServiceLifetime.Singleton, sp => applicationPartManager);

            IServiceProvider serviceProvider = container.BuildContainer();
            return new ODataConventionModelBuilder(serviceProvider);
#else
            return new ODataConventionModelBuilder();
#endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
#if NETCORE
        public static ODataConventionModelBuilder Create(IRouteBuilder routeBuilder)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider);
        }
#else
        public static ODataConventionModelBuilder Create(HttpConfiguration configuration)
        {
            return new ODataConventionModelBuilder(configuration);
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="isQueryCompositionMode">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
#if NETCORE
        public static ODataConventionModelBuilder Create(IRouteBuilder routeBuilder, bool isQueryCompositionMode)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider, isQueryCompositionMode);
        }
#else
        public static ODataConventionModelBuilder Create(HttpConfiguration configuration, bool isQueryCompositionMode)
        {
            return new ODataConventionModelBuilder(configuration, isQueryCompositionMode);
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="modelAliasing">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder CreateWithModelAliasing(bool modelAliasing)
        {
            ODataConventionModelBuilder builder = Create();
            builder.ModelAliasingEnabled = modelAliasing;
            return builder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="modelAliasing">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
#if NETCORE
        public static ODataConventionModelBuilder CreateWithModelAliasing(IRouteBuilder routeBuilder, bool modelAliasing)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider) { ModelAliasingEnabled = modelAliasing };
        }
#else
        public static ODataConventionModelBuilder CreateWithModelAliasing(HttpConfiguration configuration, bool modelAliasing)
        {
            return new ODataConventionModelBuilder(configuration) { ModelAliasingEnabled = modelAliasing };
        }
#endif
    }
}
