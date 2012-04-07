// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class RemoteAttributeTest
    {
        // Good route name, bad route name
        // Controller + Action

        [Fact]
        public void GuardClauses()
        {
            // Act & Assert
            Assert.ThrowsArgumentNullOrEmpty(
                () => new RemoteAttribute(null, "controller"),
                "action");
            Assert.ThrowsArgumentNullOrEmpty(
                () => new RemoteAttribute("action", null),
                "controller");
            Assert.ThrowsArgumentNullOrEmpty(
                () => new RemoteAttribute(null),
                "routeName");
            Assert.ThrowsArgumentNullOrEmpty(
                () => RemoteAttribute.FormatPropertyForClientValidation(String.Empty),
                "property");
            Assert.ThrowsArgumentNullOrEmpty(
                () => new RemoteAttribute("foo").FormatAdditionalFieldsForClientValidation(String.Empty),
                "property");
        }

        [Fact]
        public void IsValidAlwaysReturnsTrue()
        {
            // Act & Assert
            Assert.True(new RemoteAttribute("RouteName", "ParameterName").IsValid(null));
            Assert.True(new RemoteAttribute("ActionName", "ControllerName", "ParameterName").IsValid(null));
        }

        [Fact]
        public void BadRouteNameThrows()
        {
            // Arrange
            ControllerContext context = new ControllerContext();
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(object));
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("RouteName");

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => new List<ModelClientValidationRule>(attribute.GetClientValidationRules(metadata, context)),
                "A route named 'RouteName' could not be found in the route collection.\r\nParameter name: name");
        }

        [Fact]
        public void NoRouteWithActionControllerThrows()
        {
            // Arrange
            ControllerContext context = new ControllerContext();
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(string), "Length");
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("Action", "Controller");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => new List<ModelClientValidationRule>(attribute.GetClientValidationRules(metadata, context)),
                "No url for remote validation could be found.");
        }

        [Fact]
        public void GoodRouteNameReturnsCorrectClientData()
        {
            // Arrange
            string url = null;
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(string), "Length");
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("RouteName");
            attribute.RouteTable.Add("RouteName", new Route("my/url", new MvcRouteHandler()));

            // Act
            ModelClientValidationRule rule = attribute.GetClientValidationRules(metadata, GetMockControllerContext(url)).Single();

            // Assert
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);
            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("/my/url", rule.ValidationParameters["url"]);
        }

        [Fact]
        public void ActionControllerReturnsCorrectClientDataWithoutNamedParameters()
        {
            // Arrange
            string url = null;

            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(string), "Length");
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("Action", "Controller");
            attribute.RouteTable.Add(new Route("{controller}/{action}", new MvcRouteHandler()));

            // Act
            ModelClientValidationRule rule = attribute.GetClientValidationRules(metadata, GetMockControllerContext(url)).Single();

            // Assert
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);
            Assert.Equal(2, rule.ValidationParameters.Count);
            Assert.Equal("/Controller/Action", rule.ValidationParameters["url"]);
            Assert.Equal("*.Length", rule.ValidationParameters["additionalfields"]);
            Assert.Throws<KeyNotFoundException>(
                () => rule.ValidationParameters["type"],
                "The given key was not present in the dictionary.");
        }

        [Fact]
        public void ActionControllerReturnsCorrectClientDataWithNamedParameters()
        {
            // Arrange
            string url = null;

            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, typeof(string), "Length");
            TestableRemoteAttribute attribute = new TestableRemoteAttribute("Action", "Controller");
            attribute.HttpMethod = "POST";
            attribute.AdditionalFields = "Password,ConfirmPassword";

            attribute.RouteTable.Add(new Route("{controller}/{action}", new MvcRouteHandler()));

            // Act
            ModelClientValidationRule rule = attribute.GetClientValidationRules(metadata, GetMockControllerContext(url)).Single();

            // Assert
            Assert.Equal("remote", rule.ValidationType);
            Assert.Equal("'Length' is invalid.", rule.ErrorMessage);
            Assert.Equal(3, rule.ValidationParameters.Count);
            Assert.Equal("/Controller/Action", rule.ValidationParameters["url"]);
            Assert.Equal("*.Length,*.Password,*.ConfirmPassword", rule.ValidationParameters["additionalfields"]);
            Assert.Equal("POST", rule.ValidationParameters["type"]);
        }

        private ControllerContext GetMockControllerContext(string url)
        {
            Mock<ControllerContext> context = new Mock<ControllerContext>();
            context.Setup(c => c.HttpContext.Request.ApplicationPath)
                .Returns("/");
            context.Setup(c => c.HttpContext.Response.ApplyAppPathModifier(It.IsAny<string>()))
                .Callback<string>(vpath => url = vpath)
                .Returns(() => url);

            return context.Object;
        }

        private class TestableRemoteAttribute : RemoteAttribute
        {
            public RouteCollection RouteTable = new RouteCollection();

            public TestableRemoteAttribute(string action, string controller)
                : base(action, controller)
            {
            }

            public TestableRemoteAttribute(string routeName)
                : base(routeName)
            {
            }

            protected override RouteCollection Routes
            {
                get { return RouteTable; }
            }
        }
    }
}
