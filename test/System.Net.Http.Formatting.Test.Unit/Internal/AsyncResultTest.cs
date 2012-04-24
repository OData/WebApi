// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Internal;
using System.Threading;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class AsyncResultTest
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromMilliseconds(10);

        [Fact]
        public void Constructor_Initializes()
        {
            // Arrange
            AsyncCallback callback = new AsyncCallback(_ => { });
            object state = new object();

            // Act
            MockAsyncResult mockAsyncResult = new MockAsyncResult(callback, state);

            // Assert
            Assert.True(mockAsyncResult.HasCallback);
            Assert.False(mockAsyncResult.IsCompleted);
            Assert.False(mockAsyncResult.CompletedSynchronously);
            Assert.Same(state, mockAsyncResult.AsyncState);
        }

        [Fact]
        public void AsyncWaitHandle_ReturnsSameHandle()
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            WaitHandle handle1 = mockAsyncResult.AsyncWaitHandle;
            WaitHandle handle2 = mockAsyncResult.AsyncWaitHandle;

            // Assert
            Assert.Same(handle1, handle2);
        }

        [Fact]
        public void AsyncWaitHandle_ReturnsIncompleteHandle()
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            WaitHandle handle = mockAsyncResult.AsyncWaitHandle;
            bool signal = handle.WaitOne(_timeout);

            // Assert
            Assert.False(signal);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AsyncWaitHandle_ThrowsIfAcessedAfterEndIsCalled(bool completedSynchronously)
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);
            mockAsyncResult.Complete(completedSynchronously);
            MockAsyncResult.End<MockAsyncResult>(mockAsyncResult);

            // Act/Assert
            Assert.Throws<InvalidOperationException>(() => mockAsyncResult.AsyncWaitHandle);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Complete_SetsCompletedSynchronously(bool completedSynchronously)
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            mockAsyncResult.Complete(completedSynchronously);

            // Assert
            Assert.Equal(completedSynchronously, mockAsyncResult.CompletedSynchronously);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Complete_SetsIsCompleted(bool completedSynchronously)
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            mockAsyncResult.Complete(completedSynchronously);

            // Assert
            Assert.True(mockAsyncResult.IsCompleted);
        }

        [Fact]
        public void Complete_SetsHandleWhenNotCompletingSynchronously()
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            mockAsyncResult.Complete(false);
            WaitHandle handle = mockAsyncResult.AsyncWaitHandle;
            bool signal = handle.WaitOne(_timeout);

            // Assert
            Assert.True(signal);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Complete_CallsCallback(bool completedSynchronously)
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(false);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);

            // Act
            mockAsyncResult.Complete(completedSynchronously);

            // Assert
            Assert.True(mockCallback.WasInvoked);
            Assert.Same(mockAsyncResult, mockCallback.AsyncResult);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Complete_ThrowsOnMultipleCompletes(bool completedSynchronously)
        {
            // Arrange
            MockAsyncResult mockAsyncResult = new MockAsyncResult(null, null);

            // Act
            mockAsyncResult.Complete(completedSynchronously);

            // Assert
            Assert.Throws<InvalidOperationException>(() => mockAsyncResult.Complete(completedSynchronously));
        }

        [Fact]
        public void Complete_ThrowsIfCallbackThrows()
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(true);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);

            // Act/Assert
            Assert.Throws<InvalidOperationException>(() => mockAsyncResult.Complete(false));
        }

        [Fact]
        public void End_ThrowsOnNullAsyncResult()
        {
            Assert.ThrowsArgumentNull(() => MockAsyncResult.End<MockAsyncResult>(null), "result");
        }

        [Fact]
        public void End_ThrowsOnInvalidAsyncResult()
        {
            Mock<IAsyncResult> mockIAsyncResult = new Mock<IAsyncResult>();
            Assert.ThrowsArgument(() => MockAsyncResult.End<MockAsyncResult>(mockIAsyncResult.Object), "result");
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void End_ThrowsIfCalledTwiceOnSameAsyncResult(bool completedSynchronously)
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(false);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);
            mockAsyncResult.Complete(completedSynchronously);

            // Act
            MockAsyncResult.End<MockAsyncResult>(mockAsyncResult);

            // Act
            Assert.Throws<InvalidOperationException>(() => MockAsyncResult.End<MockAsyncResult>(mockAsyncResult));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void End_ThrowsIfCompletedWithException(bool completedSynchronously)
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(false);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);
            ApplicationException applicationException = new ApplicationException("Complete failed!");
            mockAsyncResult.Complete(completedSynchronously, applicationException);

            // Act/Assert
            Assert.Throws<ApplicationException>(() => MockAsyncResult.End<MockAsyncResult>(mockAsyncResult));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void End_ClosesHandle(bool completedSynchronously)
        {
            // Arrange
            MockAsyncCallback mockCallback = new MockAsyncCallback(false);
            MockAsyncResult mockAsyncResult = new MockAsyncResult(mockCallback.Callback, null);
            ApplicationException applicationException = new ApplicationException("Complete failed!");
            mockAsyncResult.Complete(completedSynchronously);
            WaitHandle handle = mockAsyncResult.AsyncWaitHandle;

            // Act
            MockAsyncResult.End<MockAsyncResult>(mockAsyncResult);

            // Assert
            Assert.ThrowsObjectDisposed(() => handle.WaitOne(), "");
        }

        internal class MockAsyncResult : AsyncResult
        {
            public MockAsyncResult(AsyncCallback callback, object state)
                : base(callback, state)
            {
            }

            public new void Complete(bool completedSynchronously)
            {
                base.Complete(completedSynchronously);
            }

            public new void Complete(bool completedSynchronously, Exception e)
            {
                base.Complete(completedSynchronously, e);
            }

            public static new TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : AsyncResult
            {
                return AsyncResult.End<TAsyncResult>(result);
            }
        }

        public class MockAsyncCallback
        {
            private bool _throwInCallback;

            public MockAsyncCallback(bool throwInCallback)
            {
                _throwInCallback = throwInCallback;
            }

            public bool WasInvoked { get; private set; }

            public IAsyncResult AsyncResult { get; private set; }

            public Exception CallbackException { get; private set; }

            public void Callback(IAsyncResult result)
            {
                WasInvoked = true;
                AsyncResult = result;
                if (_throwInCallback)
                {
                    CallbackException = new Exception("Callback exception");
                    throw CallbackException;
                }
            }
        }
    }
}
