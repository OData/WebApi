using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Tracing.Tracers;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing
{
    public class TraceManagerTest
    {
        [Fact]
        public void TraceManager_Is_In_Default_ServiceResolver()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            ITraceManager traceManager = config.ServiceResolver.GetService(typeof (ITraceManager)) as ITraceManager;

            // Assert
            Assert.IsType<TraceManager>(traceManager);
        }

        [Theory]
        [InlineData(typeof(IHttpControllerFactory))]
        [InlineData(typeof(IHttpControllerActivator))]
        [InlineData(typeof(IHttpActionSelector))]
        [InlineData(typeof(IHttpActionInvoker))]
        [InlineData(typeof(IActionValueBinder))]
        [InlineData(typeof(IContentNegotiator))]
        public void Initialize_Does_Not_Alter_Configuration_When_No_TraceWriter_Present(Type serviceType)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            object defaultService = config.ServiceResolver.GetService(serviceType);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            Assert.Same(defaultService.GetType(), config.ServiceResolver.GetService(serviceType).GetType());
        }

        [Theory]
        [InlineData(typeof(IHttpControllerFactory))]
        [InlineData(typeof(IHttpControllerActivator))]
        [InlineData(typeof(IHttpActionSelector))]
        [InlineData(typeof(IHttpActionInvoker))]
        [InlineData(typeof(IActionValueBinder))]
        [InlineData(typeof(IContentNegotiator))]
        public void Initialize_Alters_Configuration_When_TraceWriter_Present(Type serviceType)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> traceWriter = new Mock<ITraceWriter>() { CallBase = true };
            config.ServiceResolver.SetService(typeof(ITraceWriter), traceWriter.Object);
            object defaultService = config.ServiceResolver.GetService(serviceType);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            Assert.NotSame(defaultService.GetType(), config.ServiceResolver.GetService(serviceType).GetType());
        }

        [Fact]
        public void Initialize_Does_Not_Alter_MessageHandlers_When_No_TraceWriter_Present()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<DelegatingHandler> mockHandler = new Mock<DelegatingHandler>() { CallBase = true };
            config.MessageHandlers.Add(mockHandler.Object);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            Assert.Equal(config.MessageHandlers[config.MessageHandlers.Count - 1].GetType(), mockHandler.Object.GetType());
        }

        [Fact]
        public void Initialize_Alters_MessageHandlers_WhenTraceWriter_Present()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> traceWriter = new Mock<ITraceWriter>() { CallBase = true };
            config.ServiceResolver.SetService(typeof(ITraceWriter), traceWriter.Object);
            Mock<DelegatingHandler> mockHandler = new Mock<DelegatingHandler>() { CallBase = true };
            config.MessageHandlers.Add(mockHandler.Object);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            Assert.IsAssignableFrom<RequestMessageHandlerTracer>(config.MessageHandlers[config.MessageHandlers.Count - 1]);
            Assert.IsAssignableFrom<MessageHandlerTracer>(config.MessageHandlers[config.MessageHandlers.Count - 2]);
        }

        [Fact]
        public void Initialize_Does_Not_Alter_MediaTypeFormatters_When_No_TraceWriter_Present()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            new TraceManager().Initialize(config);

            // Assert
            foreach (var formatter in config.Formatters)
            {
                Assert.False(typeof(IFormatterTracer).IsAssignableFrom(formatter.GetType()));
            }
        }

        [Fact]
        public void Initialize_Alters_MediaTypeFormatters_WhenTraceWriter_Present()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<ITraceWriter> traceWriter = new Mock<ITraceWriter>() { CallBase = true };
            config.ServiceResolver.SetService(typeof(ITraceWriter), traceWriter.Object);

            // Act
            new TraceManager().Initialize(config);

            // Assert
            foreach (var formatter in config.Formatters)
            {
                Assert.IsAssignableFrom<IFormatterTracer>(formatter);
            }
        }
    }
}
