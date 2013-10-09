// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionCatchBlocksTests
    {
        [Fact]
        public void HttpBatchHandler_IsSpecifiedValue()
        {
            // Arrange, Act & Assert
            Assert.Equal("HttpBatchHandler", ExceptionCatchBlocks.HttpBatchHandler);
        }

        [Fact]
        public void HttpControllerDispatcher_IsSpecifiedValue()
        {
            // Arrange, Act & Assert
            Assert.Equal("HttpControllerDispatcher", ExceptionCatchBlocks.HttpControllerDispatcher);
        }

        [Fact]
        public void HttpServer_IsSpecifiedValue()
        {
            // Arrange, Act & Assert
            Assert.Equal("HttpServer", ExceptionCatchBlocks.HttpServer);
        }

        [Fact]
        public void IExceptionFilter_IsSpecifiedValue()
        {
            // Arrange, Act & Assert
            Assert.Equal("IExceptionFilter", ExceptionCatchBlocks.IExceptionFilter);
        }
    }
}
