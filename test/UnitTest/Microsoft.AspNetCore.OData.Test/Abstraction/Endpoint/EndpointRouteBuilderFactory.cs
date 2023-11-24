//-----------------------------------------------------------------------------
// <copyright file="EndpointRouteBuilderFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCOREAPP2_1
using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create IEndpointRouteBuilder.
    /// </summary>
    public class EndpointRouteBuilderFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IEndpointRouteBuilder Create()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMvc();
            serviceCollection.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            serviceCollection.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            serviceCollection.AddOData();

            IApplicationBuilder appBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Create the Mock of IEndpointRouteBuilder
            return new MockEndpointRouteBuilder(appBuilder);
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IEndpointRouteBuilder Create(string routeName, Action<IContainerBuilder> configureAction = null)
        {
            IEndpointRouteBuilder builder = Create();
            if (!string.IsNullOrEmpty(routeName))
            {
                // Build and configure the root container.
                IPerRouteContainer perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
                if (perRouteContainer == null)
                {
                    throw Error.ArgumentNull("routeName");
                }

                // Create an service provider for this route. Add the default services to the custom configuration actions.
                Action<IContainerBuilder> builderAction =
                    ODataEndpointRouteBuilderExtensions.ConfigureDefaultServices(builder, configureAction);

                perRouteContainer.CreateODataRootContainer(routeName, builderAction);
            }

            return builder;
        }
    }
}
#endif
