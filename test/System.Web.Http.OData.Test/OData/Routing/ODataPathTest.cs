// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing
{
    public class ODataPathTest
    {
        [Fact]
        public void ToStringWithNoSegments()
        {
            // Arrange
            ODataPath path = new ODataPath();

            // Act
            string value = path.ToString();

            // Assert
            Assert.Empty(value);
        }

        [Fact]
        public void ToStringWithOneSegment()
        {
            // Arrange
            string expectedValue = "Set";
            ODataPath path = new ODataPath(new EntitySetPathSegment(expectedValue));

            // Act
            string value = path.ToString();

            // Assert
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void ToStringWithOneTwoSegments()
        {
            // Arrange
            string expectedFirstSegment = "Set";
            string expectedSecondSegment = "Action";
            ODataPath path = new ODataPath(new EntitySetPathSegment(expectedFirstSegment),
                new ActionPathSegment(expectedSecondSegment));

            // Act
            string value = path.ToString();

            // Assert
            string expectedValue = expectedFirstSegment + "/" + expectedSecondSegment;
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void ToStringWithKeyValueSegment()
        {
            // Arrange
            string segment = "1";
            ODataPath path = new ODataPath(new KeyValuePathSegment(segment));

            // Act
            string value = path.ToString();

            // Assert
            string expectedValue = "(" + segment + ")";
            Assert.Equal(expectedValue, value);
        }
    }
}
