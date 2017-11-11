// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
using Moq;

namespace Microsoft.Test.AspNet.OData.Routing.Conventions
{
    public class AttributeRoutingConventionTest
    {
        private static readonly string RouteName = Microsoft.Test.AspNet.OData.Formatter.HttpRouteCollectionExtensions.RouteName;

        [Fact]
        public void CtorTakingModelAndConfiguration_ThrowsArgumentNull_Configuration()
        {
            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(routeName: RouteName, configuration: null),
                "configuration");
        }

        [Fact]
        public void CtorTakingModelAndConfiguration_ThrowsArgumentNull_RouteName()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(routeName: null, configuration: configuration),
                "routeName");
        }

        [Fact]
        public void CtorTakingModelAndConfigurationAndPathHandler_ThrowsArgumentNull_Configuration()
        {
            IODataPathTemplateHandler oDataPathTemplateHandler = new Mock<IODataPathTemplateHandler>().Object;

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(configuration: null,
                    routeName: RouteName, pathTemplateHandler: oDataPathTemplateHandler),
                "configuration");
        }

        [Fact]
        public void CtorTakingModelAndConfigurationAndPathHandler_ThrowsArgumentNull_PathTemplateHandler()
        {
            HttpConfiguration configuration = DependencyInjectionHelper.CreateConfigurationWithRootContainer();

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(configuration: configuration,
                    routeName: RouteName, pathTemplateHandler: null),
                "pathTemplateHandler");
        }

        [Fact]
        public void CtorTakingModelAndControllers_ThrowsArgumentNull_Controllers()
        {
            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(routeName: RouteName, controllers: null),
                "controllers");
        }

        [Fact]
        public void CtorTakingModelAndControllersAndPathHandler_ThrowsArgumentNull_Configuration()
        {
            IODataPathTemplateHandler oDataPathTemplateHandler = new Mock<IODataPathTemplateHandler>().Object;

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(controllers: null,
                    routeName: RouteName, pathTemplateHandler: oDataPathTemplateHandler),
                "controllers");
        }

        [Fact]
        public void CtorTakingModelAndControllersAndPathHandler_ThrowsArgumentNull_PathTemplateHandler()
        {
            IEnumerable<HttpControllerDescriptor> controllers = new HttpControllerDescriptor[0];

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(controllers: controllers,
                    routeName: RouteName, pathTemplateHandler: null),
                "pathTemplateHandler");
        }

        [Fact]
        public void CtorTakingHttpConfiguration_InitializesAttributeMappings_OnFirstSelectControllerCall()
        {
            // Arrange
            HttpConfiguration config = DependencyInjectionHelper.CreateConfigurationWithRootContainer();

            ODataPathTemplate pathTemplate = new ODataPathTemplate();
            Mock<IODataPathTemplateHandler> pathTemplateHandler = new Mock<IODataPathTemplateHandler>();
            pathTemplateHandler.Setup(p => p.ParseTemplate("Customers", config.GetODataRootContainer(RouteName)))
                .Returns(pathTemplate).Verifiable();

            AttributeRoutingConvention convention = new AttributeRoutingConvention(RouteName, config, pathTemplateHandler.Object);
            config.EnsureInitialized();

            // Act
            convention.SelectController(new ODataPath(), new HttpRequestMessage());

            // Assert
            pathTemplateHandler.VerifyAll();
            Assert.NotNull(convention.AttributeMappings);
            Assert.Equal("GetCustomers", convention.AttributeMappings[pathTemplate].ActionName);
        }

        [Theory]
        [InlineData(typeof(TestODataController), "Customers", "GetCustomers")]
        [InlineData(typeof(TestODataController), "Customers({key})/Orders", "GetOrdersOfACustomer")]
        [InlineData(typeof(TestODataController), "Customers({key})", "GetCustomer")]
        [InlineData(typeof(TestODataController), "VipCustomer", "GetVipCustomer")] // Singleton
        [InlineData(typeof(TestODataController), "VipCustomer/Orders", "GetOrdersOfVipCustomer")] // Singleton/Navigation
        [InlineData(typeof(TestODataControllerWithPrefix), "Customers", "GetCustomers")]
        [InlineData(typeof(TestODataControllerWithPrefix), "Customers({key})/Orders", "GetOrdersOfACustomer")]
        [InlineData(typeof(TestODataControllerWithPrefix), "Customers({key})", "GetCustomer")]
        [InlineData(typeof(SingletonTestControllerWithPrefix), "VipCustomer", "GetVipCustomerWithPrefix")] // Singleton
        [InlineData(typeof(SingletonTestControllerWithPrefix), "VipCustomer/Name", "GetVipCustomerNameWithPrefix")] // Singleton/property
        [InlineData(typeof(SingletonTestControllerWithPrefix), "VipCustomer/Orders", "GetVipCustomerOrdersWithPrefix")] // Singleton/Navigation
        [InlineData(typeof(TestODataControllerWithMultiplePrefixes), "Customers({key})", "GetCustomer")]
        [InlineData(typeof(TestODataControllerWithMultiplePrefixes), "Customers({key})/Orders", "GetOrdersOfACustomer")]
        [InlineData(typeof(TestODataControllerWithMultiplePrefixes), "VipCustomer", "GetCustomer")]
        [InlineData(typeof(TestODataControllerWithMultiplePrefixes), "VipCustomer/Orders", "GetOrdersOfACustomer")]
        public void AttributeMappingsIsInitialized_WithRightActionAndTemplate(Type controllerType,
            string expectedPathTemplate, string expectedActionName)
        {
            // Arrange
            HttpControllerDescriptor controller = new HttpControllerDescriptor(DependencyInjectionHelper.CreateConfigurationWithRootContainer(), "TestController",
                controllerType);

            ODataPathTemplate pathTemplate = new ODataPathTemplate();
            Mock<IODataPathTemplateHandler> pathTemplateHandler = new Mock<IODataPathTemplateHandler>();
            pathTemplateHandler
                .Setup(p => p.ParseTemplate(expectedPathTemplate, controller.Configuration.GetODataRootContainer(RouteName)))
                .Returns(pathTemplate)
                .Verifiable();

            AttributeRoutingConvention convention = new AttributeRoutingConvention(RouteName, new[] { controller }, pathTemplateHandler.Object);

            // Act
            convention.SelectController(new ODataPath(), new HttpRequestMessage());

            // Assert
            pathTemplateHandler.VerifyAll();
            Assert.NotNull(convention.AttributeMappings);
            Assert.Equal(expectedActionName, convention.AttributeMappings[pathTemplate].ActionName);
        }

        [Fact]
        public void Constructor_ThrowsInvalidOperation_IfFailsToParsePathTemplate()
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            HttpControllerDescriptor controller = new HttpControllerDescriptor(DependencyInjectionHelper.CreateConfigurationWithRootContainer(model.Model),
                "TestController", typeof(InvalidPathTemplateController));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => new AttributeRoutingConvention(RouteName, new[] { controller }, new DefaultODataPathHandler()),
                "The path template 'Customers/Order' on the action 'GetCustomers' in controller 'TestController' is not " +
                "a valid OData path template. The request URI is not valid. Since the segment 'Customers' refers to a " +
                "collection, this must be the last segment in the request URI or it must be followed by an function or " +
                "action that can be bound to it otherwise all intermediate segments must refer to a single resource.");
        }

        [Fact]
        public void AttributeMappingsInitialization_ThrowsInvalidOperation_IfNoConfigEnsureInitialized()
        {
            // Arrange
            HttpConfiguration configuration = DependencyInjectionHelper.CreateConfigurationWithRootContainer();
            AttributeRoutingConvention convention = new AttributeRoutingConvention(RouteName, configuration);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => convention.AttributeMappings,
                "The object has not yet been initialized. Ensure that HttpConfiguration.EnsureInitialized() is called " +
                "in the application's startup code after all other initialization code.");
        }

        [Fact]
        public void AttributeRoutingConvention_ConfigEnsureInitialized_ThrowsForInvalidPathTemplate()
        {
            // Arrange
            HttpConfiguration configuration = new[] { typeof(TestODataController) }.GetHttpConfiguration();
            configuration.EnableODataDependencyInjectionSupport();
            AttributeRoutingConvention convention = new AttributeRoutingConvention(RouteName, configuration);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => configuration.EnsureInitialized(),
                "The path template 'Customers' on the action 'GetCustomers' in controller 'TestOData' is not a valid OData path template. " +
                "The operation import overloads matching 'Customers' are invalid. This is most likely an error in the IEdmModel.");
        }

        [Fact]
        public void AttributeRoutingConvention_ConfigEnsureInitialized_DoesNotThrowForValidPathTemplate()
        {
            // Arrange
            IEdmModel model = new CustomersModelWithInheritance().Model;
            HttpConfiguration configuration = new[] { typeof(TestODataController) }.GetHttpConfiguration();
            configuration.EnableODataDependencyInjectionSupport(model);
            AttributeRoutingConvention convention = new AttributeRoutingConvention(RouteName, configuration);

            // Act & Assert
            Assert.DoesNotThrow(() => configuration.EnsureInitialized());
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
    }
}
