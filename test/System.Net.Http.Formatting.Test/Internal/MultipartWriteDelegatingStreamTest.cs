// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Mocks;
using System.Text;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http.Internal
{
    public class MultipartWriteDelegatingStreamTest
    {
        private static readonly byte[] _testData = Encoding.UTF8.GetBytes("Hello World!");

        [Fact]
        public void MultipartWriteDelegatingStreamTest_CallsCallbackOnSuccess()
        {
            // Arrange
            object expectedState = new object();
            IAsyncResult mockAsyncResult = MockCompletedAsyncResult.Create(true, expectedState);

            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            mockInnerStream.Setup(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Returns<byte[], int, int, AsyncCallback, object>((data, offset, count, callback, state) => mockAsyncResult);
            mockInnerStream.Setup(s => s.EndWrite(mockAsyncResult));

            MockMultipartWriteDelegatingStream mockStream = new MockMultipartWriteDelegatingStream(mockInnerStream.Object);
            MockAsyncCallback mockCallback = new MockAsyncCallback();

            // Act
            IAsyncResult result = mockStream.BeginWrite(_testData, 0, _testData.Length, mockCallback.Handler, expectedState);

            // Assert
            Assert.True(mockCallback.WasInvoked);
            Assert.Same(expectedState, mockCallback.AsyncResult.AsyncState);
        }

        [Fact]
        public void MultipartWriteDelegatingStreamTest_CallsCallbackOnFailure()
        {
            // Arrange
            object expectedState = new object();
            IAsyncResult mockAsyncResult = MockCompletedAsyncResult.Create(true, expectedState);

            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            mockInnerStream.Setup(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()))
                .Returns<byte[], int, int, AsyncCallback, object>((data, offset, count, callback, state) => mockAsyncResult);
            mockInnerStream.Setup(s => s.EndWrite(mockAsyncResult))
                .Throws(new Exception("Catch this!"));

            MockMultipartWriteDelegatingStream mockStream = new MockMultipartWriteDelegatingStream(mockInnerStream.Object);
            MockAsyncCallback mockCallback = new MockAsyncCallback();

            // Act
            IAsyncResult result = mockStream.BeginWrite(_testData, 0, _testData.Length, mockCallback.Handler, expectedState);

            // Assert
            Assert.True(mockCallback.WasInvoked);
            Assert.Same(expectedState, mockCallback.AsyncResult.AsyncState);
        }
    }
}
