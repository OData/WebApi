// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

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
            Assert.Same(_model, _request.ODataProperties().Model);
            Assert.Same(_routeName, _request.ODataProperties().RouteName);
            Assert.Same(_conventions, _request.ODataProperties().RoutingConventions);
            Assert.Same(_pathHandler, _request.ODataProperties().PathHandler);
        }

        [Fact]
        public void Match_ThrowsHttpResponseException_IfPathParserThrowsODataException()
        {
            var values = new Dictionary<string, object>() { { "odataPath", "" } };
            _request.SetConfiguration(new HttpConfiguration() { IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always });
            Mock<IODataPathHandler> pathHandler = new Mock<IODataPathHandler>();
            string exceptionMessage = "NOOODATA";
            pathHandler.Setup(handler => handler.Parse(_model, "")).Throws(new ODataException(exceptionMessage));
            var constraint = new ODataPathRouteConstraint(pathHandler.Object, _model, _routeName, _conventions);

            var ex = Assert.Throws<HttpResponseException>(
                () => constraint.Match(_request, null, null, values, HttpRouteDirection.UriResolution));

            Assert.Equal(HttpStatusCode.NotFound, ex.Response.StatusCode);
            HttpError error = ex.Response.Content.ReadAsAsync<HttpError>().Result;
            Assert.Equal("The OData path is invalid.", error.Message);
            Assert.Equal(exceptionMessage, error.ExceptionMessage);
        }
    }
}
