//-----------------------------------------------------------------------------
// <copyright file="ODataPathRouteConstraintTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Routing
{
    public class ODataPathRouteConstraintTest
    {
        IEdmModel _model = ODataConventionModelBuilderFactory.Create().GetEdmModel();
        string _routeName = "name";
        IEnumerable<IODataRoutingConvention> _conventions = ODataRoutingConventions.CreateDefault();
        IServiceProvider _rootContainer;
        IODataPathHandler _pathHandler;

        private static IList<string> _stringsWithUnescapedSlashes = new List<string>
        {
            { "virtualRoot/odata" },
            { "virtualRoot/prefix/odata" }
        };

        private static IList<string> _stringsLegalEverywhere = new List<string>
        {
            { "some%23hashes" },                                // "some#hashes"
            { "some%2fslashes" },                               // "some/slashes"
            { "some%3Fquestion%3Fmarks" },                      // "some?question?marks"
            { "some%3flower%23escapes" },                       // "some?lower#escapes"
            { "" },
            { "odata" },
            { "some%20spaces" },                                // "some spaces"
            { "_some+plus+signs_" },
            { "some(sub)and&other=delims" },
            { "some(delims)but%2Bupper:escaped" },              // "some(delims)but+upper:escaped"
            { "some(delims)but%2blower:escaped" },              // "some(delims)but+lower:escaped"
#if NETCORE
            // RFC3986 specifies "[" and "]" and gen-delims in BNF notation in section  2.2
            // but section 3.2.2 specifies that the are only valid in a Uri as part of an IPV6
            // host. Therefore, they are not valid in any part of a Uri.
            { ":@" },                                           // general delimeters that work
#else
            // For back-compat, AspNet still tests with "[" and "]" as gen-delims.
            { ":[]@" },                                         // general delimeters that work
#endif
            { "Chinese%E8%A5%BF%E9%9B%85%E5%9B%BEChars" },      // "Chinese西雅图Chars"
            { "Unicode%D8%83Format%D8%83Char" },                // "Unicode؃Format؃Char", class Cf
            { "Unicode%E1%BF%BCTitlecase%E1%BF%BCChar" },       // "UnicodeῼTitlecaseῼChar", class Lt
            { "Unicode%E0%A4%83Combining%E0%A4%83Char" },       // "UnicodeःCombiningःChar", class Mc
        };

        public ODataPathRouteConstraintTest()
        {
            _rootContainer = new MockContainer(_model, _conventions);
            _pathHandler = _rootContainer.GetRequiredService<IODataPathHandler>();
        }

        public static TheoryDataSet<string> PrefixStrings
        {
            get
            {
                var dataSet = new TheoryDataSet<string>();
                foreach (var item in _stringsWithUnescapedSlashes)
                {
                    dataSet.Add(item);
                }

                foreach (var item in _stringsLegalEverywhere)
                {
                    dataSet.Add(item);
                }

                return dataSet;
            }
        }

        // Cross product of prefixes (all of _stringsWithUnescapedSlashes and all _stringsLegalEverywhere) with OData paths (all
        // of _stringsLegalEverywhere).
        public static TheoryDataSet<string, string> PrefixAndODataStrings
        {
            get
            {
                var dataSet = new TheoryDataSet<string, string>();
                foreach (var prefix in _stringsWithUnescapedSlashes)
                {
                    foreach (var oDataPath in _stringsLegalEverywhere)
                    {
                        dataSet.Add(prefix, oDataPath);
                    }
                }

                foreach (var prefix in _stringsLegalEverywhere)
                {
                    foreach (var oDataPath in _stringsLegalEverywhere)
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

            var constraint = CreatePathRouteConstraint();
            Assert.True(ConstraintMatch(constraint, null, values, RouteDirection.UriGeneration));
        }

        [Fact]
        public void Match_ReturnsFalse_IfODataPathRouteValueMissing()
        {
            var values = new Dictionary<string, object>();

            var constraint = CreatePathRouteConstraint();
            Assert.False(ConstraintMatch(constraint, null, values, RouteDirection.UriResolution));
        }

        [Fact]
        public void Match_ReturnsFalse_IfODataPathCannotBeParsed()
        {
            // Arrange
            var routeRequest = new TestRouteRequest(HttpMethod.Get, "http://any/NotAnODataPath");
            var values = new Dictionary<string, object>() { { "odataPath", "NotAnODataPath" } };
            var constraint = CreatePathRouteConstraint();

            // Act & Assert
            Assert.False(ConstraintMatch(constraint, routeRequest, values, RouteDirection.UriResolution));
        }

        [Fact]
        public void Match_ReturnsTrue_IfODataPathCanBeParsed()
        {
            // Arrange
            var routeRequest = new TestRouteRequest(HttpMethod.Get, "http://any/odata/$metadata");
            string expectedRoute = "$metadata";
            var values = new Dictionary<string, object>() { { "odataPath", expectedRoute } };
            var constraint = CreatePathRouteConstraint();

            // Act & Assert
            Assert.True(ConstraintMatch(constraint, routeRequest, values, RouteDirection.UriResolution));
#if NETCORE
            Assert.Equal(_routeName, routeRequest.InnerRequest.ODataFeature().RouteName);
            Assert.NotNull(routeRequest.InnerRequest.ODataFeature().Path);
            Assert.Equal(expectedRoute, routeRequest.InnerRequest.ODataFeature().Path.ToString());
#else
            Assert.Equal("Metadata", values["controller"]);
            Assert.Same(_model, routeRequest.InnerRequest.GetModel());
            Assert.Equal(_conventions, routeRequest.InnerRequest.GetRoutingConventions());
            Assert.Same(_pathHandler, routeRequest.InnerRequest.GetPathHandler());
            Assert.Equal(_routeName, routeRequest.InnerRequest.ODataProperties().RouteName);
            Assert.NotNull(routeRequest.InnerRequest.ODataProperties().Path);
            Assert.Equal(expectedRoute, routeRequest.InnerRequest.ODataProperties().Path.ToString());
#endif
        }

        [Theory]
        [MemberData(nameof(PrefixStrings))]
        public void Match_DeterminesExpectedServiceRoot_ForMetadata(string prefixString)
        {
            // Arrange
            var expectedRoot = "http://any/" + prefixString;
            if (!String.IsNullOrEmpty(prefixString))
            {
                expectedRoot += '/';
            }

            var pathHandler = new TestPathHandler();
            var routeRequest = new TestRouteRequest(HttpMethod.Get, expectedRoot + "$metadata")
            {
                PathHandler = pathHandler,
            };

            var constraint = CreatePathRouteConstraint();
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, "$metadata" },
            };

            // Act
            var matched = ConstraintMatch(constraint, routeRequest, values, RouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
            Assert.NotNull(pathHandler.ODataPath);
            Assert.Equal("$metadata", pathHandler.ODataPath);
        }

        [Theory]
        [MemberData(nameof(PrefixStrings))]
        public void Match_DeterminesExpectedServiceRoot_ForMetadataWithEscapedSeparator(string prefixString)
        {
            // Arrange
            var originalRoot = "http://any/" + prefixString;
            var expectedRoot = originalRoot;
            if (!String.IsNullOrEmpty(prefixString))
            {
                originalRoot += "%2F";  // Escaped '/'
            }

            var pathHandler = new TestPathHandler();
            var routeRequest = new TestRouteRequest(HttpMethod.Get, originalRoot + "$metadata")
            {
                PathHandler = pathHandler,
            };

            var constraint = CreatePathRouteConstraint();
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, "$metadata" },
            };

            // Act
            var matched = ConstraintMatch(constraint, routeRequest, values, RouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
            Assert.NotNull(pathHandler.ODataPath);
            Assert.Equal("$metadata", pathHandler.ODataPath);
        }

        [Theory]
        [MemberData(nameof(PrefixAndODataStrings))]
        public void Match_DeterminesExpectedServiceRoot_ForFunctionCall(string prefixString, string oDataString)
        {
            // Arrange
            var expectedRoot = "http://any/" + prefixString;
            if (!String.IsNullOrEmpty(prefixString))
            {
                expectedRoot += '/';
            }

            var builder = new ODataModelBuilder();
            builder.Function("Unbound").Returns<string>().Parameter<string>("p0");
            var model = builder.GetEdmModel();

            var pathHandler = new TestPathHandler();
            var oDataPath = String.Format("Unbound(p0='{0}')", oDataString);
            var routeRequest = new TestRouteRequest(HttpMethod.Get, expectedRoot + oDataPath)
            {
                PathHandler = pathHandler,
                Model = model,
            };

            var constraint = CreatePathRouteConstraint();
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, Uri.UnescapeDataString(oDataPath) },
            };

            // Act
            var matched = ConstraintMatch(constraint, routeRequest, values, RouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
            Assert.NotNull(pathHandler.ODataPath);
            Assert.Equal(oDataPath, pathHandler.ODataPath);
        }

        [Theory]
        [MemberData(nameof(PrefixAndODataStrings))]
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

            var builder = new ODataModelBuilder();
            builder.Function("Unbound").Returns<string>().Parameter<string>("p0");
            var model = builder.GetEdmModel();

            var pathHandler = new TestPathHandler();
            var oDataPath = String.Format("Unbound(p0='{0}')", oDataString);
            var routeRequest = new TestRouteRequest(HttpMethod.Get, originalRoot + oDataPath)
            {
                PathHandler = pathHandler,
                Model = model,
            };

            var constraint = CreatePathRouteConstraint();
            var values = new Dictionary<string, object>
            {
                { ODataRouteConstants.ODataPath, Uri.UnescapeDataString(oDataPath) },
            };

            // Act
            var matched = ConstraintMatch(constraint, routeRequest, values, RouteDirection.UriResolution);

            // Assert
            Assert.True(matched);
            Assert.NotNull(pathHandler.ServiceRoot);
            Assert.Equal(expectedRoot, pathHandler.ServiceRoot);
            Assert.NotNull(pathHandler.ODataPath);
            Assert.Equal(oDataPath, pathHandler.ODataPath);
        }

        private ODataPathRouteConstraint CreatePathRouteConstraint()
        {
            return new ODataPathRouteConstraint(_routeName);
        }

        // Wrap a PathHandler to allow us to check serviceRoot the constraint calculates.
        private class TestPathHandler : DefaultODataPathHandler
        {
            public string ServiceRoot { get; private set; }
            public string ODataPath { get; private set; }

            public override ODataPath Parse(string serviceRoot, string odataPath, IServiceProvider requestContainer)
            {
                ServiceRoot = serviceRoot;
                ODataPath = odataPath;
                return base.Parse(serviceRoot, odataPath, requestContainer);
            }
        }

        /// <summary>
        /// Test class for abstracting the version request.
        /// </summary>
        private class TestRouteRequest
        {
            public TestRouteRequest(HttpMethod method, string uri)
            {
                this.Method = method;
                this.Uri = uri;
            }

            public HttpMethod Method { get; private set; }
            public string Uri { get; private set; }
            public IODataPathHandler PathHandler { get; set; }
            public IEdmModel Model { get; set; }
#if NETCORE
            public HttpRequest InnerRequest { get; set; }
#else
            public HttpRequestMessage InnerRequest { get; set; }
#endif
        }

        /// <summary>
        /// Abstraction for route direction.
        /// </summary>
        private enum RouteDirection
        {
            UriResolution = 0,
            UriGeneration
        }

        /// <summary>
        /// Test method to call constraint.Match using the proper arguments for each platform.
        /// </summary>
        /// <param name="constraint">The constraint object.</param>
        /// <param name="routeRequest">The abstracted request.</param>
        /// <param name="direction">The abstracted route direction.</param>
        /// <returns>Result from constraint.Match,</returns>
        private bool ConstraintMatch(ODataPathRouteConstraint constraint, TestRouteRequest routeRequest, Dictionary<string, object> values, RouteDirection direction)
        {
#if NETCORE
            IRouteBuilder config = RoutingConfigurationFactory.Create();
            if (routeRequest?.PathHandler != null && routeRequest?.Model != null)
            {
                config.MapODataServiceRoute(_routeName, "", builder =>
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routeRequest.PathHandler)
                           .AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routeRequest.Model));
            }
            else if (routeRequest?.PathHandler != null)
            {
                config.MapODataServiceRoute(_routeName, "", builder =>
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routeRequest.PathHandler));
            }
            else if (routeRequest?.Model != null)
            {
                config.MapODataServiceRoute(_routeName, "", builder =>
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routeRequest.Model));
            }
            else
            {
                config.MapODataServiceRoute(_routeName, "", builder => { });
            }

            HttpRequest request = (routeRequest != null)
                ? RequestFactory.Create(routeRequest.Method, routeRequest.Uri, config, _routeName)
                : RequestFactory.Create();

            // The RequestFactory will create a request container which most tests want but for checking the constraint,
            // we don't want a request container before the test runs since Match() creates one.
            request.DeleteRequestContainer(true);

            if (routeRequest != null)
            {
                routeRequest.InnerRequest = request;
            }

            AspNetCore.Routing.RouteDirection routeDirection = (direction == RouteDirection.UriResolution)
                ? AspNetCore.Routing.RouteDirection.IncomingRequest
                : AspNetCore.Routing.RouteDirection.UrlGeneration;

            RouteValueDictionary routeValues = new RouteValueDictionary(values);

            return constraint.Match(request.HttpContext, null, null, routeValues, routeDirection);
#else
            HttpRequestMessage request = (routeRequest != null)
                ? new HttpRequestMessage(routeRequest.Method, routeRequest.Uri)
                : new HttpRequestMessage();

            var httpRouteCollection = new HttpRouteCollection
            {
                { _routeName, new HttpRoute() },
            };

            var configuration = new HttpConfiguration(httpRouteCollection);
            if (routeRequest != null && routeRequest.PathHandler != null && routeRequest.Model != null)
            {
                configuration.CreateODataRootContainer(_routeName, builder =>
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routeRequest.PathHandler)
                           .AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routeRequest.Model));
            }
            else if (routeRequest != null && routeRequest.PathHandler != null)
            {
                configuration.CreateODataRootContainer(_routeName, builder =>
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routeRequest.PathHandler));
            }
            else if (routeRequest != null && routeRequest.Model != null)
            {
                configuration.CreateODataRootContainer(_routeName, builder =>
                    builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => routeRequest.Model));
            }
            else
            {
                PerRouteContainer perRouteContainer = configuration.GetPerRouteContainer() as PerRouteContainer;
                perRouteContainer.SetODataRootContainer(_routeName, _rootContainer);
            }

            request.SetConfiguration(configuration);
            if (routeRequest != null)
            {
                routeRequest.InnerRequest = request;
            }

            HttpRouteDirection routeDirection = (direction == RouteDirection.UriResolution)
                ? HttpRouteDirection.UriResolution
                : HttpRouteDirection.UriGeneration;

            return constraint.Match(request, null, null, values, routeDirection);
#endif
        }
    }
}
