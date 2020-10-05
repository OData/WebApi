// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCOREAPP2_0
using System;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    public class ODataEndpointLinkGeneratorTests
    {
        [Fact]
        public void ODataLinkGeneratorCotrThrowsIfLinkGeneratorIsNull()
        {
            // Arrange & Act
            Action test = () => new ODataEndpointLinkGenerator(null);

            // Assert
            ExceptionAssert.ThrowsArgumentNull(test, "generator");
        }

        [Fact]
        public void GetPathByAddressReturnsCorrectODataPath()
        {
            // Arrange
            int address = 1;
            IPerRouteContainer container = new PerRouteContainer();
            container.AddRoute("odata", "ms/{data}");

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<IPerRouteContainer>(container)
                .BuildServiceProvider();

            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };
            httpContext.ODataFeature().RouteName = "odata";
            RouteValueDictionary values = new RouteValueDictionary();
            values["odataPath"] = "";

            RouteValueDictionary ambientValues = new RouteValueDictionary();
            ambientValues["data"] = 2;

            // Act
            Mock<LinkGenerator> mock = new Mock<LinkGenerator>();
            ODataEndpointLinkGenerator linkGenerator = new ODataEndpointLinkGenerator(mock.Object);
            string path = linkGenerator.GetPathByAddress(httpContext, address, values, ambientValues);

            // Assert
            Assert.Equal("ms/2", path);
        }

        [Theory]
        [InlineData("/test")]
        [InlineData("/test/other")]
        public void GetPathByAddressReturnsCorrectODataPathWithPathBase(string pathBase)
        {
            // Arrange
            int address = 1;
            IPerRouteContainer container = new PerRouteContainer();
            container.AddRoute("odata", "ms/{data}");

            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<IPerRouteContainer>(container)
                .BuildServiceProvider();

            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            httpContext.Request.PathBase = pathBase;
            httpContext.ODataFeature().RouteName = "odata";
            RouteValueDictionary values = new RouteValueDictionary();
            values["odataPath"] = "";

            RouteValueDictionary ambientValues = new RouteValueDictionary();
            ambientValues["data"] = 2;

            // Act
            Mock<LinkGenerator> mock = new Mock<LinkGenerator>();
            ODataEndpointLinkGenerator linkGenerator = new ODataEndpointLinkGenerator(mock.Object);
            string path = linkGenerator.GetPathByAddress(httpContext, address, values, ambientValues);

            // Assert
            Assert.Equal(pathBase + "/ms/2", path);
        }

        [Fact]
        public void GetPathByAddressCallInnerLinkGeneratorGetPathByAddress()
        {
            // Arrange
            int address = 1;
            RouteValueDictionary values = new RouteValueDictionary();
            PathString pathBase = new PathString();
            FragmentString fragment = new FragmentString();
            LinkOptions options = null;
            Mock<LinkGenerator> mock = new Mock<LinkGenerator>();
            mock.Setup(g => g.GetPathByAddress(address, values, pathBase, fragment, options)).Verifiable();

            // Act
            ODataEndpointLinkGenerator linkGenerator = new ODataEndpointLinkGenerator(mock.Object);
            linkGenerator.GetPathByAddress(address, values, pathBase, fragment, options);

            // Assert
            mock.Verify();
        }

        [Fact]
        public void GetUriByAddressCallInnerLinkGeneratorGetUriByAddress()
        {
            // Arrange
            int address = 1;
            RouteValueDictionary values = new RouteValueDictionary();
            FragmentString fragment = new FragmentString();
            Mock<LinkGenerator> mock = new Mock<LinkGenerator>();
            HttpContext httpContext = new DefaultHttpContext();

            mock.Setup(g => g.GetUriByAddress(httpContext, address, values, null, null, null, null, fragment, null)).Verifiable();

            // Act
            ODataEndpointLinkGenerator linkGenerator = new ODataEndpointLinkGenerator(mock.Object);
            linkGenerator.GetUriByAddress(httpContext, address, values, null, null, null, null, fragment, null);

            // Assert
            mock.Verify();
        }

        [Theory]
        [InlineData("{abc}", "abcValue")]
        [InlineData("odata{abc", "odata{abc")]
        [InlineData("odataabc}", "odataabc}")]
        [InlineData("{abc}{xyz}", "abcValuexyzValue")]
        [InlineData("odata{abc xyz}", "odata{abc xyz}")]
        [InlineData("abc{abc}xyz{xyz}", "abcabcValuexyzxyzValue")]
        [InlineData("odata{abc}/{abc}", "odataabcValue/abcValue")]
        [InlineData("odata{abc}/{xyz}", "odataabcValue/xyzValue")]
        [InlineData("odata{unknow}/{xyz}", "odata{unknow}/xyzValue")]
        public void BindPrefixTemplateWorksAsExpected(string prefix, string expect)
        {
            // Arrange
            RouteValueDictionary values = new RouteValueDictionary
            (
                new
                {
                    abc = "abcValue",
                    xyz = "xyzValue"
                }
            );

            // Act
            string actual = ODataEndpointLinkGenerator.BindPrefixTemplate(prefix, values, null);

            // Assert
            Assert.Equal(expect, actual);
        }

        [Theory]
        [InlineData("api/v1/tenants/{tid:guid}", "api/v1/tenants/003890b9972e4092aef80e882f1e0999")]
        public void BindPrefixTemplateWithGuidParameterWorksAsExpected(string prefix, string expect)
        {
            // Arrange
            RouteValueDictionary values = new RouteValueDictionary
            (
                new
                {
                    tid = "003890b9972e4092aef80e882f1e0999"
                }
            );

            // Act
            string actual = ODataEndpointLinkGenerator.BindPrefixTemplate(prefix, values, null);

            // Assert
            Assert.Equal(expect, actual);
        }
    }
}
#endif