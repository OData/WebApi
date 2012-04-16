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
        public void Equals_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.Equals(randomObject)).Returns(true).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.Equals(randomObject);

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void GetHashCode_Calls_Inner()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.GetHashCode()).Returns(1).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            int valueReturned = tracer.GetHashCode();

            // Assert
            Assert.Equal(1, valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void IsDefaultAttribute_Calls_Inner()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.IsDefaultAttribute()).Returns(true).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.IsDefaultAttribute();

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void Match_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.Match(randomObject)).Returns(true).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.Match(randomObject);

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void TypeId_Calls_Inner()
        {
            // Arrange
            object randomObject = new Object();
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.TypeId).Returns(randomObject).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            object valueReturned = tracer.TypeId;

            // Assert
            Assert.Same(randomObject, valueReturned);
            mockAttribute.Verify();
        }

        [Fact]
        public void AllowMultiple_Calls_Inner()
        {
            // Arrange
            Mock<ExceptionFilterAttribute> mockAttribute = new Mock<ExceptionFilterAttribute>();
            mockAttribute.Setup(a => a.AllowMultiple).Returns(true).Verifiable();
            ExceptionFilterAttributeTracer tracer = new ExceptionFilterAttributeTracer(mockAttribute.Object, new TestTraceWriter());

            // Act
            bool valueReturned = tracer.AllowMultiple;

            // Assert
            Assert.True(valueReturned);
            mockAttribute.Verify();
        }

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
