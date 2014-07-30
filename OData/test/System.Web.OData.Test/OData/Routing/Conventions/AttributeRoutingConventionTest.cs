// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
{
    public class AttributeRoutingConventionTest
    {
        [Fact]
        public void CtorTakingModelAndConfiguration_ThrowsArgumentNull_Model()
        {
            HttpConfiguration configuration = new HttpConfiguration();

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: null, configuration: configuration),
                "model");
        }

        [Fact]
        public void CtorTakingModelAndConfiguration_ThrowsArgumentNull_Configuration()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: model, configuration: null),
                "configuration");
        }

        [Fact]
        public void CtorTakingModelAndConfigurationAndPathHandler_ThrowsArgumentNull_Model()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            IODataPathTemplateHandler oDataPathTemplateHandler = new Mock<IODataPathTemplateHandler>().Object;

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: null, configuration: configuration,
                    pathTemplateHandler: oDataPathTemplateHandler),
                "model");
        }

        [Fact]
        public void CtorTakingModelAndConfigurationAndPathHandler_ThrowsArgumentNull_Configuration()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;
            IODataPathTemplateHandler oDataPathTemplateHandler = new Mock<IODataPathTemplateHandler>().Object;

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: model, configuration: null,
                    pathTemplateHandler: oDataPathTemplateHandler),
                "configuration");
        }

        [Fact]
        public void CtorTakingModelAndConfigurationAndPathHandler_ThrowsArgumentNull_PathTemplateHandler()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;
            HttpConfiguration configuration = new HttpConfiguration();

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: model, configuration: configuration,
                    pathTemplateHandler: null),
                "pathTemplateHandler");
        }

        [Fact]
        public void CtorTakingModelAndControllers_ThrowsArgumentNull_Model()
        {
            IEnumerable<HttpControllerDescriptor> controllers = new HttpControllerDescriptor[0];

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: null, controllers: controllers),
                "model");
        }

        [Fact]
        public void CtorTakingModelAndControllers_ThrowsArgumentNull_Controllers()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: model, controllers: null),
                "controllers");
        }

        [Fact]
        public void CtorTakingModelAndControllersAndPathHandler_ThrowsArgumentNull_Model()
        {
            IEnumerable<HttpControllerDescriptor> controllers = new HttpControllerDescriptor[0];
            IODataPathTemplateHandler oDataPathTemplateHandler = new Mock<IODataPathTemplateHandler>().Object;

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: null, controllers: controllers,
                    pathTemplateHandler: oDataPathTemplateHandler),
                "model");
        }

        [Fact]
        public void CtorTakingModelAndControllersAndPathHandler_ThrowsArgumentNull_Configuration()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;
            IODataPathTemplateHandler oDataPathTemplateHandler = new Mock<IODataPathTemplateHandler>().Object;

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: model, controllers: null,
                    pathTemplateHandler: oDataPathTemplateHandler),
                "controllers");
        }

        [Fact]
        public void CtorTakingModelAndControllersAndPathHandler_ThrowsArgumentNull_PathTemplateHandler()
        {
            IEdmModel model = new Mock<IEdmModel>().Object;
            IEnumerable<HttpControllerDescriptor> controllers = new HttpControllerDescriptor[0];

            Assert.ThrowsArgumentNull(
                () => new AttributeRoutingConvention(model: model, controllers: controllers,
                    pathTemplateHandler: null),
                "pathTemplateHandler");
        }

        [Fact]
        public void CtorTakingHttpConfiguration_InitializesAttributeMappings_OnFirstSelectControllerCall()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            ODataPathTemplate pathTemplate = new ODataPathTemplate();
            Mock<IODataPathTemplateHandler> pathTemplateHandler = new Mock<IODataPathTemplateHandler>();
            pathTemplateHandler.Setup(p => p.ParseTemplate(model.Model, "Customers")).Returns(pathTemplate).Verifiable();

            AttributeRoutingConvention convention = new AttributeRoutingConvention(model.Model, config, pathTemplateHandler.Object);
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
        public void AttributeMappingsIsInitialized_WithRightActionAndTemplate(Type controllerType,
            string expectedPathTemplate, string expectedActionName)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            HttpControllerDescriptor controller = new HttpControllerDescriptor(new HttpConfiguration(), "TestController",
                controllerType);

            ODataPathTemplate pathTemplate = new ODataPathTemplate();
            Mock<IODataPathTemplateHandler> pathTemplateHandler = new Mock<IODataPathTemplateHandler>();
            pathTemplateHandler
                .Setup(p => p.ParseTemplate(model.Model, expectedPathTemplate))
                .Returns(pathTemplate)
                .Verifiable();

            AttributeRoutingConvention convention = new AttributeRoutingConvention(model.Model,
                new[] { controller }, pathTemplateHandler.Object);

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
            HttpControllerDescriptor controller = new HttpControllerDescriptor(new HttpConfiguration(), "TestController",
                typeof(InvalidPathTemplateController));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => new AttributeRoutingConvention(model.Model, new[] { controller }),
                "The path template 'Customers/Order' on the action 'GetCustomers' in controller 'TestController' is not " +
                "a valid OData path template. The request URI is not valid. Since the segment 'Customers' refers to a " +
                "collection, this must be the last segment in the request URI or it must be followed by an function or " +
                "action that can be bound to it otherwise all intermediate segments must refer to a single resource.");
        }

        [Fact]
        public void AttributeMappingsInitialization_ThrowsInvalidOperation_IfNoConfigEnsureInitialized()
        {
            // Arrange
            HttpConfiguration configuration = new HttpConfiguration();
            IEdmModel model = Mock.Of<IEdmModel>();
            AttributeRoutingConvention convention = new AttributeRoutingConvention(model, configuration);

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
            IEdmModel model = new EdmModel();
            HttpConfiguration configuration = new[] { typeof(TestODataController) }.GetHttpConfiguration();
            AttributeRoutingConvention convention = new AttributeRoutingConvention(model, configuration);

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
            AttributeRoutingConvention convention = new AttributeRoutingConvention(model, configuration);

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
