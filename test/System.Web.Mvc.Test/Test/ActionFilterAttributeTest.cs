// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class ActionFilterAttributeTest
    {
        [Fact]
        public void DefaultOrderIsNegativeOne()
        {
            // Act
            var attr = new EmptyActionFilterAttribute();

            // Assert
            Assert.Equal(-1, attr.Order);
        }

        [Fact]
        public void OrderIsSetCorrectly()
        {
            // Act
            var attr = new EmptyActionFilterAttribute() { Order = 98052 };

            // Assert
            Assert.Equal(98052, attr.Order);
        }

        [Fact]
        public void SpecifyingInvalidOrderThrows()
        {
            // Act & Assert
            Assert.ThrowsArgumentOutOfRange(
                delegate { new EmptyActionFilterAttribute() { Order = -2 }; },
                "value",
                "Order must be greater than or equal to -1.");
        }

        private class EmptyActionFilterAttribute : ActionFilterAttribute
        {
        }
    }
}
