// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.TestUtil;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Test
{
    public class ExceptionContextTest
    {
        [Fact]
        public void ConstructorThrowsIfExceptionIsNull()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            Exception exception = null;

            // Act & assert
            Assert.ThrowsArgumentNull(
                delegate { new ExceptionContext(controllerContext, exception); }, "exception");
        }

        [Fact]
        public void ExceptionProperty()
        {
            // Arrange
            ControllerContext controllerContext = new Mock<ControllerContext>().Object;
            Exception exception = new Exception();

            // Act
            ExceptionContext exceptionContext = new ExceptionContext(controllerContext, exception);

            // Assert
            Assert.Equal(exception, exceptionContext.Exception);
        }

        [Fact]
        public void ResultProperty()
        {
            // Arrange
            ExceptionContext exceptionContext = new Mock<ExceptionContext>().Object;

            // Act & assert
            MemberHelper.TestPropertyWithDefaultInstance(exceptionContext, "Result", new ViewResult(), EmptyResult.Instance);
        }
    }
}
