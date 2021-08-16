//-----------------------------------------------------------------------------
// <copyright file="ODataEndpointPatternTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCOREAPP2_0
using System;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Extensions
{
    public class ODataEndpointPatternTests
    {
        [Fact]
        public void CreateODataEndpointPatternThrowsIfRouteNameIsNull()
        {
            // Arrange & Act
            Action test = () => ODataEndpointPattern.CreateODataEndpointPattern(null, "prefix");

            // Assert
            ExceptionAssert.ThrowsArgumentNull(test, "routeName");
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
