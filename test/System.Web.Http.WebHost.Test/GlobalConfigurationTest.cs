// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.WebHost;
using System.Web.Http.WebHost.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class GlobalConfigurationTest
    {
        [Fact]
        public void ConfigurationRouteCollection_IsHostedHttpRouteCollection()
        {
            // Arrange
            HttpConfiguration configuration = GlobalConfiguration.Configuration;
            Assert.NotNull(configuration); // Guard

            // Act
            HttpRouteCollection routes = configuration.Routes;

            // Assert
            Assert.IsType<HostedHttpRouteCollection>(routes);
            // Note that there is currently no easy way to test that it references RouteTable.Routes.
        }

        [Theory]
        [InlineData(typeof(IAssembliesResolver), typeof(WebHostAssembliesResolver))]
        [InlineData(typeof(IHttpControllerTypeResolver), typeof(WebHostHttpControllerTypeResolver))]
        [InlineData(typeof(IHostBufferPolicySelector), typeof(WebHostBufferPolicySelector))]
        [InlineData(typeof(IHostPrincipalService), typeof(WebHostPrincipalService))]
        public void ConfigurationService_IsWebHost(Type serviceInterfaceType, Type expectedImplementationType)
        {
            // Arrange
            HttpConfiguration configuration = GlobalConfiguration.Configuration;
            Assert.NotNull(configuration); // Guard
            Assert.NotNull(configuration.Services); // Guard

            // Act
            object service = configuration.Services.GetService(serviceInterfaceType);

            // Assert
            Assert.IsType(expectedImplementationType, service);
        }

        [Fact]
        public void DefaultHandler_IsHttpRoutingDispatcher()
        {
            // Act
            HttpMessageHandler handler = GlobalConfiguration.DefaultHandler;

            // Assert
            Assert.IsType<HttpRoutingDispatcher>(handler);
            // Note that there is currently no easy way to test it references GlobalConfiguration.Configuration.
        }

        [Fact]
        public void DefaultServer_ReferencesConfiguration()
        {
            // Act
            HttpServer server = GlobalConfiguration.DefaultServer;

            // Assert
            Assert.NotNull(server);
            Assert.Same(GlobalConfiguration.Configuration, server.Configuration);
        }

        [Fact]
        public void DefaultServer_ReferencesDefaultHandler()
        {
            // Act
            HttpServer server = GlobalConfiguration.DefaultServer;

            // Assert
            Assert.NotNull(server);
            Assert.Same(GlobalConfiguration.DefaultHandler, server.Dispatcher);
        }
    }
}
