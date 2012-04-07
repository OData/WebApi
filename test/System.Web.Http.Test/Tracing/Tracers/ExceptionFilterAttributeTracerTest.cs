// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class ExceptionFilterAttributeTracerTest
    {
        [Fact]
        public void ExecuteExceptionFilterAsync_Traces()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<ExceptionFilterAttribute> mockAttr = new Mock<ExceptionFilterAttribute>() { CallBase = true };
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionExecutedContext actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttr.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "OnException" },
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "OnException" },
            };

            // Act
            Task task = ((IExceptionFilter)tracer).ExecuteExceptionFilterAsync(actionExecutedContext, CancellationToken.None);
            task.Wait();

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void ExecuteExceptionFilterAsync_Throws_And_Traces_When_Inner_OnException_Throws()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<ExceptionFilterAttribute> mockAttr = new Mock<ExceptionFilterAttribute>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockAttr.Setup(a => a.OnException(It.IsAny<HttpActionExecutedContext>())).Throws(exception);
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionExecutedContext actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttr.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "OnException" },
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "OnException" }
            };

            // Act
            Exception thrown =
                Assert.Throws<InvalidOperationException>(
                    () => ((IExceptionFilter) tracer).ExecuteExceptionFilterAsync(actionExecutedContext, CancellationToken.None));

            // Assert
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }
    }
}
