// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Async.Test
{
    public class AsyncResultWrapperTest
    {
        [Fact]
        public void Begin_AsynchronousCompletion()
        {
            // Arrange
            AsyncCallback capturedCallback = null;
            IAsyncResult resultGivenToCallback = null;
            IAsyncResult innerResult = new MockAsyncResult();

            // Act
            IAsyncResult outerResult = AsyncResultWrapper.Begin(
                ar => { resultGivenToCallback = ar; },
                null,
                (callback, state) =>
                {
                    capturedCallback = callback;
                    return innerResult;
                },
                ar => { });

            capturedCallback(innerResult);

            // Assert
            Assert.Equal(outerResult, resultGivenToCallback);
        }

        [Fact]
        public void Begin_ReturnsAsyncResultWhichWrapsInnerResult()
        {
            // Arrange
            IAsyncResult innerResult = new MockAsyncResult()
            {
                AsyncState = "inner state",
                CompletedSynchronously = true,
                IsCompleted = true
            };

            // Act
            IAsyncResult outerResult = AsyncResultWrapper.Begin(
                null, "outer state",
                (callback, state) => innerResult,
                ar => { });

            // Assert
            Assert.Equal(innerResult.AsyncState, outerResult.AsyncState);
            Assert.Equal(innerResult.AsyncWaitHandle, outerResult.AsyncWaitHandle);
            Assert.Equal(innerResult.CompletedSynchronously, outerResult.CompletedSynchronously);
            Assert.Equal(innerResult.IsCompleted, outerResult.IsCompleted);
        }

        [Fact]
        public void Begin_SynchronousCompletion()
        {
            // Arrange
            IAsyncResult resultGivenToCallback = null;
            IAsyncResult innerResult = new MockAsyncResult();

            // Act
            IAsyncResult outerResult = AsyncResultWrapper.Begin(
                ar => { resultGivenToCallback = ar; },
                null,
                (callback, state) =>
                {
                    callback(innerResult);
                    return innerResult;
                },
                ar => { });

            // Assert
            Assert.Equal(outerResult, resultGivenToCallback);
        }

        [Fact]
        public void Begin_AsynchronousButAlreadyCompleted()
        {
            // Arrange
            Mock<IAsyncResult> innerResultMock = new Mock<IAsyncResult>();
            innerResultMock.Setup(ir => ir.CompletedSynchronously).Returns(false);
            innerResultMock.Setup(ir => ir.IsCompleted).Returns(true);

            // Act
            IAsyncResult outerResult = AsyncResultWrapper.Begin(
                null,
                null,
                (callback, state) =>
                {
                    callback(innerResultMock.Object);
                    return innerResultMock.Object;
                },
                ar => { });

            // Assert
            Assert.True(outerResult.CompletedSynchronously);
        }

        [Fact]
        public void BeginSynchronous_Action()
        {
            // Arrange
            bool actionCalled = false;

            // Act
            IAsyncResult asyncResult = AsyncResultWrapper.BeginSynchronous(null, null, delegate { actionCalled = true; });
            AsyncResultWrapper.End(asyncResult);

            // Assert
            Assert.True(actionCalled);
            Assert.True(asyncResult.IsCompleted);
            Assert.True(asyncResult.CompletedSynchronously);
        }

        [Fact]
        public void BeginSynchronous_Func()
        {
            // Act
            IAsyncResult asyncResult = AsyncResultWrapper.BeginSynchronous(null, null, () => 42);
            int retVal = AsyncResultWrapper.End<int>(asyncResult);

            // Assert
            Assert.Equal(42, retVal);
            Assert.True(asyncResult.IsCompleted);
            Assert.True(asyncResult.CompletedSynchronously);
        }

        [Fact]
        public void End_ExecutesStoredDelegateAndReturnsValue()
        {
            // Arrange
            IAsyncResult asyncResult = AsyncResultWrapper.Begin(
                null, null,
                (callback, state) => new MockAsyncResult(),
                ar => 42);

            // Act
            int returned = AsyncResultWrapper.End<int>(asyncResult);

            // Assert
            Assert.Equal(42, returned);
        }

        [Fact]
        public void End_ThrowsIfAsyncResultIsIncorrectType()
        {
            // Arrange
            IAsyncResult asyncResult = AsyncResultWrapper.Begin(
                null, null,
                (callback, state) => new MockAsyncResult(),
                ar => { });

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { AsyncResultWrapper.End<int>(asyncResult); },
                @"The provided IAsyncResult is not valid for this method.
Parameter name: asyncResult");
        }

        [Fact]
        public void End_ThrowsIfAsyncResultIsNull()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { AsyncResultWrapper.End(null); }, "asyncResult");
        }

        [Fact]
        public void End_ThrowsIfAsyncResultTagMismatch()
        {
            // Arrange
            IAsyncResult asyncResult = AsyncResultWrapper.Begin(
                null, null,
                (callback, state) => new MockAsyncResult(),
                ar => { },
                "some tag");

            // Act & assert
            Assert.Throws<ArgumentException>(
                delegate { AsyncResultWrapper.End(asyncResult, "some other tag"); },
                @"The provided IAsyncResult is not valid for this method.
Parameter name: asyncResult");
        }

        [Fact]
        public void End_ThrowsIfCalledTwiceOnSameAsyncResult()
        {
            // Arrange
            IAsyncResult asyncResult = AsyncResultWrapper.Begin(
                null, null,
                (callback, state) => new MockAsyncResult(),
                ar => { });

            // Act & assert
            AsyncResultWrapper.End(asyncResult);
            Assert.Throws<InvalidOperationException>(
                delegate { AsyncResultWrapper.End(asyncResult); },
                @"The provided IAsyncResult has already been consumed.");
        }

        [Fact]
        public void TimedOut()
        {
            // Arrange
            ManualResetEvent waitHandle = new ManualResetEvent(false /* initialState */);

            AsyncCallback callback = ar => { waitHandle.Set(); };

            // Act & assert
            IAsyncResult asyncResult = AsyncResultWrapper.Begin(
                callback, null,
                (innerCallback, innerState) => new MockAsyncResult(),
                ar => { Assert.True(false, "This callback should never execute since we timed out."); },
                null, 0);

            // wait for the timeout
            waitHandle.WaitOne();

            Assert.Throws<TimeoutException>(
                delegate { AsyncResultWrapper.End(asyncResult); });
        }
    }
}
