// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Hosting;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Dispatcher
{
    public class DefaultHttpControllerActivatorTest
    {
        // Create tests

        [Fact]
        public void Create_GuardClauses()
        {
            // Arrange
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            var descriptor = new HttpControllerDescriptor(config, "Simple", typeof(SimpleController));
            var activator = new DefaultHttpControllerActivator();

            // Act & assert
            Assert.ThrowsArgumentNull(() => activator.Create(request: null, controllerDescriptor: descriptor, controllerType: typeof(SimpleController)), "request");
            Assert.ThrowsArgumentNull(() => activator.Create(request, controllerDescriptor: null, controllerType: typeof(SimpleController)), "controllerDescriptor");
            Assert.ThrowsArgumentNull(() => activator.Create(request, descriptor, controllerType: null), "controllerType");
            Assert.Throws<InvalidOperationException>(
                () => activator.Create(request, descriptor, typeof(AbstractController)),
                "An error occurred when trying to create a controller of type 'AbstractController'. Make sure that the controller has a parameterless public constructor.");
            Assert.Throws<InvalidOperationException>(
                () => activator.Create(request, descriptor, typeof(ControllerWithCtorParams)),
                "An error occurred when trying to create a controller of type 'ControllerWithCtorParams'. Make sure that the controller has a parameterless public constructor.");
        }

        [Fact]
        public void Create_MakesInstanceOfController()
        {
            // Arrange
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            var descriptor = new HttpControllerDescriptor(config, "Simple", typeof(SimpleController));
            var activator = new DefaultHttpControllerActivator();

            // Act
            IHttpController result = activator.Create(request, descriptor, typeof(SimpleController));

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SimpleController>(result);
        }

        [Fact]
        public void Create_UsesControllerFromRequestLevelDependencyScope()
        {
            // Arrange
            var controller = new ControllerWithCtorParams(42);
            var mockScope = new Mock<IDependencyScope>();
            mockScope.Setup(r => r.GetService(typeof(ControllerWithCtorParams))).Returns(controller).Verifiable();
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            request.Properties[HttpPropertyKeys.DependencyScope] = mockScope.Object;
            var descriptor = new HttpControllerDescriptor(config, "Name", typeof(ControllerWithCtorParams));
            var activator = new DefaultHttpControllerActivator();

            // Act
            IHttpController result = activator.Create(request, descriptor, typeof(ControllerWithCtorParams));

            // Assert
            Assert.Same(controller, result);
            mockScope.Verify();
        }

        // Helper classes

        abstract class AbstractController : ApiController { }

        class SimpleController : ApiController { }

        class ControllerWithCtorParams : ApiController
        {
            public ControllerWithCtorParams(int unused) { }
        }
    }
}
