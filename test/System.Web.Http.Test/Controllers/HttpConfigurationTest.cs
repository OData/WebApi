// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class HttpConfigurationTest
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<HttpConfiguration>(TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsDisposable);
        }

        [Fact]
        public void Default_Constructor()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            Assert.Empty(configuration.Filters);
            Assert.NotEmpty(configuration.Formatters);
            Assert.Empty(configuration.MessageHandlers);
            Assert.Empty(configuration.Properties);
            Assert.Empty(configuration.Routes);
            Assert.NotNull(configuration.DependencyResolver);
            Assert.NotNull(configuration.Services);
            Assert.Equal("/", configuration.VirtualPathRoot);
        }

        [Fact]
        public void Parameter_Constructor()
        {
            string path = "/some/path";
            HttpRouteCollection routes = new HttpRouteCollection(path);
            HttpConfiguration configuration = new HttpConfiguration(routes);

            Assert.Same(routes, configuration.Routes);
            Assert.Equal(path, configuration.VirtualPathRoot);
        }

        [Fact]
        public void Dispose_Idempotent()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Dispose();
            configuration.Dispose();
        }

        [Fact]
        public void Dispose_DisposesOfServices()
        {
            // Arrange
            var configuration = new HttpConfiguration();
            var services = new Mock<DefaultServices> { CallBase = true };
            configuration.Services = services.Object;

            // Act
            configuration.Dispose();

            // Assert
            services.Verify(s => s.Dispose(), Times.Once());
        }

        [Fact]
        public void DependencyResolver_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();

            // Act & assert
            Assert.ThrowsArgumentNull(() => config.DependencyResolver = null, "value");
        }
    }
}
