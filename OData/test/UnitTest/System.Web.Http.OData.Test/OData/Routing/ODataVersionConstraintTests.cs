﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.Routing;
using Microsoft.Data.OData;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing.Test
{
    public class ODataVersionConstraintTests
    {
        [Fact]
        public void Can_Create_ODataVersionConstraint()
        {
            // Arrange & Act
            ODataVersionConstraint versionConstraint = new ODataVersionConstraint(ODataVersion.V2, ODataVersion.V3);

            // Assert
            Assert.Equal(ODataVersion.V2, versionConstraint.MinVersion);
            Assert.Equal(ODataVersion.V3, versionConstraint.MaxVersion);
        }

        [Fact]
        public void Ctor_PamaterlessDefaultsToRangeV1ToV3()
        {
            // Act
            ODataVersionConstraint constraint = new ODataVersionConstraint();

            // Assert
            Assert.Equal(ODataVersion.V1, constraint.MinVersion);
            Assert.Equal(ODataVersion.V3, constraint.MaxVersion);
        }

        [Fact]
        public void Ctor_SingleParameterConfiguresSingleVersion()
        {
            // Act
            ODataVersionConstraint constraint = new ODataVersionConstraint(ODataVersion.V2);

            // Assert
            Assert.Equal(constraint.MinVersion, constraint.MaxVersion);
            Assert.Equal(ODataVersion.V2, constraint.MaxVersion);
        }

        [Fact]
        public void Ctor_ThrowsIfMinVersionGreaterThanMaxVersion()
        {
            // Act & Assert
            Assert.ThrowsArgumentGreaterThanOrEqualTo(
                () => new ODataVersionConstraint(ODataVersion.V3, ODataVersion.V2),
                "maxVersion", "V3", "V2");
        }

        [Fact]
        public void Matches_IfHeaders_Dont_Exist()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint(ODataVersion.V3);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");

            // Act
            bool result = constraint.Match(request, null, null, null, HttpRouteDirection.UriResolution);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("OData-Version")]
        [InlineData("OData-MaxVersion")]
        public void DoesNotMatch_IfNextVersionHeadersExist(string headerName)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint(ODataVersion.V2);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation(headerName, "4.0");

            // Act
            bool result = constraint.Match(request, null, null, null, HttpRouteDirection.UriResolution);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("OData-Version")]
        [InlineData("OData-MaxVersion")]
        public void DoesNotMatch_IfOnlyNextVersionHeadersExistWithEnabledRelaxFlag(string headerName)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation(headerName, "4.0");

            // Act
            bool result = constraint.Match(request, route: null, parameterName: null, values: null,
                routeDirection: HttpRouteDirection.UriResolution);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("OData-Version")]
        [InlineData("OData-MaxVersion")]
        public void DoesNotMatch_IfNextVersionHeadersExistWithEnabledRelaxFlag(string headerName)
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation("DataServiceVersion", "3.0");
            request.Headers.TryAddWithoutValidation(headerName, "4.0");

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
        public void DoesNotMatch_IfBothMaxVersionAndNextVersionHeadersExistWithEnabledRelaxFlag()
        {
            // Arrange
            ODataVersionConstraint constraint = new ODataVersionConstraint
            {
                IsRelaxedMatch = true
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation("MaxDataServiceVersion", "3.0");
            request.Headers.TryAddWithoutValidation("OData-MaxVersion", "4.0");
            request.Headers.TryAddWithoutValidation("OData-Version", "4.0");

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
            request.Headers.TryAddWithoutValidation("DataServiceVersion", "3.0");

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
            ODataVersionConstraint constraint = new ODataVersionConstraint(ODataVersion.V3);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:12345/itdoesnotmatter");
            request.Headers.TryAddWithoutValidation("DataServiceVersion", "invalid");

            // Act
            bool result = constraint.Match(request, null, null, null, HttpRouteDirection.UriGeneration);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("invalid", "3.0", false)]
        [InlineData("invalid", "invalid", false)]
        [InlineData(null, "invalid", false)]
        [InlineData("invalid", null, false)]
        [InlineData("4.0", "3.0", false)]
        [InlineData(null, "4.0", false)]
        [InlineData("3.0", "2.0", true)]
        [InlineData("3.0", null, true)]
        [InlineData("3.0", "invalid", true)]
        [InlineData(null, "3.0", true)]
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
