// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
using static Microsoft.AspNet.OData.Test.Routing.AttributeRoutingTest;
#else
using System;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class AttributeRoutingConventionTest
    {
        private static readonly string RouteName = Abstraction.HttpRouteCollectionExtensions.RouteName;

        [Fact]
        public void CtorTakingModelAndConfiguration_ThrowsArgumentNull_Configuration()
        {
#if NETCORE
            ExceptionAssert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(routeName: RouteName, serviceProvider: null),
                "serviceProvider");
#else
            ExceptionAssert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(routeName: RouteName, configuration: null),
                "configuration");
#endif
        }

        [Fact]
        public void CtorTakingModelAndConfiguration_ThrowsArgumentNull_RouteName()
        {
            var configuration = RoutingConfigurationFactory.Create();

            ExceptionAssert.ThrowsArgumentNull(
                () => CreateAttributeRoutingConvention(null, configuration),
                "routeName");
        }

#if NETFX // Only needed for AspNet
        [Fact]
        public void CtorTakingModelAndConfigurationAndPathHandler_ThrowsArgumentNull_Configuration()
        {
            IODataPathTemplateHandler oDataPathTemplateHandler = new Mock<IODataPathTemplateHandler>().Object;

            ExceptionAssert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(configuration: null,
                    routeName: RouteName, pathTemplateHandler: oDataPathTemplateHandler),
                "configuration");
        }

        [Fact]
        public void CtorTakingModelAndConfigurationAndPathHandler_ThrowsArgumentNull_PathTemplateHandler()
        {
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer("OData");

            ExceptionAssert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(configuration: configuration,
                    routeName: RouteName, pathTemplateHandler: null),
                "pathTemplateHandler");
        }
#endif

        [Fact]
        public void CtorTakingModelAndControllers_ThrowsArgumentNull_Controllers()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(routeName: RouteName, controllers: null),
                "controllers");
        }

        [Fact]
        public void CtorTakingModelAndControllersAndPathHandler_ThrowsArgumentNull_Configuration()
        {
            IODataPathTemplateHandler oDataPathTemplateHandler = new Mock<IODataPathTemplateHandler>().Object;

            ExceptionAssert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(controllers: null,
                    routeName: RouteName, pathTemplateHandler: oDataPathTemplateHandler),
                "controllers");
        }

        [Fact]
        public void CtorTakingModelAndControllersAndPathHandler_ThrowsArgumentNull_PathTemplateHandler()
        {
            var controllers = ControllerDescriptorFactory.CreateCollection();

            ExceptionAssert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(controllers: controllers,
                    routeName: RouteName, pathTemplateHandler: null),
                "pathTemplateHandler");
        }

        [Fact]
        public void CtorTakingHttpConfiguration_InitializesAttributeMappings_OnFirstSelectControllerCall()
        {
            // Arrange
            var config = RoutingConfigurationFactory.CreateWithRootContainer(RouteName);
            var serviceProvider = GetServiceProvider(config, RouteName);
            var request = RequestFactory.Create(config, RouteName);

#if NETCORE
            request.ODataFeature().Path = new ODataPath();
            request.Method = "Get";
            ControllerDescriptorFactory.Create(config, "MetadataAndService", typeof(MetadataAndServiceController));
#endif

            ODataPathTemplate pathTemplate = new ODataPathTemplate();
            Mock<IODataPathTemplateHandler> pathTemplateHandler = new Mock<IODataPathTemplateHandler>();
            pathTemplateHandler.Setup(p => p.ParseTemplate("$metadata", serviceProvider))
                .Returns(pathTemplate).Verifiable();

            AttributeRoutingConvention convention = CreateAttributeRoutingConvention(RouteName, config, pathTemplateHandler.Object);
            EnsureAttributeMapping(convention, config);

            // Act
            Select(convention, request);

            // Assert
            pathTemplateHandler.VerifyAll();
            Assert.NotNull(convention.AttributeMappings);
            Assert.Equal("GetMetadata", convention.AttributeMappings[pathTemplate].ActionName);
        }

        [Theory]
        [InlineData(typeof(TestODataController), "Get", "Customers", "GetCustomers")]
        [InlineData(typeof(TestODataController), "Get", "Customers({key})/Orders", "GetOrdersOfACustomer")]
        [InlineData(typeof(TestODataController), "Get", "Customers({key})", "GetCustomer")]
        [InlineData(typeof(TestODataController), "Head", "Customers({key})", "GetCustomer")]
        [InlineData(typeof(TestODataController), "Get", "VipCustomer", "GetVipCustomer")] // Singleton
        [InlineData(typeof(TestODataController), "Get", "VipCustomer/Orders", "GetOrdersOfVipCustomer")] // Singleton/Navigation
        [InlineData(typeof(TestODataControllerWithPrefix), "Get", "Customers", "GetCustomers")]
        [InlineData(typeof(TestODataControllerWithPrefix), "Get", "Customers({key})/Orders", "GetOrdersOfACustomer")]
        [InlineData(typeof(TestODataControllerWithPrefix), "Get", "Customers({key})", "GetCustomer")]
        [InlineData(typeof(SingletonTestControllerWithPrefix), "Get", "VipCustomer", "GetVipCustomerWithPrefix")] // Singleton
        [InlineData(typeof(SingletonTestControllerWithPrefix), "Get", "VipCustomer/Name", "GetVipCustomerNameWithPrefix")] // Singleton/property
        [InlineData(typeof(SingletonTestControllerWithPrefix), "Get", "VipCustomer/Orders", "GetVipCustomerOrdersWithPrefix")] // Singleton/Navigation
        [InlineData(typeof(TestODataControllerWithMultiplePrefixes), "Get", "Customers({key})", "GetCustomer")]
        [InlineData(typeof(TestODataControllerWithMultiplePrefixes), "Get", "Customers({key})/Orders", "GetOrdersOfACustomer")]
        [InlineData(typeof(TestODataControllerWithMultiplePrefixes), "Get", "VipCustomer", "GetCustomer")]
        [InlineData(typeof(TestODataControllerWithMultiplePrefixes), "Get", "VipCustomer/Orders", "GetOrdersOfACustomer")]
        public void AttributeMappingsIsInitialized_WithRightActionAndTemplate(
            Type controllerType,
            string method,
            string expectedPathTemplate,
            string expectedActionName)
        {
            // Arrange
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer(RouteName);
            var serviceProvider = GetServiceProvider(configuration, RouteName);
            var request = RequestFactory.Create(configuration, RouteName);
#if NETCORE
            request.ODataFeature().Path = new ODataPath();
            request.Method = method;
#else
            request.Method = new HttpMethod(method);
#endif

            var descriptors = ControllerDescriptorFactory.Create(configuration, "TestController", 
                controllerType);

            ODataPathTemplate pathTemplate = new ODataPathTemplate();
            Mock<IODataPathTemplateHandler> pathTemplateHandler = new Mock<IODataPathTemplateHandler>();
            pathTemplateHandler
                .Setup(p => p.ParseTemplate(expectedPathTemplate, serviceProvider))
                .Returns(pathTemplate)
                .Verifiable();

            AttributeRoutingConvention convention = new AttributeRoutingConvention(RouteName, descriptors, pathTemplateHandler.Object);

            // Act
            Select(convention, request);

            // Assert
            pathTemplateHandler.VerifyAll();
            Assert.NotNull(convention.AttributeMappings);
            Assert.Equal(expectedActionName, convention.AttributeMappings[pathTemplate].ActionName);
        }

        [Fact]
        public void Constructor_ThrowsInvalidOperation_IfFailsToParsePathTemplate()
        {
            // Arrange
            IEdmModel model = new CustomersModelWithInheritance().Model;
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer(RouteName,
                (b => b.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model)));

            var descriptors = ControllerDescriptorFactory.Create(configuration,
                "TestController", typeof(InvalidPathTemplateController));

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => new AttributeRoutingConvention(RouteName, descriptors, new DefaultODataPathHandler()),
                "The path template 'Customers/Order' on the action 'GetCustomers' in controller 'TestController' is not " +
                "a valid OData path template. Bad Request - Error in query syntax.");
        }

#if NETFX // AspNetCore version uses lazy initialization.
        [Fact]
        public void AttributeMappingsInitialization_ThrowsInvalidOperation_IfNoConfigEnsureInitialized()
        {
            // Arrange
            var configuration = RoutingConfigurationFactory.CreateWithRootContainer(RouteName);
            AttributeRoutingConvention convention = CreateAttributeRoutingConvention(RouteName, configuration);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => convention.AttributeMappings,
                "The object has not yet been initialized. Ensure that HttpConfiguration.EnsureInitialized() is called " +
                "in the application's startup code after all other initialization code.");
        }
#endif

        [Fact]
        public void AttributeRoutingConvention_ConfigEnsureInitialized_ThrowsForInvalidPathTemplate()
        {
            // Arrange
            var configuration = RoutingConfigurationFactory.CreateWithRootContainerAndTypes(RouteName, null, typeof(TestODataController));
#if NETCORE
            ControllerDescriptorFactory.Create(configuration, "TestOData", typeof(TestODataController));
#endif

            AttributeRoutingConvention convention = CreateAttributeRoutingConvention(RouteName, configuration);

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => EnsureAttributeMapping(convention, configuration),
                "The path template 'Customers' on the action 'GetCustomers' in controller 'TestOData' is not a valid OData path template. " +
                "Resource not found for the segment 'Customers'.");
        }

        [Fact]
        public void AttributeRoutingConvention_ConfigEnsureInitialized_DoesNotThrowForValidPathTemplate()
        {
            // Arrange
            IEdmModel model = new CustomersModelWithInheritance().Model;
            var configuration = RoutingConfigurationFactory.CreateWithRootContainerAndTypes(
                RouteName,
                (b => b.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model)),
                typeof(TestODataController));

            AttributeRoutingConvention convention = CreateAttributeRoutingConvention(RouteName, configuration);

            // Act & Assert
            ExceptionAssert.DoesNotThrow(() => EnsureAttributeMapping(convention, configuration));
        }

        public class TestODataController : ODataController
        {
            [ODataRoute("Customers")]
            public void GetCustomers()
            {
            }

            [ODataRoute("Customers({key})/Orders")]
            public void GetOrdersOfACustomer()
            {
            }

            [HttpGet,HttpHead]
            [ODataRoute("Customers({key})")]
            public void GetCustomer()
            {
            }

            // Singleton
            [ODataRoute("VipCustomer")]
            public void GetVipCustomer()
            {
            }

            // Singleton/navigation property
            [ODataRoute("VipCustomer/Orders")]
            public void GetOrdersOfVipCustomer()
            {
            }
        }

        [ODataRoutePrefix("Customers({key})")]
        public class TestODataControllerWithPrefix : ODataController
        {
            [ODataRoute("Orders")]
            public void GetOrdersOfACustomer()
            {
            }

            [ODataRoute("")]
            public void GetCustomer()
            {
            }

            [ODataRoute("/Customers")]
            public void GetCustomers()
            {
            }
        }

        [ODataRoutePrefix("Customers({key})")]
        [ODataRoutePrefix("VipCustomer")]
        public class TestODataControllerWithMultiplePrefixes : ODataController
        {
            [ODataRoute("Orders")]
            public void GetOrdersOfACustomer()
            {
            }

            [ODataRoute("")]
            public void GetCustomer()
            {
            }
        }

        [ODataRoutePrefix("VipCustomer")]
        public class SingletonTestControllerWithPrefix : ODataController
        {
            [ODataRoute("Orders")]
            public void GetVipCustomerOrdersWithPrefix()
            {
            }

            [ODataRoute("")]
            public void GetVipCustomerWithPrefix()
            {
            }

            [ODataRoute("Name")]
            public void GetVipCustomerNameWithPrefix()
            {
            }
        }

        public class InvalidPathTemplateController : ODataController
        {
            [ODataRoute("Customers/Order")]
            public void GetCustomers()
            {
            }
        }

#if NETCORE
        private AttributeRoutingConvention CreateAttributeRoutingConvention(
            string routeName,
            IRouteBuilder routeBuilder,
            IODataPathTemplateHandler pathTemplateHandler = null)
        {
            if (pathTemplateHandler == null)
            {
                return new AttributeRoutingConvention(routeName: routeName, serviceProvider: routeBuilder.ServiceProvider);
            }

            return new AttributeRoutingConvention(routeName: routeName, serviceProvider: routeBuilder.ServiceProvider, pathTemplateHandler: pathTemplateHandler);
        }

        private IServiceProvider GetServiceProvider(IRouteBuilder routeBuilder, string routeName)
        {
            IPerRouteContainer perRouteContainer = routeBuilder.ServiceProvider.GetRequiredService<IPerRouteContainer>();
            return perRouteContainer.GetODataRootContainer(routeName);
        }

        private void EnsureAttributeMapping(AttributeRoutingConvention convention, IRouteBuilder routeBuilder)
        {
            var mappings = convention.AttributeMappings;
        }

        private ControllerActionDescriptor Select(AttributeRoutingConvention convention, HttpRequest request)
        {
            RouteContext routeContext = new RouteContext(request.HttpContext);
            return convention.SelectAction(routeContext)?.FirstOrDefault();
        }
#else
        private AttributeRoutingConvention CreateAttributeRoutingConvention(
            string routeName,
            HttpConfiguration configuration,
            IODataPathTemplateHandler pathTemplateHandler = null)
        {
            if (pathTemplateHandler == null)
            {
                return new AttributeRoutingConvention(routeName: routeName, configuration: configuration);
            }

            return new AttributeRoutingConvention(routeName: routeName, configuration: configuration, pathTemplateHandler: pathTemplateHandler);
        }

        private IServiceProvider GetServiceProvider(HttpConfiguration configuration, string routeName)
        {
            return configuration.GetODataRootContainer(routeName);
        }

        private void EnsureAttributeMapping(AttributeRoutingConvention convention, HttpConfiguration configuration)
        {
            configuration.EnsureInitialized();
        }

        private string Select(AttributeRoutingConvention convention, HttpRequestMessage request)
        {
            return convention.SelectController(new ODataPath(), request);
        }
#endif
    }
}
