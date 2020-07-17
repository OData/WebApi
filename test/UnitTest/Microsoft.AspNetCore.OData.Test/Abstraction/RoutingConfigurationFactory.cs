// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
#if NETCOREAPP2_0
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.OData;
using Moq;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create IRouteBuilder/HttpConfiguration.
    /// </summary>
    public class RoutingConfigurationFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IRouteBuilder Create()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMvc();
            serviceCollection.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            serviceCollection.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            serviceCollection.AddOData();

            // For routing tests, add an IActionDescriptorCollectionProvider.
            serviceCollection.AddSingleton<IActionDescriptorCollectionProvider, TestActionDescriptorCollectionProvider>();

            // Add an action select to return a default descriptor.
            var mockAction = new Mock<ActionDescriptor>();
            ActionDescriptor actionDescriptor = mockAction.Object;

            var mockActionSelector = new Mock<IActionSelector>();
            mockActionSelector
                .Setup(a => a.SelectCandidates(It.IsAny<RouteContext>()))
                .Returns(new ActionDescriptor[] { actionDescriptor });

            mockActionSelector
                .Setup(a => a.SelectBestCandidate(It.IsAny<RouteContext>(), It.IsAny<IReadOnlyList<ActionDescriptor>>()))
                .Returns(actionDescriptor);

            // Add a mock action invoker & factory.
            var mockInvoker = new Mock<IActionInvoker>();
            mockInvoker.Setup(i => i.InvokeAsync())
                .Returns(Task.FromResult(true));

            var mockInvokerFactory = new Mock<IActionInvokerFactory>();
            mockInvokerFactory.Setup(f => f.CreateInvoker(It.IsAny<ActionContext>()))
                .Returns(mockInvoker.Object);

            // Create a logger, diagnostic source and app builder.
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            IApplicationBuilder appBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Create a route build with a default path handler.
            IRouteBuilder routeBuilder = new RouteBuilder(appBuilder);

#if NETCOREAPP2_0
            routeBuilder.DefaultHandler = new MvcRouteHandler(
                mockInvokerFactory.Object,
                mockActionSelector.Object,
                diagnosticSource,
                mockLoggerFactory.Object,
                new ActionContextAccessor());
#else
            //appBuilder.ApplicationServices.GetRequiredService<MvcRouteHandler>();
            routeBuilder.DefaultHandler = new MyMvcRouteHandler();
#endif

            return routeBuilder;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IRouteBuilder CreateWithRoute(string route)
        {
            // TODO: Need to add the route to the prefix.
            IRouteBuilder routeBuilder = Create();
            return routeBuilder;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IRouteBuilder CreateWithRootContainer(string routeName, Action<IContainerBuilder> configureAction = null)
        {
            IRouteBuilder builder = Create();
            if (!string.IsNullOrEmpty(routeName))
            {
                // Build and configure the root container.
                IPerRouteContainer perRouteContainer = builder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
                if (perRouteContainer == null)
                {
                    throw Error.ArgumentNull("routeName");
                }

                // Create an service provider for this route. Add the default services to the custom configuration actions.
                Action<IContainerBuilder> builderAction = ODataRouteBuilderExtensions.ConfigureDefaultServices(builder, configureAction);
                IServiceProvider serviceProvider = perRouteContainer.CreateODataRootContainer(routeName, builderAction);
            }

            return builder;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IRouteBuilder CreateWithTypes(params Type[] types)
        {
            IRouteBuilder builder = Create();
            builder.Count().OrderBy().Filter().Expand().MaxTop(null);

            ApplicationPartManager applicationPartManager = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            AssemblyPart part = new AssemblyPart(new MockAssembly(types));
            applicationPartManager.ApplicationParts.Add(part);

            return builder;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IRouteBuilder CreateWithRootContainerAndTypes(string routeName = null, Action<IContainerBuilder> configureAction = null, params Type[] types)
        {
            IRouteBuilder builder = CreateWithRootContainer(routeName, configureAction);

            ApplicationPartManager applicationPartManager = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            AssemblyPart part = new AssemblyPart(new MockAssembly(types));
            applicationPartManager.ApplicationParts.Add(part);

            return builder;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IRouteBuilder CreateWithDisabledCaseInsensitiveRequestPropertyBinding()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddMvc();
            serviceCollection.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            serviceCollection.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            serviceCollection.AddOData();

            // For routing tests, add an IActionDescriptorCollectionProvider.
            serviceCollection.AddSingleton<IActionDescriptorCollectionProvider, TestActionDescriptorCollectionProvider>();

            // Add an action select to return a default descriptor.
            var mockAction = new Mock<ActionDescriptor>();
            ActionDescriptor actionDescriptor = mockAction.Object;

            var mockActionSelector = new Mock<IActionSelector>();
            mockActionSelector
                .Setup(a => a.SelectCandidates(It.IsAny<RouteContext>()))
                .Returns(new ActionDescriptor[] { actionDescriptor });

            mockActionSelector
                .Setup(a => a.SelectBestCandidate(It.IsAny<RouteContext>(), It.IsAny<IReadOnlyList<ActionDescriptor>>()))
                .Returns(actionDescriptor);

            // Add a mock action invoker & factory.
            var mockInvoker = new Mock<IActionInvoker>();
            mockInvoker.Setup(i => i.InvokeAsync())
                .Returns(Task.FromResult(true));

            var mockInvokerFactory = new Mock<IActionInvokerFactory>();
            mockInvokerFactory.Setup(f => f.CreateInvoker(It.IsAny<ActionContext>()))
                .Returns(mockInvoker.Object);

            // Create a logger, diagnostic source and app builder.
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            IApplicationBuilder appBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Create a route build with a default path handler.
            IRouteBuilder routeBuilder = new RouteBuilder(appBuilder);

#if NETCOREAPP2_0
            routeBuilder.DefaultHandler = new MvcRouteHandler(
                mockInvokerFactory.Object,
                mockActionSelector.Object,
                diagnosticSource,
                mockLoggerFactory.Object,
                new ActionContextAccessor());
#else
            //appBuilder.ApplicationServices.GetRequiredService<MvcRouteHandler>();
            routeBuilder.DefaultHandler = new MyMvcRouteHandler();
#endif
            routeBuilder.SetCompatibilityOptions(CompatibilityOptions.DisableCaseInsensitiveRequestPropertyBinding);
            return routeBuilder;
        }
    }

    internal class MyMvcRouteHandler : IRouter
    {
        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            return Task.CompletedTask;
        }
    }
}