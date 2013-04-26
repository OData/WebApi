// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class UrlRewriterHelperTest
    {
        [Fact]
        public void WasRequestRewritten_FalseIfUrlRewriterIsTurnedOff()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            Mock<HttpContextBase> requestMock = new Mock<HttpContextBase>();
            requestMock.Setup(c => c.Request.ServerVariables.Get(UrlRewriterHelper.UrlRewriterEnabledServerVar)).Returns((string)null).Verifiable();

            // Act
            bool result = helper.WasRequestRewritten(requestMock.Object);

            // Assert
            Assert.False(result);
            requestMock.Verify();
            requestMock.Verify(c => c.Request.ServerVariables.Get(UrlRewriterHelper.UrlWasRewrittenServerVar), Times.Never());
        }

        [Fact]
        public void WasRequestRewritten_FalseIfUrlRewriterIsTurnedOnButRequestWasNotRewritten()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            Mock<HttpContextBase> requestMock = new Mock<HttpContextBase>();
            requestMock.Setup(c => c.Request.ServerVariables.Get(UrlRewriterHelper.UrlRewriterEnabledServerVar)).Returns("yes").Verifiable();
            requestMock.Setup(c => c.Request.ServerVariables.Get(UrlRewriterHelper.UrlWasRewrittenServerVar)).Returns((string)null).Verifiable();

            // Act
            bool result = helper.WasRequestRewritten(requestMock.Object);

            // Assert
            Assert.False(result);
            requestMock.Verify();
        }

        [Fact]
        public void WasRequestRewritten_TrueIfUrlRewriterIsTurnedOnAndRequestWasRewritten()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            Mock<HttpContextBase> requestMock = new Mock<HttpContextBase>();
            requestMock.Setup(c => c.Request.ServerVariables.Get(UrlRewriterHelper.UrlRewriterEnabledServerVar)).Returns("yes").Verifiable();
            requestMock.Setup(c => c.Request.ServerVariables.Get(UrlRewriterHelper.UrlWasRewrittenServerVar)).Returns("yes").Verifiable();

            // Act
            bool result = helper.WasRequestRewritten(requestMock.Object);

            // Assert
            Assert.True(result);
            requestMock.Verify();
        }

        [Fact]
        public void WasRequestRewritten_ChecksIfUrlRewriterIsTurnedOnOnlyOnce()
        {
            // Arrange
            UrlRewriterHelper helper = new UrlRewriterHelper();
            Mock<HttpContextBase> request1Mock = new Mock<HttpContextBase>();
            request1Mock.Setup(c => c.Request.ServerVariables).Returns(new NameValueCollection());
            Mock<HttpContextBase> request2Mock = new Mock<HttpContextBase>();

            // Act
            bool result1 = helper.WasRequestRewritten(request1Mock.Object);
            bool result2 = helper.WasRequestRewritten(request2Mock.Object);

            // Assert
            request1Mock.Verify(c => c.Request.ServerVariables, Times.Once());
            request2Mock.Verify(c => c.Request.ServerVariables, Times.Never());
            Assert.False(result1);
            Assert.False(result2);
        }
    }
}
