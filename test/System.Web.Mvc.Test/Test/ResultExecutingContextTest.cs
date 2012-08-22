// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class ResultExecutingContextTest
    {
        [Fact]
        public void ConstructorThrowsIfControllerContextIsNull()
        {
            // Arrange
            ControllerContext controllerContext = null;
            ActionResult result = new ViewResult();

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ResultExecutingContext(controllerContext, result); }, "controllerContext");
        }

        [Fact]
        public void ConstructorThrowsIfResultIsNull()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            ActionResult result = null;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ResultExecutingContext(controllerContext, result); }, "result");
        }

        [Fact]
        public void ResultProperty()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            ActionResult result = new ViewResult();

            // Act
            ResultExecutingContext resultExecutingContext = new ResultExecutingContext(controllerContext, result);

            // Assert
            Assert.Equal(result, resultExecutingContext.Result);
        }
    }
}
