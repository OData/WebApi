// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http
{
    public class ErrorTest
    {
        [Fact]
        public void Format()
        {
            // Arrange
            string expected = "The formatted message";

            // Act
            string actual = Error.Format("The {0} message", "formatted");

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
