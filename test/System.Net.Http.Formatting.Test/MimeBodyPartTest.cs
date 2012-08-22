// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
{
    public class MimeBodyPartTest
    {
        public static TheoryDataSet<MultipartStreamProvider> MultipartStreamProviders
        {
            get
            {
                return new TheoryDataSet<MultipartStreamProvider>
                {
                    new HttpContentMultipartExtensionsTests.NullProvider(),
                    new HttpContentMultipartExtensionsTests.BadStreamProvider(),
                    new HttpContentMultipartExtensionsTests.ReadOnlyStreamProvider(),
                };
            }
        }

        [Theory]
        [TestDataSet(typeof(MimeBodyPartTest), "MultipartStreamProviders")]
        public void GetOutputStream_ThrowsOnInvalidStreamProvider(MultipartStreamProvider streamProvider)
        {
            HttpContent parent = new StringContent("hello");
            MimeBodyPart bodypart = new MimeBodyPart(streamProvider, 1024);
            Assert.Throws<InvalidOperationException>(() => bodypart.GetOutputStream(parent));
        }

        [Fact]
        public void Dispose_ClosesOutputStreamOnNonMemoryStream()
        {
            // Arrange
            HttpContent parent = new StringContent("hello");
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.CanWrite).Returns(true);
            Mock<MultipartStreamProvider> mockStreamProvider = new Mock<MultipartStreamProvider>();
            mockStreamProvider.Setup(sp => sp.GetStream(It.IsAny<HttpContent>(), It.IsAny<HttpContentHeaders>())).Returns(mockStream.Object);
            MimeBodyPart bodypart = new MimeBodyPart(mockStreamProvider.Object, 1024);

            // Act
            bodypart.GetOutputStream(parent);
            bodypart.Dispose();

            // Assert
            mockStream.Verify(s => s.Close(), Times.Once());
        }

        [Fact]
        public void Dispose_SetsPositionToZeroOnMemoryStream()
        {
            // Arrange
            HttpContent parent = new StringContent("hello");
            Mock<MemoryStream> mockStream = new Mock<MemoryStream> { CallBase = true };
            Mock<MultipartStreamProvider> mockStreamProvider = new Mock<MultipartStreamProvider>();
            mockStreamProvider.Setup(sp => sp.GetStream(It.IsAny<HttpContent>(), It.IsAny<HttpContentHeaders>())).Returns(mockStream.Object);
            MimeBodyPart bodypart = new MimeBodyPart(mockStreamProvider.Object, 1024);

            // Act
            bodypart.GetOutputStream(parent);
            bodypart.Dispose();

            // Assert
            mockStream.VerifySet(s => s.Position = 0);
        }
    }
}
