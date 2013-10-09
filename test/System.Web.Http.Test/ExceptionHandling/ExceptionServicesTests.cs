// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionServicesTests
    {
        [Fact]
        public void CreateLoggerWithServices_ReturnsCompositeExceptionLoggerWithServicesLoggers()
        {
            // Arrange
            IExceptionLogger expectedLogger = CreateDummyLogger();

            using (ServicesContainer services = CreateServices(expectedLogger))
            {
                // Act
                IExceptionLogger logger = ExceptionServices.CreateLogger(services);

                // Assert
                Assert.IsType<CompositeExceptionLogger>(logger);
                IEnumerable<IExceptionLogger> loggers = ((CompositeExceptionLogger)logger).Loggers;
                Assert.Equal(1, loggers.Count());
                Assert.Same(expectedLogger, loggers.Single());
            }
        }

        [Fact]
        public void CreateLoggerWithConfiguration_ReturnsCompositeExceptionLoggerWithServicesLoggers()
        {
            // Arrange
            IExceptionLogger expectedLogger = CreateDummyLogger();

            using (HttpConfiguration configuration = CreateConfiguration(expectedLogger))
            {
                // Act
                IExceptionLogger logger = ExceptionServices.CreateLogger(configuration);

                // Assert
                Assert.IsType<CompositeExceptionLogger>(logger);
                IEnumerable<IExceptionLogger> loggers = ((CompositeExceptionLogger)logger).Loggers;
                Assert.Equal(1, loggers.Count());
                Assert.Same(expectedLogger, loggers.Single());
            }
        }

        [Fact]
        public void CreateLoggerWithConfiguration_IfConfigurationIsNull_Throws()
        {
            // Arrange
            HttpConfiguration configuration = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => ExceptionServices.CreateLogger(configuration), "configuration");
        }

        [Fact]
        public void CreateHandlerWithServices_ReturnsLastChanceHandlerWithServicesInnerHandler()
        {
            // Arrange
            IExceptionHandler expectedHandler = CreateDummyHandler();

            using (ServicesContainer services = CreateServices(expectedHandler))
            {
                // Act
                IExceptionHandler handler = ExceptionServices.CreateHandler(services);

                // Assert
                Assert.IsType<LastChanceExceptionHandler>(handler);
                IExceptionHandler innerHandler = ((LastChanceExceptionHandler)handler).InnerHandler;
                Assert.Same(expectedHandler, innerHandler);
            }
        }

        [Fact]
        public void CreateHandlerWithServices_IfHandlerIsAbsent_ReturnsLastChanceHandlerWithEmptyInnerHandler()
        {
            // Arrange
            IExceptionHandler servicesHandler = null;

            using (ServicesContainer services = CreateServices(servicesHandler))
            {
                // Act
                IExceptionHandler handler = ExceptionServices.CreateHandler(services);

                // Assert
                Assert.IsType<LastChanceExceptionHandler>(handler);
                IExceptionHandler innerHandler = ((LastChanceExceptionHandler)handler).InnerHandler;
                Assert.IsType<EmptyExceptionHandler>(innerHandler);
            }
        }

        [Fact]
        public void CreateHandlerWithConfiguration_ReturnsLastChanceHandlerWithServicesInnerHandler()
        {
            // Arrange
            IExceptionHandler expectedHandler = CreateDummyHandler();

            using (HttpConfiguration configuration = CreateConfiguration(expectedHandler))
            {
                // Act
                IExceptionHandler handler = ExceptionServices.CreateHandler(configuration);

                // Assert
                Assert.IsType<LastChanceExceptionHandler>(handler);
                IExceptionHandler innerHandler = ((LastChanceExceptionHandler)handler).InnerHandler;
                Assert.Same(expectedHandler, innerHandler);
            }
        }

        [Fact]
        public void CreateHandlerWithConfiguration_IfConfigurationIsNull_Throws()
        {
            // Arrange
            HttpConfiguration configuration = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => ExceptionServices.CreateHandler(configuration), "configuration");
        }

        private static HttpConfiguration CreateConfiguration(IExceptionHandler handler)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.Replace(typeof(IExceptionHandler), handler);
            return configuration;
        }

        private static HttpConfiguration CreateConfiguration(params IExceptionLogger[] loggers)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.Services.ReplaceRange(typeof(IExceptionLogger), loggers);
            return configuration;
        }

        private static IExceptionHandler CreateDummyHandler()
        {
            return new Mock<IExceptionHandler>(MockBehavior.Strict).Object;
        }

        private static IExceptionLogger CreateDummyLogger()
        {
            return new Mock<IExceptionLogger>(MockBehavior.Strict).Object;
        }

        private static ServicesContainer CreateServices(IExceptionHandler handler)
        {
            Mock<ServicesContainer> mock = new Mock<ServicesContainer>(MockBehavior.Strict);
            mock.Setup(s => s.GetService(typeof(IExceptionHandler))).Returns(handler);
            mock.Setup(s => s.Dispose());
            return mock.Object;
        }

        private static ServicesContainer CreateServices(params IExceptionLogger[] loggers)
        {
            Mock<ServicesContainer> mock = new Mock<ServicesContainer>(MockBehavior.Strict);
            mock.Setup(s => s.GetServices(typeof(IExceptionLogger))).Returns(loggers);
            mock.Setup(s => s.Dispose());
            return mock.Object;
        }
    }
}
