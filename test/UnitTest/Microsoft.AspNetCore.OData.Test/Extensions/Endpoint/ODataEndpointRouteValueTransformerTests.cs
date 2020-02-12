// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCOREAPP2_0
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    public class ODataEndpointRouteValueTransformerTests
    {
        private IServiceProvider Services { get; }
        private IEndpointRouteBuilder Builder { get; }
        private IList<ActionDescriptor> Actions { get; }

        public ODataEndpointRouteValueTransformerTests()
        {
            Actions = new List<ActionDescriptor>
            {
                new ControllerActionDescriptor
                {
                    AttributeRouteInfo = new AttributeRouteInfo()
                    {
                        Template = "/test",
                    },
                    RouteValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "action", "Test" },
                        { "controller", "Test" },
                    },
                },
            };

        //    Builder = EndpointRouteBuilderFactory.Create();
        }

        [Fact]
        public void ODataEndpointRouteValueTransformerrowsIfRouteNameIsNull()
        {
            IEdmModel model = GetEdmModel();

            IEndpointRouteBuilder builder = EndpointRouteBuilderFactory.CreateWithRootContainer("odata",
                c => c.AddService(ServiceLifetime.Singleton, b => model));

            // Arrange & Act
            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = builder.ServiceProvider
            };

            RouteContext routeContext = new RouteContext(httpContext);
            ActionDescriptor actionDescriptor = new ControllerActionDescriptor();
            IReadOnlyList<ActionDescriptor> candidates = new List<ActionDescriptor>();

            Mock<IActionSelector> selector = new Mock<IActionSelector>();
            selector.Setup(e => e.SelectCandidates(routeContext)).Returns(candidates);
            selector.Setup(e => e.SelectBestCandidate(routeContext, candidates)).Returns(actionDescriptor);

            ODataEndpointRouteValueTransformer transformer = new ODataEndpointRouteValueTransformer(selector.Object);

            // Assert
            RouteValueDictionary values = new RouteValueDictionary();
            values.Add("ODataEndpointPath_odata", "Customers(1)");

            var results = transformer.TransformAsync(httpContext, values);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public class Customer
        {
            public int Id { get; set; }
        }

        [Theory]
        [InlineData("odata", "", "{**ODataEndpointPath_odata}")]
        [InlineData("odata", null, "{**ODataEndpointPath_odata}")]
        [InlineData("odata", "myPre", "myPre/{**ODataEndpointPath_odata}")]
        [InlineData("otherName", "myPre/{abc}", "myPre/{abc}/{**ODataEndpointPath_otherName}")]
        public void CreateODataEndpointPatternWorksAsExpected(string name, string prefix, string expected)
        {
            // Arrange & Act
            string actual = ODataEndpointPattern.CreateODataEndpointPattern(name, prefix);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("ODataEndpointPath_odata", "path", "odata", "path")]
        [InlineData("ODataEndpointPath_otherName", "path1", "otherName", "path1")]
        [InlineData("ODataendpointPath_odata", "path", null, null)] // be noted there's a lower case of "endpoint"
        [InlineData("anything", "anyPath", null, null)]
        public void GetODataRouteInfoWorksAsExpected(string routeKey, string routeValue, string routeName, string pathValue)
        {
            // Arrange
            RouteValueDictionary values = new RouteValueDictionary();
            values.Add(routeKey, routeValue);

            // Act
            (string actualName, object actualPath) = values.GetODataRouteInfo();

            // Assert
            Assert.Equal(routeName, actualName);
            Assert.Equal(pathValue, actualPath);
        }
    }
}

#endif