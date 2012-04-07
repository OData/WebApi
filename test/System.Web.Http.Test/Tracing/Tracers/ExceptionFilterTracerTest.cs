// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class ExceptionFilterTracerTest
    {
        [Fact]
        public void ExecuteExceptionFilterAsync_Traces()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<IExceptionFilter> mockFilter = new Mock<IExceptionFilter>() { CallBase = true };
            mockFilter.Setup(
                f => f.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(), It.IsAny<CancellationToken>())).
                Returns(TaskHelpers.Completed());
            HttpActionExecutedContext actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ExceptionFilterTracer tracer = new ExceptionFilterTracer(mockFilter.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteExceptionFilterAsync" },
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "ExecuteExceptionFilterAsync" },
            };

            // Act
            Task task = ((IExceptionFilter)tracer).ExecuteExceptionFilterAsync(actionExecutedContext, CancellationToken.None);
            task.Wait();

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void ExecuteExceptionFilterAsync_Faults_And_Traces_When_Inner_Faults()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<IExceptionFilter> mockFilter = new Mock<IExceptionFilter>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
            tcs.TrySetException(exception);
            mockFilter.Setup(a => a.ExecuteExceptionFilterAsync(It.IsAny<HttpActionExecutedContext>(), It.IsAny<CancellationToken>())).Returns(tcs.Task);
            HttpActionExecutedContext actionExecutedContext = ContextUtil.GetActionExecutedContext(request, response);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ExceptionFilterTracer tracer = new ExceptionFilterTracer(mockFilter.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteExceptionFilterAsync" },
                new TraceRecord(request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "ExecuteExceptionFilterAsync" }
            };

            // Act
            Task task = ((IExceptionFilter)tracer).ExecuteExceptionFilterAsync(actionExecutedContext, CancellationToken.None);


            // Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(() => task.Wait());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }
    }
}
