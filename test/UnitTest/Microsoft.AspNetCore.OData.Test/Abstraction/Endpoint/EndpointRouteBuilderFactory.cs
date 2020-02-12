// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCOREAPP2_0
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.ObjectPool;
using Microsoft.OData;
using System;
using System.Net.Http;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create IRouteBuilder/HttpConfiguration.
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

            //// For routing tests, add an IActionDescriptorCollectionProvider.
            //serviceCollection.AddSingleton<IActionDescriptorCollectionProvider, TestActionDescriptorCollectionProvider>();

            //// Add an action select to return a default descriptor.
            //var mockAction = new Mock<ActionDescriptor>();
            //ActionDescriptor actionDescriptor = mockAction.Object;

            //var mockActionSelector = new Mock<IActionSelector>();
            //mockActionSelector
            //    .Setup(a => a.SelectCandidates(It.IsAny<RouteContext>()))
            //    .Returns(new ActionDescriptor[] { actionDescriptor });

            //mockActionSelector
            //    .Setup(a => a.SelectBestCandidate(It.IsAny<RouteContext>(), It.IsAny<IReadOnlyList<ActionDescriptor>>()))
            //    .Returns(actionDescriptor);

            //// Add a mock action invoker & factory.
            //var mockInvoker = new Mock<IActionInvoker>();
            //mockInvoker.Setup(i => i.InvokeAsync())
            //    .Returns(Task.FromResult(true));

            //var mockInvokerFactory = new Mock<IActionInvokerFactory>();
            //mockInvokerFactory.Setup(f => f.CreateInvoker(It.IsAny<ActionContext>()))
            //    .Returns(mockInvoker.Object);

            //// Create a logger, diagnostic source and app builder.
            //var mockLoggerFactory = new Mock<ILoggerFactory>();
            //var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");

            IApplicationBuilder appBuilder = new ApplicationBuilder(serviceCollection.BuildServiceProvider());

            // Create the Mock of IEndpointRouteBuilder
            return new MockEndpointRouteBuilder(appBuilder);
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static IEndpointRouteBuilder CreateWithRootContainer(string routeName,
            Action<IContainerBuilder> configureAction = null)
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
                IServiceProvider serviceProvider = perRouteContainer.CreateODataRootContainer(routeName, builderAction);
            }

            return builder;
        }

        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        public static HttpRequest Create(HttpMethod method, string uri, IEndpointRouteBuilder routeBuilder = null, string routeName = null)
        {
            HttpRequest request = Create(routeBuilder, routeName);
            request.Method = method.ToString();

            Uri requestUri = new Uri(uri);
            request.Scheme = requestUri.Scheme;
            request.Host = requestUri.IsDefaultPort ?
                new HostString(requestUri.Host) :
                new HostString(requestUri.Host, requestUri.Port);
            request.QueryString = new QueryString(requestUri.Query);
            request.Path = new PathString(requestUri.AbsolutePath);

            if (path != null)
            {
                request.ODataFeature().Path = path;
            }

            return request;
        }
    }
}
#endif