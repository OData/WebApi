// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Test
{
    public class HttpUnauthorizedResultTest
    {
        [Fact]
        public void ExecuteResult()
        {
            // Arrange
            Mock<ControllerContext> mockControllerContext = new Mock<ControllerContext>();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusCode = 401).Verifiable();
            mockControllerContext.SetupSet(c => c.HttpContext.Response.StatusDescription = "Some description").Verifiable();

            HttpUnauthorizedResult result = new HttpUnauthorizedResult("Some description");

            // Act
            result.ExecuteResult(mockControllerContext.Object);

            // Assert
            mockControllerContext.Verify();
        }

        [Fact]
        public void StatusCode()
        {
            Assert.Equal(401, new HttpUnauthorizedResult().StatusCode);
        }
    }
}
