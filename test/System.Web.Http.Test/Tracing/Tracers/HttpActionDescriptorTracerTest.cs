using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpActionDescriptorTracerTest
    {
        // This test verifies only one kind of filter is wrapped, proving
        // the static FilterTracer.CreateFilterTracer was called from GetFilterPipeline.  
        // Deeper testing of FilterTracer.CreateFilterTracer is in FilterTracerTest.
        [Fact]
        public void GetFilterPipeline_Returns_Wrapped_Filters()
        {
            // Arrange
            Mock<IFilter> mockFilter = new Mock<IFilter>();
            FilterInfo filter = new FilterInfo(mockFilter.Object, FilterScope.First);
            Collection<FilterInfo> filterCollection = new Collection<FilterInfo>(new FilterInfo[] { filter });
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetFilterPipeline()).Returns(filterCollection);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, new TestTraceWriter());

            // Act
            Collection<FilterInfo> wrappedFilterCollection = tracer.GetFilterPipeline();

            // Assert
            Assert.IsType<FilterTracer>(wrappedFilterCollection[0].Instance);
        }

        // This test verifies only one kind of filter is wrapped, proving
        // the static FilterTracer.CreateFilterTracer was called from GetFilterPipeline.  
        // Deeper testing of FilterTracer.CreateFilterTracer is in FilterTracerTest.
        [Fact]
        public void GetFilters_Returns_Wrapped_IFilters()
        {
            // Arrange
            Mock<IFilter> mockFilter = new Mock<IFilter>();
            Collection<IFilter> filters = new Collection<IFilter>(new IFilter[] { mockFilter.Object });
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetFilters()).Returns(filters);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, new TestTraceWriter());

            // Act
            IFilter[] wrappedFilters = tracer.GetFilters().ToArray();

            // Assert
            Assert.IsType<FilterTracer>(wrappedFilters[0]);
        }

        [Fact]
        public void Execute_Invokes_Inner_Execute()
        {
            // Arrange
            bool executeCalled = false;
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(
                a => a.Execute(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>())).Callback(() => executeCalled = true);
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            IDictionary<string, object> arguments = new Dictionary<string, object>();
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, new TestTraceWriter());

            // Act
            tracer.Execute(controllerContext, arguments);

            // Assert
            Assert.True(executeCalled);
        }

        [Fact]
        public void Execute_Traces()
        {
            // Arrange
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(
                a => a.Execute(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>())).Callback(() => {});
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            IDictionary<string, object> arguments = new Dictionary<string, object>();
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(controllerContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(controllerContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.End }
            };

            // Act
            tracer.Execute(controllerContext, arguments);

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
        }

        [Fact]
        public void Execute_Throws_What_Inner_Throws_And_Traces()
        {
            // Arrange
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            InvalidOperationException exception = new InvalidOperationException("test");
            mockActionDescriptor.Setup(
                a => a.Execute(It.IsAny<HttpControllerContext>(), It.IsAny<IDictionary<string, object>>())).Throws(exception);
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext();
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ApiController));
            IDictionary<string, object> arguments = new Dictionary<string, object>();
            TestTraceWriter traceWriter = new TestTraceWriter();
            HttpActionDescriptorTracer tracer = new HttpActionDescriptorTracer(controllerContext, mockActionDescriptor.Object, traceWriter);
            TraceRecord[] expectedTraces = new TraceRecord[]
            {
                new TraceRecord(controllerContext.Request, TraceCategories.ActionCategory, TraceLevel.Info) { Kind = TraceKind.Begin },
                new TraceRecord(controllerContext.Request, TraceCategories.ActionCategory, TraceLevel.Error) { Kind = TraceKind.End }
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => tracer.Execute(controllerContext, arguments));

            // Assert
            Assert.Equal<TraceRecord>(expectedTraces, traceWriter.Traces, new TraceRecordComparer());
            Assert.Same(exception, traceWriter.Traces[1].Exception);
        }
    }
}
