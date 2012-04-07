// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Moq;
using Xunit;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpControllerDescriptorTracerTest
    {
        private static readonly HttpControllerContext _controllerContext = ContextUtil.CreateControllerContext(instance: _controller);
        private static readonly HttpRequestMessage _request = _controllerContext.Request;
        private static readonly IHttpController _controller = new Mock<IHttpController>().Object;
        private static readonly InvalidOperationException _exception = new InvalidOperationException("test");

        [Fact]
        public void CreateController_Invokes_Inner_And_Traces()
        {
            // Arrange
            Mock<HttpControllerDescriptor> mockControllerDescriptor = CreateMockControllerDescriptor();
            mockControllerDescriptor.Setup(b => b.CreateController(It.IsAny<HttpRequestMessage>())).Returns(_controller);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerDescriptorTracer tracer = GetHttpControllerDescriptorTracer(mockControllerDescriptor.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "CreateController" },
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "CreateController" }
            };

            // Act
            IHttpController controller = tracer.CreateController(_request);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.IsAssignableFrom<HttpControllerTracer>(controller);
        }

        [Fact]
        public void CreateController_Throws_And_Traces_When_Inner_Throws()
        {
            // Arrange
            Mock<HttpControllerDescriptor> mockControllerDescriptor = CreateMockControllerDescriptor();
            mockControllerDescriptor.Setup(b => b.CreateController(It.IsAny<HttpRequestMessage>())).Throws(_exception);
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerDescriptorTracer tracer = GetHttpControllerDescriptorTracer(mockControllerDescriptor.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "CreateController" },
                new TraceRecord(_request, TraceCategories.ControllersCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "CreateController" }
            };

            // Act
            Exception thrown = Assert.Throws<InvalidOperationException>(() => tracer.CreateController(_request));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(_exception, thrown);
            Assert.Same(_exception, traceWriter.Traces[1].Exception);
        }

        private static HttpControllerDescriptorTracer GetHttpControllerDescriptorTracer(HttpControllerDescriptor controllerDescriptor, ITraceWriter traceWriter)
        {
            return new HttpControllerDescriptorTracer(
                configuration: new HttpConfiguration(),
                controllerName: "AnyController",
                controllerType: _controller.GetType(),
                innerDescriptor: controllerDescriptor,
                traceWriter: traceWriter);
        }

        private static Mock<HttpControllerDescriptor> CreateMockControllerDescriptor()
        {
            Mock<HttpControllerDescriptor> mockControllerDescriptor = new Mock<HttpControllerDescriptor>(_controllerContext.Configuration, "AnyController", _controller.GetType());
            return mockControllerDescriptor;
        }
    }
}
