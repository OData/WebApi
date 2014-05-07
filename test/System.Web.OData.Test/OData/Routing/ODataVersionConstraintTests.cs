// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.OData.Extensions;
using Microsoft.OData.Core;
using Microsoft.TestCommon;

namespace System.Web.OData.Routing.Test
{
    public class ODataVersionConstraintTests
    {
        [Fact]
        public void Ctor_PamaterlessDefaultsToV4()
        {
            // Act
            ODataVersionConstraint constraint = new ODataVersionConstraint();

            // Assert
            Assert.NotNull(constraint.Version);
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
        public void DoesntMatch_IfPreviousVersionHeadersExist(string headerName)
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
                string versionHeaderName = HttpRequestMessageProperties.ODataServiceVersionHeader;
                request.Headers.TryAddWithoutValidation(versionHeaderName, dataServiceVersion);
            }
            if (maxDataServiceVersion != null)
            {
                string maxVersionHeaderName = HttpRequestMessageProperties.ODataMaxServiceVersionHeader;
                request.Headers.TryAddWithoutValidation(maxVersionHeaderName, maxDataServiceVersion);
            }

            // Act
            bool result = constraint.Match(request, null, null, null, HttpRouteDirection.UriResolution);

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
