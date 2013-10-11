// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.WebHost
{
    public class WebHostExceptionCatchBlocksTests
    {
        [Fact]
        public void HttpControllerHandlerWriteBufferedResponseContentAsync_IsSpecifiedValue()
        {
            // Arrange, Act & Assert
            Assert.Equal("HttpControllerHandler.WriteBufferedResponseContentAsync",
                WebHostExceptionCatchBlocks.HttpControllerHandlerWriteBufferedResponseContentAsync);
        }

        [Fact]
        public void HttpControllerHandlerWriteErrorResponseContentAsync_IsSpecifiedValue()
        {
            // Arrange, Act & Assert
            Assert.Equal("HttpControllerHandler.WriteErrorResponseContentAsync",
                WebHostExceptionCatchBlocks.HttpControllerHandlerWriteErrorResponseContentAsync);
        }

        [Fact]
        public void HttpControllerHandlerWriteStreamedResponseContentAsync_IsSpecifiedValue()
        {
            // Arrange, Act & Assert
            Assert.Equal("HttpControllerHandler.WriteStreamedResponseContentAsync",
                WebHostExceptionCatchBlocks.HttpControllerHandlerWriteStreamedResponseContentAsync);
        }

        [Fact]
        public void HttpWebRoute_IsSpecifiedValue()
        {
            // Arrange, Act & Assert
            Assert.Equal("HttpWebRoute", WebHostExceptionCatchBlocks.HttpWebRoute);
        }
    }
}
