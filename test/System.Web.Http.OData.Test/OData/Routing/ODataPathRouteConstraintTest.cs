// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
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
            var values = new Dictionary<string, object>() { { "odataPath", "NotAnODataPath" } };

            var constraint = new ODataPathRouteConstraint(_pathHandler, _model, _routeName, _conventions);
            Assert.False(constraint.Match(_request, null, null, values, HttpRouteDirection.UriResolution));
        }

        [Fact]
        public void Match_ReturnsTrue_IfODataPathCanBeParsed()
        {
            var values = new Dictionary<string, object>() { { "odataPath", "$metadata" } };

            var constraint = new ODataPathRouteConstraint(_pathHandler, _model, _routeName, _conventions);
            Assert.True(constraint.Match(_request, null, null, values, HttpRouteDirection.UriResolution));

            Assert.Equal("ODataMetadata", values["controller"]);
            Assert.Same(_model, _request.GetEdmModel());
            Assert.Same(_routeName, _request.GetODataRouteName());
            Assert.Same(_conventions, _request.GetODataRoutingConventions());
            Assert.Same(_pathHandler, _request.GetODataPathHandler());
        }
    }
}
