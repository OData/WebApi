using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpControllerFactoryTracerTest
    {
        [Fact]
        public void CreateController_Invokes_Inner_And_Traces()
        {
            // Arrange
            Mock<ApiController> mockController = new Mock<ApiController>();
            Mock<IHttpControllerFactory> mockFactory = new Mock<IHttpControllerFactory>() { CallBase = true };
            mockFactory.Setup(b => b.CreateController(It.IsAny<HttpControllerContext>(), It.IsAny<string>())).Returns(mockController.Object);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerFactoryTracer tracer = new HttpControllerFactoryTracer(mockFactory.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(controllerContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "CreateController" },
                new TraceRecord(controllerContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "CreateController" }
            };

            // Act
            IHttpController createdController = ((IHttpControllerFactory)tracer).CreateController(controllerContext, "anyName");

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.IsAssignableFrom<HttpControllerTracer>(createdController);
        }

        [Fact]
        public void CreateController_Throws_And_Traces_When_Inner_Throws()
        {
            // Arrange
            Mock<IHttpControllerFactory> mockFactory = new Mock<IHttpControllerFactory>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockFactory.Setup(b => b.CreateController(It.IsAny<HttpControllerContext>(), It.IsAny<string>())).Throws(exception);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerFactoryTracer tracer = new HttpControllerFactoryTracer(mockFactory.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(controllerContext.Request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "CreateController" },
                new TraceRecord(controllerContext.Request, TraceCategories.ControllersCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "CreateController" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => ((IHttpControllerFactory)tracer).CreateController(controllerContext, "anyName"));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }
    }
}
