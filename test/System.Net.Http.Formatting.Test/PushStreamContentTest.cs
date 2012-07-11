// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class PushStreamContentTest
    {
        [Fact]
        public void Constructor_ThrowsOnNullAction()
        {
            Assert.ThrowsArgumentNull(() => new PushStreamContent(null), "onStreamAvailable");
        }

        [Fact]
        public void Constructor_SetsDefaultMediaType()
        {
            MockStreamAction streamAction = new MockStreamAction();
            PushStreamContent content = new PushStreamContent(streamAction.Action);
            Assert.Equal(MediaTypeConstants.ApplicationOctetStreamMediaType, content.Headers.ContentType);
        }

        [Fact]
        public void Constructor_SetsMediaTypeFromString()
        {
            MockStreamAction streamAction = new MockStreamAction();
            PushStreamContent content = new PushStreamContent(streamAction.Action, "text/xml");
            Assert.Equal(MediaTypeConstants.TextXmlMediaType, content.Headers.ContentType);
        }

        [Fact]
        public void Constructor_SetsMediaType()
        {
            MockStreamAction streamAction = new MockStreamAction();
            PushStreamContent content = new PushStreamContent(streamAction.Action, MediaTypeConstants.TextXmlMediaType);
            Assert.Equal(MediaTypeConstants.TextXmlMediaType, content.Headers.ContentType);
        }

        [Fact]
        public Task SerializeToStreamAsync_CallsAction()
        {
            // Arrange
            MemoryStream outputStream = new MemoryStream();
            MockStreamAction streamAction = new MockStreamAction(close: true);
            PushStreamContent content = new PushStreamContent(streamAction.Action);

            // Act
            return content.CopyToAsync(outputStream).ContinueWith(
                copyTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, copyTask.Status);
                    Assert.True(streamAction.WasInvoked);
                    Assert.Same(content, streamAction.Content);
                    Assert.IsType<PushStreamContent.CompleteTaskOnCloseStream>(streamAction.OutputStream);
                    Assert.True(outputStream.CanRead);
                });
        }

        [Fact]
        public Task SerializeToStreamAsync_CompletesTaskOnActionException()
        {
            // Arrange
            MemoryStream outputStream = new MemoryStream();
            MockStreamAction streamAction = new MockStreamAction(throwException: true);
            PushStreamContent content = new PushStreamContent(streamAction.Action);

            // Act
            return content.CopyToAsync(outputStream).ContinueWith(
                copyTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.Faulted, copyTask.Status);
                    Assert.IsType<ApplicationException>(copyTask.Exception.GetBaseException());
                    Assert.True(streamAction.WasInvoked);
                    Assert.IsType<PushStreamContent.CompleteTaskOnCloseStream>(streamAction.OutputStream);
                    Assert.True(outputStream.CanRead);
                });
        }

        [Fact]
        public void CompleteTaskOnCloseStream_Dispose_CompletesTaskButDoNotCloseInnerStream()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            TaskCompletionSource<bool> serializeToStreamTask = new TaskCompletionSource<bool>();
            MockCompleteTaskOnCloseStream mockStream = new MockCompleteTaskOnCloseStream(mockInnerStream.Object, serializeToStreamTask);

            // Act
            mockStream.Dispose();

            // Assert 
            mockInnerStream.Protected().Verify("Dispose", Times.Never(), true);
            mockInnerStream.Verify(s => s.Close(), Times.Never());
            Assert.Equal(TaskStatus.RanToCompletion, serializeToStreamTask.Task.Status);
            Assert.True(serializeToStreamTask.Task.Result);
        }

        [Fact]
        public void NonClosingDelegatingStream_Close_CompletesTaskButDoNotCloseInnerStream()
        {
            // Arrange
            Mock<Stream> mockInnerStream = new Mock<Stream>() { CallBase = true };
            TaskCompletionSource<bool> serializeToStreamTask = new TaskCompletionSource<bool>();
            MockCompleteTaskOnCloseStream mockStream = new MockCompleteTaskOnCloseStream(mockInnerStream.Object, serializeToStreamTask);

            // Act
            mockStream.Close();

            // Assert 
            mockInnerStream.Protected().Verify("Dispose", Times.Never(), true);
            mockInnerStream.Verify(s => s.Close(), Times.Never());
            Assert.Equal(TaskStatus.RanToCompletion, serializeToStreamTask.Task.Status);
            Assert.True(serializeToStreamTask.Task.Result);
        }

        private class MockStreamAction
        {
            bool _close;
            bool _throwException;

            public MockStreamAction(bool close = false, bool throwException = false)
            {
                _close = close;
                _throwException = throwException;
            }

            public bool WasInvoked { get; private set; }

            public Stream OutputStream { get; private set; }

            public HttpContent Content { get; private set; }

            public TransportContext TransportContext { get; private set; }

            public void Action(Stream stream, HttpContent content, TransportContext context)
            {
                WasInvoked = true;
                OutputStream = stream;
                Content = content;
                TransportContext = context;

                if (_close)
                {
                    stream.Close();
                }

                if (_throwException)
                {
                    throw new ApplicationException("Action threw exception!");
                }
            }
        }

        internal class MockCompleteTaskOnCloseStream : PushStreamContent.CompleteTaskOnCloseStream
        {
            public MockCompleteTaskOnCloseStream(Stream innerStream, TaskCompletionSource<bool> serializeToStreamTask)
                : base(innerStream, serializeToStreamTask)
            {
            }
        }
    }
}
