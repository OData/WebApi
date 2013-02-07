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
        public void UseWebApiWithMessageHandler_UsesAdapter()
        {
            var config = new HttpConfiguration();
            var dispatcher = new Mock<HttpMessageHandler>().Object;
            var appBuilder = new Mock<IAppBuilder>();
            appBuilder
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    It.Is<HttpServer>(s => s.Configuration == config && s.Dispatcher == dispatcher),
                    It.IsAny<OwinBufferPolicySelector>()))
                .Returns(appBuilder.Object)
                .Verifiable();

            IAppBuilder returnedAppBuilder = appBuilder.Object.UseWebApi(config, dispatcher);

            Assert.Equal(appBuilder.Object, returnedAppBuilder);
            appBuilder.Verify();
        }

        [Fact]
        public void UseWebApiWithMessageHandler_UsesAdapterAndConfigBufferPolicySelector()
        {
            var config = new HttpConfiguration();
            var bufferPolicySelector = new Mock<IHostBufferPolicySelector>().Object;
            config.Services.Replace(typeof(IHostBufferPolicySelector), bufferPolicySelector);
            var dispatcher = new Mock<HttpMessageHandler>().Object;
            var appBuilder = new Mock<IAppBuilder>();
            appBuilder
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    It.Is<HttpServer>(s => s.Configuration == config && s.Dispatcher == dispatcher),
                    bufferPolicySelector))
                .Returns(appBuilder.Object)
                .Verifiable();

            IAppBuilder returnedAppBuilder = appBuilder.Object.UseWebApi(config, dispatcher);

            Assert.Equal(appBuilder.Object, returnedAppBuilder);
            appBuilder.Verify();
        }

        [Fact]
        public void UseHttpMessageHandler_UsesAdapter()
        {
            var messageHandler = new Mock<HttpMessageHandler>().Object;
            var appBuilder = new Mock<IAppBuilder>();
            appBuilder
                .Setup(ab => ab.Use(
                    typeof(HttpMessageHandlerAdapter),
                    messageHandler,
                    It.IsAny<OwinBufferPolicySelector>()))
                .Returns(appBuilder.Object)
                .Verifiable();

            IAppBuilder returnedAppBuilder = appBuilder.Object.UseHttpMessageHandler(messageHandler);

            Assert.Equal(appBuilder.Object, returnedAppBuilder);
            appBuilder.Verify();
        }
    }
}
