// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing.Conventions;
using System.Web.OData.TestCommon.Models;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing
{
    public class ODataPathRouteConstraintTest
    {
        IEdmModel _model = new ODataConventionModelBuilder().GetEdmModel();
        string _routeName = "name";
        IODataPathHandler _pathHandler = new DefaultODataPathHandler();
        IEnumerable<IODataRoutingConvention> _conventions = ODataRoutingConventions.CreateDefault();
        HttpRequestMessage _request = new HttpRequestMessage();

        [Fact]
        public void Match_ReturnsTrue_ForUriGeneration()
        {            
            var values = new Dictionary<string, object>();

            var constraint = new ODataPathRouteConstraint(_pathHandler, _model, _routeName, _conventions);
            Assert.True(constraint.Match(_request, null, null, values, HttpRouteDirection.UriGeneration));
        }

        [Fact]
        public void Match_ReturnsFalse_IfODataPathRouteValueMissing()
        {
            var values = new Dictionary<string, object>();

            var constraint = new ODataPathRouteConstraint(_pathHandler, _model, _routeName, _conventions);
            Assert.False(constraint.Match(_request, null, null, values, HttpRouteDirection.UriResolution));
        }

        [Fact]
        public void Match_ReturnsFalse_IfODataPathCannotBeParsed()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://any/NotAnODataPath");
            HttpRouteCollection httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var values = new Dictionary<string, object>() { { "odataPath", "NotAnODataPath" } };
            var constraint = new ODataPathRouteConstraint(_pathHandler, _model, _routeName, _conventions);

            // Act & Assert
            Assert.False(constraint.Match(request, null, null, values, HttpRouteDirection.UriResolution));
        }

        [Fact]
        public void Match_ReturnsTrue_IfODataPathCanBeParsed()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://any/odata/$metadata");
            HttpRouteCollection httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var values = new Dictionary<string, object>() { { "odataPath", "$metadata" } };
            var constraint = new ODataPathRouteConstraint(_pathHandler, _model, _routeName, _conventions);

            // Act & Assert
            Assert.True(constraint.Match(request, null, null, values, HttpRouteDirection.UriResolution));

            Assert.Equal("Metadata", values["controller"]);
            Assert.Same(_model, request.ODataProperties().Model);
            Assert.Same(_routeName, request.ODataProperties().RouteName);
            Assert.Equal(_conventions, request.ODataProperties().RoutingConventions);
            Assert.Same(_pathHandler, request.ODataProperties().PathHandler);
        }

        [Fact]
        public void Match_ReturnsFalse_IfODataPathHasNotImplementedSegment()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://any/Customers/$count");
            HttpRouteCollection httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var values = new Dictionary<string, object>() { { "odataPath", "Customers/$count" } };
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            var constraint = new ODataPathRouteConstraint(_pathHandler, model, _routeName, _conventions);

            // Act & Assert
            Assert.False(constraint.Match(request, null, null, values, HttpRouteDirection.UriResolution));
        }

        [Theory]
        [InlineData("")]
        [InlineData("odata/")]
        [InlineData("virtualRoot/odata/")]
        [InlineData("virtualRoot/prefix/odata/")]
        [InlineData("some%20spaces")]
        [InlineData("some+significant+spaces/")]
        [InlineData("some%23hashes/")]
        [InlineData("some%3Fquestion%3Fmarks/")]
        public void Match_DeterminesExpectedServiceRoot(string precedingSegments)
        {
            // Arrange
            var expectedRoot = "http://any/" + precedingSegments;
            var request = new HttpRequestMessage(HttpMethod.Get, expectedRoot + "$metadata");
            var httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var pathHandler = new TestPathHandler();
            var constraint = new ODataPathRouteConstraint(pathHandler, _model, _routeName, _conventions);
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, "$metadata" },
            };

            // Act
            var matched = constraint.Match(request, null, null, values, HttpRouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
        }

        // Wrap a PathHandler to allow us to check serviceRoot the constraint calculates.
        private class TestPathHandler : DefaultODataPathHandler
        {
            public string ServiceRoot { get; private set; }

            public override ODataPath Parse(IEdmModel model, string serviceRoot, string odataPath)
            {
                ServiceRoot = serviceRoot;
                return base.Parse(model, serviceRoot, odataPath);
            }
        }
    }
}
