// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Routing.Test
{
    public class ODataVersionConstraintTests
    {
        [Fact]
        public void Ctor_PamaterlessDefaultsToV4()
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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");

            // Act
            bool result = constraint.Match(request, null, null, null, HttpRouteDirection.UriResolution);

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation(headerName, "2.0");

            // Act
            bool result = constraint.Match(request, null, null, null, HttpRouteDirection.UriResolution);

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation(headerName, "3.0");

            // Act
            bool result = constraint.Match(request, route: null, parameterName: null, values: null,
                routeDirection: HttpRouteDirection.UriResolution);

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation(headerName, "3.0");
            request.Headers.TryAddWithoutValidation("OData-Version", "4.0");

            // Act
            bool result = constraint.Match(request, route: null, parameterName: null, values: null,
                routeDirection: HttpRouteDirection.UriResolution);

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation("MaxDataServiceVersion", "3.0");
            request.Headers.TryAddWithoutValidation("OData-MaxVersion", "4.0");

            // Act
            bool result = constraint.Match(request, route: null, parameterName: null, values: null,
                routeDirection: HttpRouteDirection.UriResolution);

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation("MaxDataServiceVersion", "3.0");
            request.Headers.TryAddWithoutValidation("DataServiceVersion", "3.0");
            request.Headers.TryAddWithoutValidation("OData-MaxVersion", "4.0");

            // Act
            bool result = constraint.Match(request, route: null, parameterName: null, values: null,
                routeDirection: HttpRouteDirection.UriResolution);

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");

            // Act
            bool result = constraint.Match(request, route: null, parameterName: null, values: null,
                routeDirection: HttpRouteDirection.UriResolution);

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation("OData-Version", "4.0");

            // Act
            bool result = constraint.Match(request, route: null, parameterName: null, values: null,
                routeDirection: HttpRouteDirection.UriResolution);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Matches_IfRouteDirectionIsLinkGeneration()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation("OData-Version", "invalid");

            // Act
            bool result = constraint.Match(request, null, null, null, HttpRouteDirection.UriGeneration);

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
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            if (dataServiceVersion != null)
            {
                string versionHeaderName = ODataVersionConstraint.ODataServiceVersionHeader;
                request.Headers.TryAddWithoutValidation(versionHeaderName, dataServiceVersion);
            }
            if (maxDataServiceVersion != null)
            {
                string maxVersionHeaderName = ODataVersionConstraint.ODataMaxServiceVersionHeader;
                request.Headers.TryAddWithoutValidation(maxVersionHeaderName, maxDataServiceVersion);
            }

            // Act
            bool result = constraint.Match(request, null, null, null, HttpRouteDirection.UriResolution);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
