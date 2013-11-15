// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Routing
{
    public class DirectRouteBuilderTests
    {
        [Fact]
        public void CreateRoute_ValidatesConstraintType_IHttpRouteConstraint()
        {
            // Arrange
            var actions = new List<HttpActionDescriptor>()
            {
                new Mock<HttpActionDescriptor>().Object,
            };

            var builder = new DirectRouteBuilder(actions.AsReadOnly());

            var constraint = new CustomConstraint();
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);
            builder.Constraints = constraints;

            // Act
            var routeEntry = builder.Build();

            // Assert
            Assert.NotNull(routeEntry.Route.Constraints["custom"]);
        }

        [Fact]
        public void BuildRoute_ValidatesConstraintType_StringRegex()
        {
            // Arrange
            var actions = new List<HttpActionDescriptor>()
            {
                new Mock<HttpActionDescriptor>().Object,
            };

            var builder = new DirectRouteBuilder(actions.AsReadOnly());

            var constraint = "product|products";
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);
            builder.Constraints = constraints;

            // Act
            var routeEntry = builder.Build();

            // Assert
            Assert.NotNull(routeEntry.Route.Constraints["custom"]);
        }

        [Fact]
        public void CreateRoute_ValidatesConstraintType_InvalidType()
        {
            // Arrange
            var actions = new List<HttpActionDescriptor>()
            {
                new Mock<HttpActionDescriptor>().Object,
            };

            var builder = new DirectRouteBuilder(actions.AsReadOnly());

            var constraint = new Uri("http://localhost/");
            var constraints = new HttpRouteValueDictionary();
            constraints.Add("custom", constraint);

            builder.Constraints = constraints;
            builder.Template = "{controller}/{id}";

            string expectedMessage =
                "The constraint entry 'custom' on the route with route template '{controller}/{id}' " +
                "must have a string value or be of a type which implements 'IHttpRouteConstraint'.";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => builder.Build(), expectedMessage);
        }

        private class CustomConstraint : IHttpRouteConstraint
        {
            public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
            {
                throw new NotImplementedException();
            }
        }
    }
}
