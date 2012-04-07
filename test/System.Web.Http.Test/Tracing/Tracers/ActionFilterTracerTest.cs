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
    public class ActionFilterTracerTest
    {
        [Fact]
        public void ExecuteActionAsync_Traces_ExecuteActionFilterAsync()
        {
            // Arrange
            HttpResponseMessage response = new HttpResponseMessage();
            Mock<IActionFilter> mockFilter = new Mock<IActionFilter>() { CallBase = true };
            mockFilter.Setup(
                f =>
                f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>(),
                                           It.IsAny<Func<Task<HttpResponseMessage>>>())).Returns(
                                               TaskHelpers.FromResult<HttpResponseMessage>(response));
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ActionFilterTracer tracer = new ActionFilterTracer(mockFilter.Object, traceWriter);
            Func<Task<HttpResponseMessage>> continuation =
                () => TaskHelpers.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteActionFilterAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "ExecuteActionFilterAsync" },
            };

            // Act
            Task<HttpResponseMessage> task = ((IActionFilter)tracer).ExecuteActionFilterAsync(actionContext, CancellationToken.None, continuation);
            task.Wait();

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void ExecuteActionAsync_Faults_And_Traces_When_Inner_Faults()
        {
            // Arrange
            InvalidOperationException exception = new InvalidOperationException("test");
            Mock<IActionFilter> mockFilter = new Mock<IActionFilter>() { CallBase = true };
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>(null);
            tcs.TrySetException(exception);
            mockFilter.Setup(f => f.ExecuteActionFilterAsync(It.IsAny<HttpActionContext>(), It.IsAny<CancellationToken>(),
                                It.IsAny<Func<Task<HttpResponseMessage>>>())).Returns(tcs.Task);
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ActionFilterTracer tracer = new ActionFilterTracer(mockFilter.Object, traceWriter);
            Func<Task<HttpResponseMessage>> continuation =
                () => TaskHelpers.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ExecuteActionFilterAsync" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "ExecuteActionFilterAsync" },
            };

            // Act
            Task<HttpResponseMessage> task = ((IActionFilter)tracer).ExecuteActionFilterAsync(actionContext, CancellationToken.None, continuation);


            // Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(() => task.Wait());
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }
    }
}
