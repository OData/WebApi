// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

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
