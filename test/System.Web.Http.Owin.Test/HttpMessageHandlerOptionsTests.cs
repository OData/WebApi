// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http.Owin
{
    public class HttpMessageHandlerOptionsTests
    {
        [Fact]
        public void MessageHandlerGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpMessageHandlerOptions product = CreateProductUnderTest();

            using (HttpMessageHandler expectedMessageHandler = CreateDummyMessageHandler())
            {
                product.MessageHandler = expectedMessageHandler;

                // Act
                HttpMessageHandler messageHandler = product.MessageHandler;

                // Assert
                Assert.Same(expectedMessageHandler, messageHandler);
            }
        }

        [Fact]
        public void BufferPolicySelectorGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpMessageHandlerOptions product = CreateProductUnderTest();
            IHostBufferPolicySelector expectedBufferPolicySelector = CreateDummyBufferPolicy();
            product.BufferPolicySelector = expectedBufferPolicySelector;

            // Act
            IHostBufferPolicySelector bufferPolicy = product.BufferPolicySelector;

            // Assert
            Assert.Same(expectedBufferPolicySelector, bufferPolicy);
        }

        private static IHostBufferPolicySelector CreateDummyBufferPolicy()
        {
            return new Mock<IHostBufferPolicySelector>(MockBehavior.Strict).Object;
        }

        private static HttpMessageHandler CreateDummyMessageHandler()
        {
            Mock<HttpMessageHandler> mock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            return mock.Object;
        }

        private static HttpMessageHandlerOptions CreateProductUnderTest()
        {
            return new HttpMessageHandlerOptions();
        }
    }
}
