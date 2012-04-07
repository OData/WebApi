// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using Xunit;

namespace System.Web.Mvc.Async.Test
{
    public class AsyncUtilTest
    {
        [Fact]
        public void WrapCallbackForSynchronizedExecution_CallsSyncIfOperationCompletedAsynchronously()
        {
            // Arrange
            MockAsyncResult asyncResult = new MockAsyncResult()
            {
                CompletedSynchronously = false,
                IsCompleted = true
            };

            bool originalCallbackCalled = false;
            AsyncCallback originalCallback = ar =>
            {
                Assert.Equal(asyncResult, ar);
                originalCallbackCalled = true;
            };

            DummySynchronizationContext syncContext = new DummySynchronizationContext();

            // Act
            AsyncCallback retVal = AsyncUtil.WrapCallbackForSynchronizedExecution(originalCallback, syncContext);
            retVal(asyncResult);

            // Assert
            Assert.True(originalCallbackCalled);
            Assert.True(syncContext.SendCalled);
        }

        [Fact]
        public void WrapCallbackForSynchronizedExecution_DoesNotCallSyncIfOperationCompletedSynchronously()
        {
            // Arrange
            MockAsyncResult asyncResult = new MockAsyncResult()
            {
                CompletedSynchronously = true,
                IsCompleted = true
            };

            bool originalCallbackCalled = false;
            AsyncCallback originalCallback = ar =>
            {
                Assert.Equal(asyncResult, ar);
                originalCallbackCalled = true;
            };

            DummySynchronizationContext syncContext = new DummySynchronizationContext();

            // Act
            AsyncCallback retVal = AsyncUtil.WrapCallbackForSynchronizedExecution(originalCallback, syncContext);
            retVal(asyncResult);

            // Assert
            Assert.True(originalCallbackCalled);
            Assert.False(syncContext.SendCalled);
        }

        [Fact]
        public void WrapCallbackForSynchronizedExecution_ReturnsNullIfCallbackIsNull()
        {
            // Act
            AsyncCallback retVal = AsyncUtil.WrapCallbackForSynchronizedExecution(null, new SynchronizationContext());

            // Assert
            Assert.Null(retVal);
        }

        [Fact]
        public void WrapCallbackForSynchronizedExecution_ReturnsOriginalCallbackIfSyncContextIsNull()
        {
            // Arrange
            AsyncCallback originalCallback = _ => { };

            // Act
            AsyncCallback retVal = AsyncUtil.WrapCallbackForSynchronizedExecution(originalCallback, null);

            // Assert
            Assert.Same(originalCallback, retVal);
        }

        private class DummySynchronizationContext : SynchronizationContext
        {
            public bool SendCalled { get; private set; }

            public override void Send(SendOrPostCallback d, object state)
            {
                SendCalled = true;
                base.Send(d, state);
            }
        }
    }
}
