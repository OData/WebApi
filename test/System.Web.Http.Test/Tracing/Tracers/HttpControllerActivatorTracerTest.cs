// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpControllerActivatorTracerTest
    {
        [Fact]
        public void Create_Invokes_Inner_And_Traces()
        {
            // Arrange
            Mock<ApiController> mockController = new Mock<ApiController>();
            Mock<IHttpControllerActivator> mockActivator = new Mock<IHttpControllerActivator>() { CallBase = true };
            mockActivator.Setup(b => b.Create(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpControllerDescriptor>(), It.IsAny<Type>())).Returns(mockController.Object);
            HttpRequestMessage request = new HttpRequestMessage();
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerActivatorTracer tracer = new HttpControllerActivatorTracer(mockActivator.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "Create" },
                new TraceRecord(request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.End, Operation = "Create" }
            };

            // Act
            IHttpController createdController = ((IHttpControllerActivator)tracer).Create(request, controllerDescriptor: null, controllerType: mockController.Object.GetType());

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.IsAssignableFrom<HttpControllerTracer>(createdController);
        }

        [Fact]
        public void Create_Throws_And_Traces_When_Inner_Throws()
        {
            // Arrange
            Mock<ApiController> mockController = new Mock<ApiController>();
            Mock<IHttpControllerActivator> mockActivator = new Mock<IHttpControllerActivator>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockActivator.Setup(b => b.Create(It.IsAny<HttpRequestMessage>(), It.IsAny<HttpControllerDescriptor>(), It.IsAny<Type>())).Throws(exception);
            HttpRequestMessage request = new HttpRequestMessage();
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpControllerActivatorTracer tracer = new HttpControllerActivatorTracer(mockActivator.Object, traceWriter);

            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.ControllersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "Create" },
                new TraceRecord(request, TraceCategories.ControllersCategory, TraceLevel.Error) { Kind = TraceKind.End, Operation = "Create" }
            };

            // Act & Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(() => ((IHttpControllerActivator)tracer).Create(request, controllerDescriptor: null, controllerType: mockController.Object.GetType()));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }
    }
}
