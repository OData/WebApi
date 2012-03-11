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
    public class HttpControllerTracerTest
    {
        private Mock<HttpActionDescriptor> _mockActionDescriptor;
        private HttpControllerDescriptor _controllerDescriptor;

        public HttpControllerTracerTest()
        {
            _mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            _mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            _mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));

            _controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "controller", typeof(ApiController));
        }

        [Fact]
        public void ExecuteAsync_Invokes_Inner_And_Traces()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<ApiController> mockController = new Mock<ApiController>() { CallBase = true };
            mockController.Setup(b => b.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<CancellationToken>())).Returns(TaskHelpers.FromResult<HttpResponseMessage>(response));

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            controllerContext.ControllerDescriptor = _controllerDescriptor;
            controllerContext.Controller = mockController.Object;

            HttpActionContext actionContext = ContextUtil.CreateActionContext(controllerContext, actionDescriptor: _mockActionDescriptor.Object);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerTracer tracer = new HttpControllerTracer(mockController.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            HttpResponseMessage actualResponse = ((IHttpController)tracer).ExecuteAsync(controllerContext, CancellationToken.None).Result;

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(response, actualResponse);
        }

        [Fact]
        public void ExecuteAsync_Faults_And_Traces_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException();
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.TrySetException(exception);
            Mock<ApiController> mockController = new Mock<ApiController>() { CallBase = true };
            mockController.Setup(b => b.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<CancellationToken>())).Returns(tcs.Task);

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            controllerContext.ControllerDescriptor = _controllerDescriptor;
            controllerContext.Controller = mockController.Object;

            HttpActionContext actionContext = ContextUtil.CreateActionContext(controllerContext, actionDescriptor: _mockActionDescriptor.Object);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerTracer tracer = new HttpControllerTracer(mockController.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => ((IHttpController)tracer).ExecuteAsync(controllerContext, CancellationToken.None).Wait());

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }

        [Fact]
        public void ExecuteAsync_IsCancelled_And_Traces_When_Inner_IsCancelled()
        {
            // Arrange
            Mock<ApiController> mockController = new Mock<ApiController>() { CallBase = true };
            mockController.Setup(b => b.ExecuteAsync(It.IsAny<HttpControllerContext>(), It.IsAny<CancellationToken>())).Returns(TaskHelpers.Canceled<HttpResponseMessage>());

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            controllerContext.ControllerDescriptor = _controllerDescriptor;
            controllerContext.Controller = mockController.Object;

            HttpActionContext actionContext = ContextUtil.CreateActionContext(controllerContext, actionDescriptor: _mockActionDescriptor.Object);

            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerTracer tracer = new HttpControllerTracer(mockController.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(actionContext.Request, TraceCategories.ControllersCategory, TraceLevel.Warn) { Kind = TraceKind.End }
            };

            // Act
            Task task = ((IHttpController) tracer).ExecuteAsync(controllerContext,CancellationToken.None);
            Exception thrown = Assert.Throws<TaskCanceledException>(() => task.Wait());

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }
    }
}
