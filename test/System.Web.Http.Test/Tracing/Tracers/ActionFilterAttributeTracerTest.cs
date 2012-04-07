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
    public class ActionFilterAttributeTracerTest
    {
        [Fact]
        public void ExecuteActionFilterAsync_Traces_Executing_And_Executed()
        {
            // Arrange
            Mock<ActionFilterAttribute> mockAttr = new Mock<ActionFilterAttribute>() { CallBase = true };
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() {CallBase = true};
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ActionFilterAttributeTracer tracer = new ActionFilterAttributeTracer(mockAttr.Object, traceWriter);
            Func<Task<HttpResponseMessage>> continuation =
                () => TaskHelpers.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ActionExecuting" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "ActionExecuting" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ActionExecuted" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "ActionExecuted" }
            };

            // Act
            Task<HttpResponseMessage> task = ((IActionFilter) tracer).ExecuteActionFilterAsync(actionContext, CancellationToken.None, continuation);
            task.Wait();

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void ExecuteActionFilterAsync_Faults_And_Traces_When_OnExecuting_Faults()
        {
            // Arrange
            Mock<ActionFilterAttribute> mockAttr = new Mock<ActionFilterAttribute>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockAttr.Setup(a => a.OnActionExecuting(It.IsAny<HttpActionContext>())).Throws(exception);
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ActionFilterAttributeTracer tracer = new ActionFilterAttributeTracer(mockAttr.Object, traceWriter);
            Func<Task<HttpResponseMessage>> continuation =
                () => TaskHelpers.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ActionExecuting" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "ActionExecuting" }
            };

            // Act
            Task<HttpResponseMessage> task = ((IActionFilter)tracer).ExecuteActionFilterAsync(actionContext, CancellationToken.None, continuation);

            // Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(() => task.Wait());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void ExecuteActionFilterAsync_Faults_And_Traces_When_OnExecuted_Faults()
        {
            // Arrange
            Mock<ActionFilterAttribute> mockAttr = new Mock<ActionFilterAttribute>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockAttr.Setup(a => a.OnActionExecuted(It.IsAny<HttpActionExecutedContext>())).Throws(exception);
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            TestTraceWriter traceWriter = new TestTraceWriter();
            ActionFilterAttributeTracer tracer = new ActionFilterAttributeTracer(mockAttr.Object, traceWriter);
            Func<Task<HttpResponseMessage>> continuation =
                () => TaskHelpers.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ActionExecuting" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "ActionExecuting" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "ActionExecuted" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "ActionExecuted" }
            };

            // Act
            Task<HttpResponseMessage> task = ((IActionFilter)tracer).ExecuteActionFilterAsync(actionContext, CancellationToken.None, continuation);

            // Assert
            Exception thrown = Assert.Throws<InvalidOperationException>(() => task.Wait());
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[3].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }
    }
}
