// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Filters
{
    public class ActionFilterAttributeTest
    {
        [Fact]
        public void AllowsMultiple_DefaultReturnsTrue()
        {
            ActionFilterAttribute actionFilter = new TestableActionFilter();

            Assert.True(actionFilter.AllowMultiple);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfContextParameterIsNull_ThrowsException()
        {
            var filter = new TestableActionFilter() as IActionFilter;
            Assert.ThrowsArgumentNull(() =>
            {
                filter.ExecuteActionFilterAsync(actionContext: null, cancellationToken: CancellationToken.None, continuation: () => null);
            }, "actionContext");
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfContinuationParameterIsNull_ThrowsException()
        {
            var filter = new TestableActionFilter() as IActionFilter;
            Assert.ThrowsArgumentNull(() =>
            {
                filter.ExecuteActionFilterAsync(actionContext: ContextUtil.CreateActionContext(), cancellationToken: CancellationToken.None, continuation: null);
            }, "continuation");
        }

        [Fact]
        public void ExecuteActionFilterAsync_InvokesOnActionExecutingBeforeContinuation()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>() { CallBase = true };
            bool onActionExecutingInvoked = false;
            filterMock.Setup(f => f.OnActionExecuting(It.IsAny<HttpActionContext>())).Callback(() =>
            {
                onActionExecutingInvoked = true;
            });
            bool? flagWhenContinuationInvoked = null;
            Func<Task<HttpResponseMessage>> continuation = () =>
            {
                flagWhenContinuationInvoked = onActionExecutingInvoked;
                return TaskHelpers.FromResult(new HttpResponseMessage());
            };
            var filter = (IActionFilter)filterMock.Object;

            // Act
            filter.ExecuteActionFilterAsync(context, CancellationToken.None, continuation).Wait();
            // Assert
            Assert.True(flagWhenContinuationInvoked.Value);
        }

        [Fact]
        public void ExecuteActionFilterAsync_OnActionExecutingMethodGetsPassedControllerContext()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>() { CallBase = false };
            var filter = (IActionFilter)filterMock.Object;

            // Act
            filter.ExecuteActionFilterAsync(context, CancellationToken.None, () =>
            {
                return TaskHelpers.FromResult(new HttpResponseMessage());
            }).Wait();

            // Assert
            filterMock.Verify(f => f.OnActionExecuting(context));
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfOnActionExecutingThrowsException_ReturnsFaultedTask()
        {
            // Arrange
            Exception e = new Exception("{51C81EE9-F8D2-4F63-A1F8-B56052E0F2A4}");
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            filterMock.Setup(f => f.OnActionExecuting(It.IsAny<HttpActionContext>())).Throws(e);
            var filter = (IActionFilter)filterMock.Object;
            bool continuationCalled = false;

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () =>
            {
                continuationCalled = true;
                return null;
            });

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsFaulted);
            Assert.Same(e, result.Exception.InnerException);
            Assert.False(continuationCalled);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfOnActionExecutingSetsResult_ShortCircuits()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            HttpResponseMessage response = new HttpResponseMessage();
            filterMock.Setup(f => f.OnActionExecuting(It.IsAny<HttpActionContext>())).Callback<HttpActionContext>(c =>
            {
                c.Response = response;
            });
            bool continuationCalled = false;
            var filter = (IActionFilter)filterMock.Object;

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () =>
            {
                continuationCalled = true;
                return null;
            }).Result;

            // Assert
            Assert.False(continuationCalled);
            Assert.Same(response, result);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfContinuationTaskWasCanceled_ReturnsCanceledTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () => TaskHelpers.Canceled<HttpResponseMessage>());

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsCanceled);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfContinuationSucceeded_InvokesOnActionExecutedAsSuccess()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;
            HttpResponseMessage response = new HttpResponseMessage();

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromResult(response));

            // Assert
            result.WaitUntilCompleted();
            filterMock.Verify(f => f.OnActionExecuted(It.Is<HttpActionExecutedContext>(ec =>
                    Object.ReferenceEquals(ec.Response, response)
                    && ec.Exception == null
                    && Object.ReferenceEquals(ec.ActionContext, context)
            )));
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfContinuationFaulted_InvokesOnActionExecutedAsError()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;
            Exception exception = new Exception("{ABCC912C-B6D1-4C27-9059-732ABC644A0C}");
            Func<Task<HttpResponseMessage>> continuation = () => TaskHelpers.FromError<HttpResponseMessage>(new AggregateException(exception));

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, continuation);

            // Assert
            result.WaitUntilCompleted();
            filterMock.Verify(f => f.OnActionExecuted(It.Is<HttpActionExecutedContext>(ec =>
                    Object.ReferenceEquals(ec.Exception, exception)
                    && ec.Response == null
                    && Object.ReferenceEquals(ec.ActionContext, context)
            )));
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfOnActionExecutedDoesNotHandleExceptionFromContinuation_ReturnsFaultedTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;
            Exception exception = new Exception("{1EC330A2-33D0-4892-9335-2D833849D54E}");
            filterMock.Setup(f => f.OnActionExecuted(It.IsAny<HttpActionExecutedContext>())).Callback<HttpActionExecutedContext>(ec =>
            {
                ec.Response = null;
            });

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromError<HttpResponseMessage>(exception));

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsFaulted);
            Assert.Same(exception, result.Exception.InnerException);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfOnActionExecutedDoesHandleExceptionFromContinuation_ReturnsSuccessfulTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;
            HttpResponseMessage newResponse = new HttpResponseMessage();
            filterMock.Setup(f => f.OnActionExecuted(It.IsAny<HttpActionExecutedContext>())).Callback<HttpActionExecutedContext>(ec =>
            {
                ec.Response = newResponse;
            });

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromError<HttpResponseMessage>(new Exception("{ED525C8E-7165-4207-B3F6-4AB095739017}")));

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsCompleted);
            Assert.Same(newResponse, result.Result);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfOnActionExecutedThrowsException_ReturnsFaultedTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;
            Exception exception = new Exception("{AC32AD02-36A7-45E5-8955-76A4E3B461C6}");
            filterMock.Setup(f => f.OnActionExecuted(It.IsAny<HttpActionExecutedContext>())).Callback<HttpActionExecutedContext>(ec =>
            {
                throw exception;
            });

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromResult(new HttpResponseMessage()));

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsFaulted);
            Assert.Same(exception, result.Exception.InnerException);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfOnActionExecutedSetsResult_ReturnsNewResult()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;
            HttpResponseMessage newResponse = new HttpResponseMessage();
            filterMock.Setup(f => f.OnActionExecuted(It.IsAny<HttpActionExecutedContext>())).Callback<HttpActionExecutedContext>(ec =>
            {
                ec.Response = newResponse;
            });

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromResult(new HttpResponseMessage()));

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsCompleted);
            Assert.Same(newResponse, result.Result);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfOnActionExecutedDoesNotChangeResult_ReturnsSameResult()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;
            HttpResponseMessage response = new HttpResponseMessage();
            filterMock.Setup(f => f.OnActionExecuted(It.IsAny<HttpActionExecutedContext>())).Callback<HttpActionExecutedContext>(ec =>
            {
                ec.Response = ec.Response;
            });

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromResult(response));

            // Assert
            result.WaitUntilCompleted();
            Assert.True(result.IsCompleted);
            Assert.Same(response, result.Result);
        }

        [Fact]
        public void ExecuteActionFilterAsync_IfOnActionExecutedRemovesSuccessfulResult_ReturnsFaultedTask()
        {
            // Arrange
            HttpActionContext context = ContextUtil.CreateActionContext();
            Mock<ActionFilterAttribute> filterMock = new Mock<ActionFilterAttribute>();
            var filter = (IActionFilter)filterMock.Object;
            HttpResponseMessage response = new HttpResponseMessage();
            filterMock.Setup(f => f.OnActionExecuted(It.IsAny<HttpActionExecutedContext>())).Callback<HttpActionExecutedContext>(ec =>
            {
                ec.Response = null;
            });

            // Act
            var result = filter.ExecuteActionFilterAsync(context, CancellationToken.None, () => TaskHelpers.FromResult(response));

            // Assert
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.IsException<InvalidOperationException>(
                exception: result.Exception,
                expectedMessage: "After calling ActionFilterAttributeProxy.OnActionExecuted, the HttpActionExecutedContext properties Result and Exception were both null. At least one of these values must be non-null. To provide a new response, please set the Result object; to indicate an error, please throw an exception."
            );
        }

        public class TestableActionFilter : ActionFilterAttribute
        {
        }
    }
}
