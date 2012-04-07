// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Web.Helpers.Test
{
    public class MapsTest
    {
        [Fact]
        public void GetDirectionsQueryReturnsLocationIfNotEmpty()
        {
            // Arrange
            var location = "a%";
            var latitude = "12.34";
            var longitude = "-56.78";

            // Act
            string result = Maps.GetDirectionsQuery(location, latitude, longitude);

            // Assert
            Assert.Equal("a%25", result);
        }

        [Fact]
        public void GetDirectionsQueryReturnsLatitudeLongitudeIfLocationIsEmpty()
        {
            // Arrange
            var location = "";
            var latitude = "12.34%";
            var longitude = "-&56.78";

            // Act
            string result = Maps.GetDirectionsQuery(location, latitude, longitude);

            // Assert
            Assert.Equal("12.34%25%2c-%2656.78", result);
        }

        [Fact]
        public void GetDirectionsQueryUsesSpecifiedEncoder()
        {
            // Arrange
            var location = "24 gnidliuB tfosorciM";
            var latitude = "12.34%";
            var longitude = "-&56.78";
            Func<string, string> encoder = k => new String(k.Reverse().ToArray());

            // Act
            string result = Maps.GetDirectionsQuery(location, latitude, longitude, encoder);

            // Assert
            Assert.Equal("Microsoft Building 42", result);
        }
    }
}
