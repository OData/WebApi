//-----------------------------------------------------------------------------
// <copyright file="ODataBatchPathMappingTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Batch
{
    public class ODataBatchPathMappingTest
    {
        [Fact]
        public void ODataBatchPathMappingWorksForNormalTemplate()
        {
            // Arrange
            var mapping = new ODataBatchPathMapping();
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/$batch");
            mapping.AddRoute("odata", "$batch");

            // Act & Assert
            Assert.True(mapping.TryGetRouteName(request.HttpContext, out string outputName));
            Assert.Equal("odata", outputName);
        }

        [Theory]
        [InlineData("world")]
        [InlineData("kit")]
        public void ODataBatchPathMappingWorksForSimpleTemplate(string name)
        {
            // Arrange
            string routeName = "odata";
            string routeTemplate = "hello/{name}/$batch";
            string uri = "http://localhost/hello/" + name + "/$batch";
            var request = RequestFactory.Create(HttpMethod.Get, uri);
            var mapping = new ODataBatchPathMapping();

            // Act
            mapping.AddRoute(routeName, routeTemplate);

            bool result = mapping.TryGetRouteName(request.HttpContext, out string outputName);

            // Assert
            Assert.True(result);
            Assert.Equal(outputName, routeName);
            var routeData = request.ODataFeature().BatchRouteData;
            Assert.NotNull(routeData);
            var actual = Assert.Single(routeData);
            Assert.Equal("name", actual.Key);
            Assert.Equal(name, actual.Value);
        }

        [Theory]
        [InlineData("1", "4")]
        [InlineData("2", "3")]
        [InlineData("latest", "unknown")]
        public void ODataBatchPathMappingWorksForComplexTemplate(string version, string spec)
        {
            // Arrange
            string routeName = "odata";
            string routeTemplate = "/v{api-version:apiVersion}/odata{spec}/$batch";
            string uri = "http://localhost/v" + version + "/odata" + spec + "/$batch";
            var request = RequestFactory.Create(HttpMethod.Get, uri);
            var mapping = new ODataBatchPathMapping();

            // Act
            mapping.AddRoute(routeName, routeTemplate);

            bool result = mapping.TryGetRouteName(request.HttpContext, out string outputName);

            // Assert
            Assert.True(result);
            Assert.Equal(outputName, routeName);
            var routeData = request.ODataFeature().BatchRouteData;
            Assert.NotNull(routeData);
            Assert.Equal(new[] { "api-version", "spec" }, routeData.Keys);
            Assert.Equal(version, routeData["api-version"]);
            Assert.Equal(spec, routeData["spec"]);
        }
    }
}
