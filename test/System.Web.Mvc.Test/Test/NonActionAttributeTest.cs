// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace System.Web.Mvc.Test
{
    public class NonActionAttributeTest
    {
        [Fact]
        public void InValidActionForRequestReturnsFalse()
        {
            // Arrange
            NonActionAttribute attr = new NonActionAttribute();

            // Act & Assert
            Assert.False(attr.IsValidForRequest(null, null));
        }
    }
}
