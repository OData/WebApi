// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Dispatcher
{
    public class DefaultHttpControllerSelectorTest
    {
        [Fact]
        public void Constructor_Throws_NullConfiguration()
        {
            Assert.ThrowsArgumentNull(
                () => new DefaultHttpControllerSelector(configuration: null),
                "configuration");
        }

        [Theory]
        [InlineData("controller", "abc")]
        [InlineData("Controller", "123")]
        [InlineData("ControLler", "123")]
        [InlineData("CONTROLLER", "ABC")]
        public void GetControllerName_PicksControllerNameFromRouteData(string controllerKeyName, string controllerName)
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values[controllerKeyName] = controllerName;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(new HttpConfiguration());

            // Act 
            string selectedControllerName = selector.GetControllerName(request);

            // Assert
            Assert.Equal(controllerName, selectedControllerName);
        }

        [Fact]
        public void GetControllerName_PicksNull_NoRouteData()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(new HttpConfiguration());

            // Act 
            string selectedControllerName = selector.GetControllerName(request);

            // Assert
            Assert.Null(selectedControllerName);
        }

        [Fact]
        public void GetControllerName_PicksNull_EmptyRouteData()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = GetRouteData();
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(new HttpConfiguration());

            // Act 
            string selectedControllerName = selector.GetControllerName(request);

            // Assert
            Assert.Null(selectedControllerName);
        }

        [Fact]
        public void DefaultHttpControllerSelector_Uses_IAssemblyResolverAndIHttpControllerTypeResolver()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IAssembliesResolver> assemblyResolver = new Mock<IAssembliesResolver>();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IAssembliesResolver), assemblyResolver.Object);
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            controllerTypeResolver.Setup(c => c.GetControllerTypes(assemblyResolver.Object)).Returns(new Collection<Type> { GetMockControllerType("Sample") }).Verifiable();
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = "Sample";
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            // Act
            selector.SelectController(request);

            // Assert
            controllerTypeResolver.Verify();
        }

        [Theory]
        [InlineData("Sample")]
        [InlineData("SAmple")]
        [InlineData("SAMPLE")]
        public void SelectController_IsCaseInsensitive(string controllerTypeName)
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample");
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = controllerTypeName;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            HttpControllerDescriptor descriptor = selector.SelectController(request);

            // Assert
            Assert.IsType(typeof(HttpControllerDescriptor), descriptor);
            Assert.Equal(controllerType, descriptor.ControllerType);
        }

        [Fact]
        public void SelectController_DoesNotCreateNewInstances_ForSameController()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample");
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = "Sample";
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            HttpControllerDescriptor descriptor1 = selector.SelectController(request);
            HttpControllerDescriptor descriptor2 = selector.SelectController(request);

            // Assert
            Assert.ReferenceEquals(descriptor1, descriptor2);
        }

        [Fact]
        public void SelectController_DoesNotCreateNewInstances_ForSameController_DiferentCasedControllerName()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample");
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            HttpRequestMessage request1 = new HttpRequestMessage();
            IHttpRouteData routeData1 = GetRouteData();
            routeData1.Values["controller"] = "Sample";
            request1.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData1;

            HttpRequestMessage request2 = new HttpRequestMessage();
            IHttpRouteData routeData2 = GetRouteData();
            routeData2.Values["controller"] = "SaMPle";
            request2.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData2;

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act
            HttpControllerDescriptor descriptor1 = selector.SelectController(request1);
            HttpControllerDescriptor descriptor2 = selector.SelectController(request2);

            // Assert
            Assert.ReferenceEquals(descriptor1, descriptor2);
        }

        [Fact]
        public void SelectController_Throws_NullRequest()
        {
            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(new HttpConfiguration());

            Assert.ThrowsArgumentNull(
                () => selector.SelectController(request: null),
                "request");
        }

        [Fact]
        public void SelectController_Throws_NotFound_NoControllerInRouteData()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = GetRouteData();

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act 
            var ex = Assert.Throws<HttpResponseException>(
                () => selector.SelectController(request));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }

        [Fact]
        public void SelectController_Throws_NotFound_NoRouteData()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            HttpRequestMessage request = new HttpRequestMessage();

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act 
            var ex = Assert.Throws<HttpResponseException>(
                () => selector.SelectController(request));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
        }


        [Fact]
        public void SelectController_Throws_NotFound_NoMatchingControllerType()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type>());

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData1 = GetRouteData();
            routeData1.Values["controller"] = "Sample";
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData1;
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act 
            var ex = Assert.Throws<HttpResponseException>(
                () => selector.SelectController(request));

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
            Assert.Equal("\"No type was found that matches the controller named 'Sample'.\"", ex.Response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void SelectController_Throws_DuplicateController()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            Mock<IHttpControllerTypeResolver> controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { GetMockControllerType("Sample"), GetMockControllerType("SampLe"), GetMockControllerType("SAmpLE") });

            HttpRequestMessage request = new HttpRequestMessage();
            IHttpRouteData routeData1 = GetRouteData();
            routeData1.Values["controller"] = "Sample";
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData1;
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            DefaultHttpControllerSelector selector = new DefaultHttpControllerSelector(configuration);

            // Act 
            var ex = Assert.Throws<HttpResponseException>(
                () => selector.SelectController(request));

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, ex.Response.StatusCode);
            string response = ex.Response.Content.ReadAsAsync<string>().Result;
            Assert.Contains(
                "Multiple types were found that match the controller named 'Sample'. This can happen if the route that services this request ('') found multiple controllers defined with the same name but differing namespaces, which is not supported.\r\n\r\nThe request for 'Sample' has found the following matching controllers:",
                response);

            var duplicateControllers = response.Split(':')[1].Split('\n').Select(str => str.Trim());
            Assert.Contains("FullSampleController", duplicateControllers);
            Assert.Contains("FullSampLeController", duplicateControllers);
            Assert.Contains("FullSAmpLEController", duplicateControllers);
        }

        private static IHttpRouteData GetRouteData()
        {
            IHttpRoute mockRoute = new Mock<IHttpRoute>().Object;
            HttpRouteData routeData = new HttpRouteData(mockRoute);

            return routeData;
        }

        private static Type GetMockControllerType(string controllerName)
        {
            Mock<Type> mockType = new Mock<Type>();

            mockType.Setup(t => t.Name).Returns(controllerName + "Controller");
            mockType.Setup(t => t.FullName).Returns("Full" + controllerName + "Controller");
            mockType.Setup(t => t.GetCustomAttributes(typeof(HttpControllerConfigurationAttribute), It.IsAny<bool>()))
                .Returns(new HttpControllerConfigurationAttribute[0]);
            return mockType.Object;
        }
    }
}
