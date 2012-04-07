// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Tracing.Tracers
{
    public class ActionValueBinderTracerTest
    {
        [Fact]
        public void GetBinding_Returns_HttpActionBindingTracer()
        {
            // Arrange
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));

            Mock<HttpParameterDescriptor> mockParameterDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            Mock<HttpParameterBinding> mockParameterBinding = new Mock<HttpParameterBinding>(mockParameterDescriptor.Object) { CallBase = true };
            HttpActionBinding actionBinding = new HttpActionBinding(mockActionDescriptor.Object, new HttpParameterBinding[] { mockParameterBinding.Object });

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "controller", typeof(ApiController));

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            controllerContext.ControllerDescriptor = controllerDescriptor;

            Mock<IActionValueBinder> mockBinder = new Mock<IActionValueBinder>() { CallBase = true };
            mockBinder.Setup(b => b.GetBinding(It.IsAny<HttpActionDescriptor>())).Returns(actionBinding);
            ActionValueBinderTracer tracer = new ActionValueBinderTracer(mockBinder.Object, new TestTraceWriter());

            // Act
            HttpActionBinding actualBinding = ((IActionValueBinder)tracer).GetBinding(mockActionDescriptor.Object);

            // Assert
            Assert.IsType<HttpActionBindingTracer>(actualBinding);
            Assert.Same(mockActionDescriptor.Object, actualBinding.ActionDescriptor);
        }

        [Fact]
        public void GetBinding_Invokes_Inner_And_Returns_ActionBinder_With_Tracing_HttpParameterBinding()
        {
            // Arrange
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));

            Mock<HttpParameterDescriptor> mockParameterDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            Mock<HttpParameterBinding> mockParameterBinding = new Mock<HttpParameterBinding>(mockParameterDescriptor.Object) { CallBase = true };
            HttpActionBinding actionBinding = new HttpActionBinding(mockActionDescriptor.Object, new HttpParameterBinding[] { mockParameterBinding.Object });

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "controller", typeof(ApiController));

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            controllerContext.ControllerDescriptor = controllerDescriptor;

            Mock<IActionValueBinder> mockBinder = new Mock<IActionValueBinder>() {CallBase = true};
            mockBinder.Setup(b => b.GetBinding(It.IsAny<HttpActionDescriptor>())).Returns(actionBinding);
            ActionValueBinderTracer tracer = new ActionValueBinderTracer(mockBinder.Object, new TestTraceWriter());

            // Act
            HttpActionBinding actualBinding = ((IActionValueBinder) tracer).GetBinding(mockActionDescriptor.Object);

            // Assert
            Assert.IsAssignableFrom<HttpParameterBindingTracer>(actualBinding.ParameterBindings[0]);
        }

        [Fact]
        public void GetBinding_Invokes_Inner_And_Returns_ActionBinder_With_Tracing_FormatterParameterBinding()
        {
            // Arrange
            Mock<HttpActionDescriptor> mockActionDescriptor = new Mock<HttpActionDescriptor>() { CallBase = true };
            mockActionDescriptor.Setup(a => a.ActionName).Returns("test");
            mockActionDescriptor.Setup(a => a.GetParameters()).Returns(new Collection<HttpParameterDescriptor>(new HttpParameterDescriptor[0]));

            Mock<HttpParameterDescriptor> mockParameterDescriptor = new Mock<HttpParameterDescriptor>() { CallBase = true };
            Mock<FormatterParameterBinding> mockParameterBinding = new Mock<FormatterParameterBinding>(mockParameterDescriptor.Object, new MediaTypeFormatterCollection(), null) { CallBase = true };
            HttpActionBinding actionBinding = new HttpActionBinding(mockActionDescriptor.Object, new HttpParameterBinding[] { mockParameterBinding.Object });

            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor(new HttpConfiguration(), "controller", typeof(ApiController));

            HttpControllerContext controllerContext = ContextUtil.CreateControllerContext(request: new HttpRequestMessage());
            controllerContext.ControllerDescriptor = controllerDescriptor;

            Mock<IActionValueBinder> mockBinder = new Mock<IActionValueBinder>() { CallBase = true };
            mockBinder.Setup(b => b.GetBinding(It.IsAny<HttpActionDescriptor>())).Returns(actionBinding);
            ActionValueBinderTracer tracer = new ActionValueBinderTracer(mockBinder.Object, new TestTraceWriter());

            // Act
            HttpActionBinding actualBinding = ((IActionValueBinder)tracer).GetBinding(mockActionDescriptor.Object);

            // Assert
            Assert.IsAssignableFrom<FormatterParameterBindingTracer>(actualBinding.ParameterBindings[0]);
        }

    }
}
