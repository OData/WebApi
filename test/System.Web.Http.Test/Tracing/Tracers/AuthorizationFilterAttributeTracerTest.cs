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
    public class AuthorizationFilterAttributeTracerTest
    {
        [Fact]
        public void ExecuteAuthorizationFilterAsync_Traces()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockAttr = new Mock<AuthorizationFilterAttribute>() { CallBase = true };
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            Func<Task<HttpResponseMessage>> continuation = () => TaskHelpers.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TestTraceWriter traceWriter = new TestTraceWriter();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttr.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "OnAuthorization" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.End,  Operation = "OnAuthorization" },
            };

            // Act
            Task task = ((IAuthorizationFilter)tracer).ExecuteAuthorizationFilterAsync(actionContext, CancellationToken.None, continuation);
            task.Wait();

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void ExecuteAuthorizationFilterAsync_Throws_And_Traces_When_Inner_OnException_Throws()
        {
            // Arrange
            Mock<AuthorizationFilterAttribute> mockAttr = new Mock<AuthorizationFilterAttribute>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockAttr.Setup(a => a.OnAuthorization(It.IsAny<HttpActionContext>())).Throws(exception);
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));
            HttpActionContext actionContext = ContextUtil.CreateActionContext(actionDescriptor: mockActionDescriptor.Object);
            Func<Task<HttpResponseMessage>> continuation = () => TaskHelpers.FromResult<HttpResponseMessage>(new HttpResponseMessage());
            TestTraceWriter traceWriter = new TestTraceWriter();
            AuthorizationFilterAttributeTracer tracer = new AuthorizationFilterAttributeTracer(mockAttr.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Info) { Kind = TraceKind.Begin, Operation = "OnAuthorization" },
                new TraceRecord(actionContext.Request, TraceCategories.FiltersCategory, TraceLevel.Error) { Kind = TraceKind.End,  Operation = "OnAuthorization" }
            };

            // Act
            Exception thrown =
                Assert.Throws<InvalidOperationException>(
                    () => ((IAuthorizationFilter)tracer).ExecuteAuthorizationFilterAsync(actionContext, CancellationToken.None, continuation).Wait());

            // Assert
            Assert.Same(exception, thrown);
            Assert.Same(exception, traceWriter.Traces[1].Exception);
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }
    }
}
