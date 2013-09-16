// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;
using Microsoft.TestCommon;
using Moq;
using Owin;

namespace System.Web.Http.Owin
{
    public class WebApiAppBuilderExtensionsTest
    {
        [Fact]
        public void UseWebApi_UsesAdapter()
        {
            var config = new HttpConfiguration();
            var appBuilder = new Mock<IAppBuilder>();
            appBuilder
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    It.Is<HttpServer>(s => s.Configuration == config),
                    It.IsAny<OwinBufferPolicySelector>()))
                .Returns(appBuilder.Object)
                .Verifiable();

            IAppBuilder returnedAppBuilder = appBuilder.Object.UseWebApi(config);

            Assert.Equal(appBuilder.Object, returnedAppBuilder);
            appBuilder.Verify();
        }

        [Fact]
        public void UseWebApi_UsesAdapterAndConfigBufferPolicySelector()
        {
            var config = new HttpConfiguration();
            var bufferPolicySelector = new Mock<IHostBufferPolicySelector>().Object;
            config.Services.Replace(typeof(IHostBufferPolicySelector), bufferPolicySelector);
            var appBuilder = new Mock<IAppBuilder>();
            appBuilder
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    It.Is<HttpServer>(s => s.Configuration == config),
                    bufferPolicySelector))
                .Returns(appBuilder.Object)
                .Verifiable();

            IAppBuilder returnedAppBuilder = appBuilder.Object.UseWebApi(config);

            Assert.Equal(appBuilder.Object, returnedAppBuilder);
            appBuilder.Verify();
        }

        [Fact]
        public void UseWebApiWithHttpServer_UsesAdapter()
        {
            // Arrange
            HttpServer httpServer = new Mock<HttpServer>().Object;
            Mock<IAppBuilder> appBuilderMock = new Mock<IAppBuilder>();
            appBuilderMock
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    httpServer,
                    It.IsAny<OwinBufferPolicySelector>()))
                .Returns(appBuilderMock.Object)
                .Verifiable();

            // Act
            IAppBuilder returnedAppBuilder = appBuilderMock.Object.UseWebApi(httpServer);

            // Assert
            Assert.Equal(appBuilderMock.Object, returnedAppBuilder);
            appBuilderMock.Verify();
        }

        [Fact]
        public void UseWebApiWithHttpServer_UsesAdapterAndConfigBufferPolicySelector()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            IHostBufferPolicySelector bufferPolicySelector = new Mock<IHostBufferPolicySelector>().Object;
            config.Services.Replace(typeof(IHostBufferPolicySelector), bufferPolicySelector);
            HttpServer httpServer = new Mock<HttpServer>(config).Object;
            Mock<IAppBuilder> appBuilderMock = new Mock<IAppBuilder>();
            appBuilderMock
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    httpServer,
                    bufferPolicySelector))
                .Returns(appBuilderMock.Object)
                .Verifiable();

            // Act
            IAppBuilder returnedAppBuilder = appBuilderMock.Object.UseWebApi(httpServer);

            // Assert
            Assert.Equal(appBuilderMock.Object, returnedAppBuilder);
            appBuilderMock.Verify();
        }
    }
}
