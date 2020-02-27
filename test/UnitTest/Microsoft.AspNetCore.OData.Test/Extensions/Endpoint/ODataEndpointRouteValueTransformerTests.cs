// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCOREAPP2_0
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    public class ODataEndpointRouteValueTransformerTests
    {
        [Fact]
        public void TransformAsyncThrowsIfHttpContextIsNull()
        {
            // Arrange & Act
            IActionSelector selector = new Mock<IActionSelector>().Object;
            ODataEndpointRouteValueTransformer transformer = new ODataEndpointRouteValueTransformer(selector);
            Action test = () => transformer.TransformAsync(httpContext: null, values: null);

            // Assert
            ExceptionAssert.ThrowsArgumentNull(test, "httpContext");
        }

        [Fact]
        public void TransformAsyncThrowsIfRouteValuesIsNull()
        {
            // Arrange & Act
            IActionSelector selector = new Mock<IActionSelector>().Object;
            ODataEndpointRouteValueTransformer transformer = new ODataEndpointRouteValueTransformer(selector);
            Action test = () => transformer.TransformAsync(httpContext: new DefaultHttpContext(), values: null);

            // Assert
            ExceptionAssert.ThrowsArgumentNull(test, "values");
        }

        [Fact]
        public void TransformAsyncReturnsEmptyIfODataPathSet()
        {
            // Arrange & Act
            IActionSelector selector = new Mock<IActionSelector>().Object;
            HttpContext context = new DefaultHttpContext();
            context.ODataFeature().Path = new OData.Routing.ODataPath();

            ODataEndpointRouteValueTransformer transformer = new ODataEndpointRouteValueTransformer(selector);
            ValueTask<RouteValueDictionary> actual = transformer.TransformAsync(context, new RouteValueDictionary());

            // Assert
            Assert.Null(actual.Result);
        }

        [Fact]
        public void TransformAsyncReturnsCorrectRouteValues()
        {
            // Arrange
            IEndpointRouteBuilder builder = EndpointRouteBuilderFactory.Create("odata",
                c => c.AddService(ServiceLifetime.Singleton, b => GetEdmModel()));

            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = builder.ServiceProvider
            };

            HttpRequest request = SetHttpRequest(httpContext, HttpMethod.Get, "http://localhost:123/Customers(1)");
            RouteValueDictionary values = new RouteValueDictionary();
            values.Add("ODataEndpointPath_odata", "Customers(1)");

            ActionDescriptor actionDescriptor = new ControllerActionDescriptor
            {
                ControllerName = "Customers",
                ActionName = "Get"
            };
            IActionSelector actionSelector = new MockActionSelector(actionDescriptor);

            // Act
            ODataEndpointRouteValueTransformer transformer = new ODataEndpointRouteValueTransformer(actionSelector);
            ValueTask<RouteValueDictionary> actual = transformer.TransformAsync(httpContext, values);

            // Assert
            Assert.NotNull(actual.Result);
            RouteValueDictionary routeValues = actual.Result;

            Assert.Equal(4, routeValues.Count);
            Assert.Equal("Customers(1)", routeValues["ODataEndpointPath_odata"]);
            Assert.Equal("Customers", routeValues["controller"]);
            Assert.Equal("Get", routeValues["action"]);
            Assert.Equal("Customers(1)", routeValues["odataPath"]);

            Assert.NotNull(httpContext.ODataFeature().Path);
            Assert.Same(actionDescriptor, httpContext.ODataFeature().ActionDescriptor);
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

        private static HttpRequest SetHttpRequest(HttpContext context, HttpMethod method, string uri)
        {
            HttpRequest request = context.Request;
            request.Method = method.ToString();

            Uri requestUri = new Uri(uri);
            request.Scheme = requestUri.Scheme;
            request.Host = requestUri.IsDefaultPort ?
                new HostString(requestUri.Host) :
                new HostString(requestUri.Host, requestUri.Port);
            request.QueryString = new QueryString(requestUri.Query);
            request.Path = new PathString(requestUri.AbsolutePath);
            return request;
        }
    }

    public class MockActionSelector : IActionSelector
    {
        private ActionDescriptor _actionDescriptor;

        public MockActionSelector(ActionDescriptor actionDescriptor)
        {
            _actionDescriptor = actionDescriptor;
        }

        public IReadOnlyList<ActionDescriptor> SelectCandidates(RouteContext context)
        {
            return new List<ActionDescriptor>();
        }

        public ActionDescriptor SelectBestCandidate(RouteContext context, IReadOnlyList<ActionDescriptor> candidates)
        {
            return _actionDescriptor;
        }
    }
}
#endif