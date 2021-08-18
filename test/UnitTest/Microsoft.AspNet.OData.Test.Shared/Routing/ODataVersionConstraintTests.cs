//-----------------------------------------------------------------------------
// <copyright file="ODataVersionConstraintTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Test
{
    public class ODataVersionConstraintTests
    {
        [Fact]
        public void Ctor_ParameterlessDefaultsToV4()
        {
            // Act
            ODataVersionConstraint constraint = new ODataVersionConstraint();

            // Assert
            Assert.Equal(ODataVersion.V4, constraint.Version);
        }

        [Fact]
        public void Matches_IfHeaders_Dont_Exist()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint();
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("OData-Version", "4.0")]
        [InlineData("OData-Version", "4.01")]
        [InlineData("OData-MinVersion", "4.0")]
        [InlineData("OData-MinVersion", "4.01")]
        [InlineData("OData-MaxVersion", "4.0")]
        [InlineData("OData-MaxVersion", "4.01")]
        public void Matches_Version(string header, string versionString)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint();
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add(header, versionString);

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("4.0", "4.0","4.0")]
        [InlineData("4.0", "4.0", "4.01")]
        [InlineData("4.0", "4.01", "4.01")]
        [InlineData("4.01", "4.0", "4.0")]
        [InlineData("4.01", "4.0", "4.01")]
        [InlineData("4.01", "4.01", "4.01")]
        public void Matches_MinMaxVersion(string odataVersion, string minVersion, string maxVersion)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint();
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add("OData-Version", odataVersion);
            request.Headers.Add("OData-MinVersion", minVersion);
            request.Headers.Add("OData-MaxVersion", maxVersion);

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("DataServiceVersion")]
        [InlineData("MinDataServiceVersion")]
        [InlineData("MaxDataServiceVersion")]
        public void DoesNotMatch_IfPreviousVersionHeadersExist(string headerName)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint();
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add(headerName, "2.0");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("DataServiceVersion")]
        [InlineData("MinDataServiceVersion")]
        [InlineData("MaxDataServiceVersion")]
        public void DoesNotMatch_IfOnlyPreviousVersionHeadersExistWithEnabledRelaxFlag(string headerName)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add(headerName, "3.0");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("DataServiceVersion")]
        [InlineData("MinDataServiceVersion")]
        [InlineData("MaxDataServiceVersion")]
        public void DoesNotMatch_IfPreviousVersionHeadersExistWithEnabledRelaxFlag(string headerName)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add(headerName, "3.0");
            request.Headers.Add("OData-Version", "4.0");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Matches_IfBothMaxVersionHeadersExistWithEnabledRelaxFlag()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add("MaxDataServiceVersion", "3.0");
            request.Headers.Add("OData-MaxVersion", "4.0");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void DoesNotMatch_IfBothMaxVersionAndPreviousVersionHeadersExistWithEnabledRelaxFlag()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add("MaxDataServiceVersion", "3.0");
            request.Headers.Add("DataServiceVersion", "3.0");
            request.Headers.Add("OData-MaxVersion", "4.0");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.False(result);
        }


        [Fact]
        public void Matches_WithoutVersionHeadersExistWithEnabledRelaxFlag()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Matches_IfOnlyVersionHeadersExistWithEnabledRelaxFlag()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add("OData-Version", "4.0");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Matches_IfRouteDirectionIsLinkGeneration()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint();
            TestVersionRequest request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.Add("OData-Version", "invalid");

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriGeneration);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("invalid", "4.0", false)]
        [InlineData("invalid", "invalid", false)]
        [InlineData(null, "invalid", false)]
        [InlineData("invalid", null, false)]
        [InlineData("3.0", "4.0", false)]
        [InlineData(null, "3.0", false)]
        [InlineData("4.0", "3.0", true)]
        [InlineData("4.0", null, true)]
        [InlineData("4.0", "invalid", true)]
        [InlineData(null, "4.0", true)]
        public void Matches_Only_WhenHeadersAreValidAndVersionIsValid(string dataServiceVersion,
            string maxDataServiceVersion, bool expectedResult)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint();
            var request = new TestVersionRequest(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            if (dataServiceVersion != null)
            {
                string versionHeaderName = ODataVersionConstraint.ODataServiceVersionHeader;
                request.Headers.Add(versionHeaderName, dataServiceVersion);
            }
            if (maxDataServiceVersion != null)
            {
                string maxVersionHeaderName = ODataVersionConstraint.ODataMaxServiceVersionHeader;
                request.Headers.Add(maxVersionHeaderName, maxDataServiceVersion);
            }

            // Act
            bool result = ConstraintMatch(constraint, request, RouteDirection.UriResolution);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Test class for abstracting the version request.
        /// </summary>
        private class TestVersionRequest
        {
            public TestVersionRequest(HttpMethod method, string uri)
            {
                this.Method = method;
                this.Uri = uri;
                this.Headers = new Dictionary<string, string>();
            }

            public HttpMethod Method { get; private set; }
            public string Uri { get; private set; }
            public Dictionary<string, string> Headers { get; private set; }
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
        /// <param name="versionRequest">The abstracted request.</param>
        /// <param name="direction">The abstracted route direction.</param>
        /// <returns>Result from constraint.Match,</returns>
        private bool ConstraintMatch(ODataVersionConstraint constraint, TestVersionRequest versionRequest, RouteDirection direction)
        {
#if NETCORE
            AspNetCore.Http.HttpContext context = new AspNetCore.Http.DefaultHttpContext();
            AspNetCore.Http.HttpRequest request = context.Request;
            foreach (KeyValuePair<string,string> kvp in versionRequest.Headers)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }

            System.Uri requestUri = new System.Uri(versionRequest.Uri);
            request.Method = versionRequest.Method.ToString();
            request.Host = new AspNetCore.Http.HostString(requestUri.Host, requestUri.Port);
            request.Scheme = requestUri.Scheme;

            AspNetCore.Routing.RouteDirection routeDirection = (direction == RouteDirection.UriResolution)
                ? AspNetCore.Routing.RouteDirection.IncomingRequest
                : AspNetCore.Routing.RouteDirection.UrlGeneration;

            return constraint.Match(context, null, null, null, routeDirection);
#else
            HttpRequestMessage request = new HttpRequestMessage(versionRequest.Method, versionRequest.Uri);
            foreach (KeyValuePair<string,string> kvp in versionRequest.Headers)
            {
                request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            System.Web.Http.Routing.HttpRouteDirection routeDirection = (direction == RouteDirection.UriResolution)
                ? System.Web.Http.Routing.HttpRouteDirection.UriResolution
                : System.Web.Http.Routing.HttpRouteDirection.UriGeneration;

            return constraint.Match(request, null, null, null, routeDirection);
#endif
        }
    }
}
