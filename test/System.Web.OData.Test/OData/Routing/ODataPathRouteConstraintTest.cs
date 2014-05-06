// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
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
            var request = new HttpRequestMessage(HttpMethod.Get, "http://any/");
            HttpRouteCollection httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var values = new Dictionary<string, object>() { { "odataPath", "NotAnODataPath" } };
            var constraint = new ODataPathRouteConstraint(_pathHandler, _model, _routeName, _conventions);

            // Act & Assert
            Assert.False(constraint.Match(request, new HttpRoute(), null, values, HttpRouteDirection.UriResolution));
        }

        [Fact]
        public void Match_ReturnsTrue_IfODataPathCanBeParsed()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://any/");
            HttpRouteCollection httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var values = new Dictionary<string, object>() { { "odataPath", "$metadata" } };
            var constraint = new ODataPathRouteConstraint(_pathHandler, _model, _routeName, _conventions);

            // Act & Assert
            Assert.True(constraint.Match(request, new HttpRoute(), null, values, HttpRouteDirection.UriResolution));

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
            var request = new HttpRequestMessage(HttpMethod.Get, "http://any/");
            HttpRouteCollection httpRouteCollection = new HttpRouteCollection();
            httpRouteCollection.Add(_routeName, new HttpRoute());
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var values = new Dictionary<string, object>() { { "odataPath", "Customers/$count" } };
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            IEdmModel model = builder.GetEdmModel();
            var constraint = new ODataPathRouteConstraint(_pathHandler, model, _routeName, _conventions);

            // Act & Assert
            Assert.False(constraint.Match(request, new HttpRoute(), null, values, HttpRouteDirection.UriResolution));
        }
    }
}
