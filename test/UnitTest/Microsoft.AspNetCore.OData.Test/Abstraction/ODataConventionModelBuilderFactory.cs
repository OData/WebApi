//-----------------------------------------------------------------------------
// <copyright file="ODataConventionModelBuilderFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create ODataConventionModelBuilder.
    /// </summary>
    public static class ODataConventionModelBuilderFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder Create()
        {
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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder Create(IRouteBuilder routeBuilder)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="isQueryCompositionMode">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder Create(IRouteBuilder routeBuilder, bool isQueryCompositionMode)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider, isQueryCompositionMode);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="modelAliasing">The value for ModelAliasingEnabled.</param>
        /// <returns>A new instance of the <see cref="ODataConventionModelBuilder"/> class.</returns>
        public static ODataConventionModelBuilder CreateWithModelAliasing(IRouteBuilder routeBuilder, bool modelAliasing)
        {
            return new ODataConventionModelBuilder(routeBuilder.ServiceProvider) { ModelAliasingEnabled = modelAliasing };
        }
    }
}
