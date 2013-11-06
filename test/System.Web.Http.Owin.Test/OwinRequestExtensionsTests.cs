// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Owin;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Owin
{
    public class OwinRequestExtensionsTests
    {
        [Fact]
        public void GetContentLength_IfHeadersIsNull_ReturnsNull()
        {
            // Arrange
            IOwinRequest request = CreateStubRequest(headers: null);
            Assert.Null(request.Headers); // Guard

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfHeadersDoesNotContainContentLengthHeader_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders();
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderValuesIsNull_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", null);
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderValuesIsEmpty_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[0]);
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderValuesIsMultiple_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { "123", "456" });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderIsSingleNullValue_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { null });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderIsSingleNonIntegerValue_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { "abc" });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfContentLengthHeaderIsNegative_ReturnsNull()
        {
            // Arrange
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { "-1" });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = OwinRequestExtensions.GetContentLength(request);

            // Assert
            Assert.False(length.HasValue);
        }

        [Fact]
        public void GetContentLength_IfValid_ReturnsValue()
        {
            int expected = 123;
            IHeaderDictionary headers = CreateStubHeaders("Content-Length", new string[] { expected.ToString() });
            IOwinRequest request = CreateStubRequest(headers);

            // Act
            int? length = request.GetContentLength();

            // Assert
            Assert.True(length.HasValue);
            Assert.Equal(expected, length.Value);
        }

        private static IHeaderDictionary CreateStubHeaders(string key, string[] value)
        {
            Mock<IHeaderDictionary> mock = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            mock.Setup(h => h.TryGetValue(key, out value)).Returns(true);
            return mock.Object;
        }

        private static IHeaderDictionary CreateStubHeaders()
        {
            Mock<IHeaderDictionary> mock = new Mock<IHeaderDictionary>(MockBehavior.Strict);
            string[] value = null;
            mock.Setup(h => h.TryGetValue(It.IsAny<string>(), out value)).Returns(false);
            return mock.Object;
        }

        private static IOwinRequest CreateStubRequest(IHeaderDictionary headers)
        {
            Mock<IOwinRequest> mock = new Mock<IOwinRequest>(MockBehavior.Strict);
            mock.SetupGet(r => r.Headers).Returns(headers);
            return mock.Object;
        }
    }
}
