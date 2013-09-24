// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class ExceptionFilterResultTests
    {
        [Fact]
        public void ExecuteAsync_IfInnerResultTaskIsSuccessful_ReturnsSuccessTask()
        {
            // Arrange
            List<string> log = new List<string>();
            var response = new HttpResponseMessage();
            var actionResult = CreateStubActionResult(TaskHelpers.FromResult(response));
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { exceptionFilterMock.Object };
            IHttpActionResult product = CreateProductUnderTest(actionContext, filters, actionResult);

            // Act
            var result = product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
            Assert.Same(response, result.Result);
            Assert.Equal(new string[] { }, log.ToArray());
        }

        [Fact]
        public void ExecuteAsync_IfInnerResultTaskIsCanceled_ReturnsCanceledTask()
        {
            // Arrange
            List<string> log = new List<string>();
            var actionResult = CreateStubActionResult(TaskHelpers.Canceled<HttpResponseMessage>());
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { exceptionFilterMock.Object };
            IHttpActionResult product = CreateProductUnderTest(actionContext, filters, actionResult);

            // Act
            var result = product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Canceled, result.Status);
            Assert.Equal(new string[] { "exceptionFilter" }, log.ToArray());
        }

        [Fact]
        public void ExecuteAsync_IfInnerResultTaskIsFaulted_ExecutesFiltersAndReturnsFaultedTaskIfNotHandled()
        {
            // Arrange
            List<string> log = new List<string>();
            var exception = new Exception();
            var actionResult = CreateStubActionResult(TaskHelpers.FromError<HttpResponseMessage>(exception));
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            Exception exceptionSeenByFilter = null;
            var exceptionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                exceptionSeenByFilter = ec.Exception;
                log.Add("exceptionFilter");
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { exceptionFilterMock.Object };
            IHttpActionResult product = CreateProductUnderTest(actionContext, filters, actionResult);

            // Act
            var result = product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.Same(exception, result.Exception.InnerException);
            Assert.Same(exception, exceptionSeenByFilter);
            Assert.Equal(new string[] { "exceptionFilter" }, log.ToArray());
        }

        [Fact]
        public void ExecuteAsync_IfInnerResultTaskIsFaulted_ExecutesFiltersAndReturnsResultIfHandled()
        {
            // Arrange
            List<string> log = new List<string>();
            var exception = new Exception();
            var actionResult = CreateStubActionResult(TaskHelpers.FromError<HttpResponseMessage>(exception));
            HttpActionContext actionContext = ContextUtil.CreateActionContext();
            HttpResponseMessage globalFilterResponse = new HttpResponseMessage();
            HttpResponseMessage actionFilterResponse = new HttpResponseMessage();
            HttpResponseMessage resultSeenByGlobalFilter = null;
            var globalFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("globalFilter");
                resultSeenByGlobalFilter = ec.Response;
                ec.Response = globalFilterResponse;
                return Task.Factory.StartNew(() => { });
            });
            var actionFilterMock = CreateExceptionFilterMock((ec, ct) =>
            {
                log.Add("actionFilter");
                ec.Response = actionFilterResponse;
                return Task.Factory.StartNew(() => { });
            });
            var filters = new IExceptionFilter[] { globalFilterMock.Object, actionFilterMock.Object };
            IHttpActionResult product = CreateProductUnderTest(actionContext, filters, actionResult);

            // Act
            var result = product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            result.WaitUntilCompleted();
            Assert.Equal(TaskStatus.RanToCompletion, result.Status);
            Assert.Same(globalFilterResponse, result.Result);
            Assert.Same(actionFilterResponse, resultSeenByGlobalFilter);
            Assert.Equal(new string[] { "actionFilter", "globalFilter" }, log.ToArray());
        }

        [Fact]
        public void ExecuteAsync_WhenFilterChangesException_ThrowsUpdatedException()
        {
            // Arrange
            Exception expectedException = new NotImplementedException();

            HttpActionContext context = CreateActionContext();

            Mock<IExceptionFilter> filterMock = new Mock<IExceptionFilter>();
            filterMock
                .Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(),
                    It.IsAny<CancellationToken>()))
                .Callback<HttpActionExecutedContext, CancellationToken>((c, t) =>
                {
                    c.Exception = expectedException;
                })
                .Returns(() => Task.FromResult<object>(null));
            IExceptionFilter filter = filterMock.Object;
            IExceptionFilter[] filters = new IExceptionFilter[] { filter };

            TaskCompletionSource<HttpResponseMessage> taskSource = new TaskCompletionSource<HttpResponseMessage>();
            taskSource.SetException(new InvalidOperationException());
            IHttpActionResult innerResult = CreateStubActionResult(taskSource.Task);

            IHttpActionResult product = CreateProductUnderTest(context, filters, innerResult);

            // Act
            Task<HttpResponseMessage> task = product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(task);
            task.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, task.Status);
            Assert.NotNull(task.Exception);
            Exception exception = task.Exception.GetBaseException();
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void ExecuteAsync_PreservesExceptionStackTrace()
        {
            // Arrange
            Exception originalException = CreateExceptionWithStackTrace();
            string expectedStackTrace = originalException.StackTrace;

            HttpActionContext context = CreateActionContext();
            IExceptionFilter[] filters = new IExceptionFilter[0];

            TaskCompletionSource<HttpResponseMessage> taskSource = new TaskCompletionSource<HttpResponseMessage>();
            taskSource.SetException(originalException);
            IHttpActionResult innerResult = CreateStubActionResult(taskSource.Task);

            IHttpActionResult product = CreateProductUnderTest(context, filters, innerResult);

            // Act
            Task<HttpResponseMessage> task = product.ExecuteAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(task);
            task.WaitUntilCompleted();
            Assert.Equal(TaskStatus.Faulted, task.Status);
            Assert.NotNull(task.Exception);
            Exception exception = task.Exception.GetBaseException();
            Assert.NotNull(expectedStackTrace);
            Assert.NotNull(exception);
            Assert.NotNull(exception.StackTrace);
            Assert.True(exception.StackTrace.StartsWith(expectedStackTrace));
        }

        private HttpActionContext CreateActionContext()
        {
            return new HttpActionContext();
        }

        private Mock<IExceptionFilter> CreateExceptionFilterMock(
            Func<HttpActionExecutedContext, CancellationToken, Task> implementation)
        {
            Mock<IExceptionFilter> filterMock = new Mock<IExceptionFilter>();
            filterMock.Setup(f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(),
                                                                CancellationToken.None))
                      .Returns(implementation)
                      .Verifiable();
            return filterMock;
        }

        private static Exception CreateExceptionWithStackTrace()
        {
            Exception exception;

            try
            {
                throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return exception;
        }

        private ExceptionFilterResult CreateProductUnderTest(HttpActionContext context, IExceptionFilter[] filters,
            IHttpActionResult innerResult)
        {
            return new ExceptionFilterResult(context, filters, innerResult);
        }

        private IHttpActionResult CreateStubActionResult(Task<HttpResponseMessage> task)
        {
            Mock<IHttpActionResult> actionResultMock = new Mock<IHttpActionResult>(MockBehavior.Strict);
            actionResultMock.Setup(r => r.ExecuteAsync(It.IsAny<CancellationToken>())).Returns(() =>
            {
                return task;
            });
            return actionResultMock.Object;
        }
    }
}
