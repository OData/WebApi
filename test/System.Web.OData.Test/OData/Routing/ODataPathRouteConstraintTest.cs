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

        private static IList<string> _stringsWithGenDelims = new List<string>
        {
            { "virtualRoot/odata" },
            { "virtualRoot/prefix/odata" },
            { "some%2Fescaped%2Fslashes" },                     // "some/escaped/slashes"
            { "some%23hashes" },                                // "some#hashes"
            { "some%3Fquestion%3Fmarks" },                      // "some?question?marks"
            { "some%3flower%23escapes" },                       // "some?lower#escapes"
        };

        private static IList<string> _stringsWithoutGenDelims = new List<string>
        {
            { "" },
            { "odata" },
            { "some%20spaces" },                                // "some spaces"
            { "_some+plus+signs_" },
            { "some(sub)and&other=delims" },
            { "some(delims)but%2Bupper:escaped" },              // "some(delims)but+upper:escaped"
            { "some(delims)but%2blower:escaped" },              // "some(delims)but+lower:escaped"
            { ":[]@" },                                         // general delimeters that work
            { "Chinese%E8%A5%BF%E9%9B%85%E5%9B%BEChars" },      // "Chinese西雅图Chars"
            { "Unicode%D8%83Format%D8%83Char" },                // "Unicode؃Format؃Char", class Cf
            { "Unicode%E1%BF%BCTitlecase%E1%BF%BCChar" },       // "UnicodeῼTitlecaseῼChar", class Lt
            { "Unicode%E0%A4%83Combining%E0%A4%83Char" },       // "UnicodeःCombiningःChar", class Mc
        };

        public static TheoryDataSet<string> PrefixStrings
        {
            get
            {
                var dataSet = new TheoryDataSet<string>();
                foreach (var item in _stringsWithGenDelims)
                {
                    dataSet.Add(item);
                }

                foreach (var item in _stringsWithoutGenDelims)
                {
                    dataSet.Add(item);
                }

                return dataSet;
            }
        }

        // Cross product of prefixes (all of _stringsWithGenDelims and all _stringsWithoutGenDelims) with OData paths (all
        // of _stringsWithoutGenDelims).
        public static TheoryDataSet<string, string> PrefixAndODataStrings
        {
            get
            {
                var dataSet = new TheoryDataSet<string, string>();
                foreach (var prefix in _stringsWithGenDelims)
                {
                    foreach (var oDataPath in _stringsWithoutGenDelims)
                    {
                        dataSet.Add(prefix, oDataPath);
                    }
                }

                foreach (var prefix in _stringsWithoutGenDelims)
                {
                    foreach (var oDataPath in _stringsWithoutGenDelims)
                    {
                        dataSet.Add(prefix, oDataPath);
                    }
                }

                return dataSet;
            }
        }

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
        [PropertyData("PrefixStrings")]
        public void Match_DeterminesExpectedServiceRoot_ForMetadata(string prefixString)
        {
            // Arrange
            var expectedRoot = "http://any/" + prefixString;
            if (!String.IsNullOrEmpty(prefixString))
            {
                expectedRoot += '/';
            }

            var request = new HttpRequestMessage(HttpMethod.Get, expectedRoot + "$metadata");
            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };
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

        [Theory]
        [PropertyData("PrefixStrings")]
        public void Match_DeterminesExpectedServiceRoot_ForMetadataWithEscapedSeparator(string prefixString)
        {
            // Arrange
            var originalRoot = "http://any/" + prefixString;
            var expectedRoot = originalRoot;
            if (!String.IsNullOrEmpty(prefixString))
            {
                originalRoot += "%2F";  // Escaped '/'
            }

            var request = new HttpRequestMessage(HttpMethod.Get, originalRoot + "$metadata");
            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };
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

        [Theory]
        [PropertyData("PrefixAndODataStrings")]
        public void Match_DeterminesExpectedServiceRoot_ForFunctionCall(string prefixString, string oDataString)
        {
            // Arrange
            var expectedRoot = "http://any/" + prefixString;
            if (!String.IsNullOrEmpty(prefixString))
            {
                expectedRoot += '/';
            }

            var oDataPath = String.Format("Unbound(p0='{0}')", oDataString);
            var request = new HttpRequestMessage(HttpMethod.Get, expectedRoot + oDataPath);
            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var builder = new ODataModelBuilder();
            builder.Function("Unbound").Returns<string>().Parameter<string>("p0");
            var model = builder.GetEdmModel();

            var pathHandler = new TestPathHandler();
            var constraint = new ODataPathRouteConstraint(pathHandler, model, _routeName, _conventions);
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, Uri.UnescapeDataString(oDataPath) },
            };

            // Act
            var matched = constraint.Match(request, null, null, values, HttpRouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
        }

        [Theory]
        [PropertyData("PrefixAndODataStrings")]
        public void Match_DeterminesExpectedServiceRoot_ForFunctionCallWithEscapedSeparator(
            string prefixString,
            string oDataString)
        {
            // Arrange
            var originalRoot = "http://any/" + prefixString;
            var expectedRoot = originalRoot;
            if (!String.IsNullOrEmpty(prefixString))
            {
                originalRoot += "%2F";  // Escaped '/'
            }

            var oDataPath = String.Format("Unbound(p0='{0}')", oDataString);
            var request = new HttpRequestMessage(HttpMethod.Get, originalRoot + oDataPath);
            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };
            request.SetConfiguration(new HttpConfiguration(httpRouteCollection));

            var builder = new ODataModelBuilder();
            builder.Function("Unbound").Returns<string>().Parameter<string>("p0");
            var model = builder.GetEdmModel();

            var pathHandler = new TestPathHandler();
            var constraint = new ODataPathRouteConstraint(pathHandler, model, _routeName, _conventions);
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, Uri.UnescapeDataString(oDataPath) },
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
