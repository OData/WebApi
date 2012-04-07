// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpActionInvokerTracerTest
    {
        private HttpActionContext _actionContext;
        private ApiController _apiController;

        public HttpActionInvokerTracerTest()
        {
            UsersController controller = new UsersController();
            _apiController = controller;

            Func<HttpResponseMessage> actionMethod = controller.Get;
            _actionContext = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(instance: _apiController),
                new ReflectedHttpActionDescriptor { MethodInfo = actionMethod.Method });
            HttpRequestMessage request = new HttpRequestMessage();
            _actionContext.ControllerContext.Request = request;
        }

        [Fact]
        public void InvokeActionAsync_Calls_ActionDescriptor_ExecuteAsync()
        {
            // Arrange
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("mockAction");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            mockActionDescriptor.Setup(a => a.ReturnType).Returns(typeof(void));
            mockActionDescriptor.Setup(a => a.ResultConverter).Returns(new VoidResultConverter());
            bool executeWasCalled = false;
            mockActionDescriptor.Setup(a => a.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(() => TaskHelpers.FromResult<object>(null))
                .Callback(() => { executeWasCalled = true; });

            HttpActionContext context = ContextUtil.CreateActionContext(
                ContextUtil.CreateControllerContext(instance: _apiController),
                mockActionDescriptor.Object);

            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), new TestTraceWriter());

            // Act
            ((IHttpActionInvoker)tracer).InvokeActionAsync(context, CancellationToken.None).Wait();

            // Assert
            Assert.True(executeWasCalled);
        }

        [Fact]
        public void InvokeActionAsync_Traces_Begin_And_End_Info()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            Task task = ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None);
            task.Wait();

            // Assert
            Assert.Equal(2, traceWriter.Traces.Count);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void InvokeActionAsync_Returns_Cancelled_Inner_Task()
        {
            // Arrange
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), new TestTraceWriter());

            // Act
            var response = ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, cancellationSource.Token);

            // Assert
            Assert.Throws<TaskCanceledException>(() => { response.Wait(); });
            Assert.Equal<TaskStatus>(TaskStatus.Canceled, response.Status);
        }

        [Fact]
        public void InvokeActionAsync_Traces_Cancelled_Inner_Task()
        {
            // Arrange
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), traceWriter);
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Warn) { Kind = TraceKind.End }
            };

            // Act
            var response = ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, cancellationSource.Token);

            // Assert
            Assert.Throws<TaskCanceledException>(() => { response.Wait(); });
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void InvokeActionAsync_Returns_Faulted_Inner_Task()
        {
            // Arrange
            Mock<ApiControllerActionInvoker> mockActionInvoker = new Mock<ApiControllerActionInvoker>() { CallBase = true };
            InvalidOperationException expectedException = new InvalidOperationException("test message");
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>(null);
            tcs.TrySetException(expectedException);
            mockActionInvoker.Setup(
                a => a.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Returns(tcs.Task);
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(mockActionInvoker.Object, new TestTraceWriter());

            // Act
            var response = ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None);

            // Assert
            Assert.Throws<InvalidOperationException>(() => response.Wait());
            Assert.Equal<TaskStatus>(TaskStatus.Faulted, response.Status);
            Assert.Equal(expectedException.Message, response.Exception.GetBaseException().Message);
        }

        [Fact]
        public void InvokeActionAsync_Traces_Faulted_Inner_Task()
        {
            // Arrange
            Mock<ApiControllerActionInvoker> mockActionInvoker = new Mock<ApiControllerActionInvoker>() { CallBase = true };
            InvalidOperationException expectedException = new InvalidOperationException("test message");
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>(null);
            tcs.TrySetException(expectedException);
            mockActionInvoker.Setup(
                a => a.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Returns(tcs.Task);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(mockActionInvoker.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act
            var response = ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None);

            // Assert
            Assert.Throws<InvalidOperationException>(() => response.Wait());
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal(expectedException, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void InvokeActionAsync_Throws_When_ActionContext_Is_Null()
        {
            // Arrange
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(new ApiControllerActionInvoker(), new TestTraceWriter());

            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => ((IHttpActionInvoker)tracer).InvokeActionAsync(null, CancellationToken.None),
                "actionContext");
        }

        [Fact]
        public void InvokeActionAsync_Throws_Exception_Thrown_From_Inner()
        {
            // Arrange
            InvalidOperationException expectedException = new InvalidOperationException("test message");
            Mock<ApiControllerActionInvoker> mockActionInvoker = new Mock<ApiControllerActionInvoker>() {CallBase = true};
            mockActionInvoker.Setup(
                a => a.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Throws(expectedException);
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(mockActionInvoker.Object, new TestTraceWriter());

            // Act & Assert
            InvalidOperationException thrownException = Assert.Throws<InvalidOperationException>(
                () => ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None)
            );

            // Assert
            Assert.Equal(expectedException, thrownException);
        }

        [Fact]
        public void InvokeActionAsync_Traces_Exception_Thrown_From_Inner()
        {
            // Arrange
            InvalidOperationException expectedException = new InvalidOperationException("test message");
            Mock<ApiControllerActionInvoker> mockActionInvoker = new Mock<ApiControllerActionInvoker>() { CallBase = true };
            mockActionInvoker.Setup(
                a => a.InvokeActionAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>())).Throws(expectedException);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionInvokerTracer tracer = new HttpActionInvokerTracer(mockActionInvoker.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(_actionContext.Request, TraceCategories.ActionCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => ((IHttpActionInvoker)tracer).InvokeActionAsync(_actionContext, CancellationToken.None)
            );

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Equal(expectedException, traceWriter.Traces[1].Exception);
        }
    }
}
