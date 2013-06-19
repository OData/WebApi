// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Filters;
using Microsoft.TestCommon;

namespace System.Web.Http.Controllers
{
    public class HttpAuthenticationContextTests
    {
        [Fact]
        public void Constructor_Throws_WhenActionContextIsNull()
        {
            // Arrange
            HttpActionContext actionContext = null;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { CreateProductUnderTest(actionContext); }, "actionContext");
        }

        [Fact]
        public void ActionContext_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpActionContext expectedActionContext = CreateActionContext();
            HttpAuthenticationContext product = CreateProductUnderTest(expectedActionContext);

            // Act
            HttpActionContext actionContext = product.ActionContext;

            // Assert
            Assert.Same(expectedActionContext, actionContext);
        }

        [Fact]
        public void Request_ReturnsActionContextRequest()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpActionContext actionContext = CreateActionContext(expectedRequest);
                HttpAuthenticationContext product = CreateProductUnderTest(actionContext);

                // Act
                HttpRequestMessage request = product.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        private static HttpActionContext CreateActionContext()
        {
            return new HttpActionContext();
        }

        private static HttpActionContext CreateActionContext(HttpRequestMessage request)
        {
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = request;
            HttpActionContext actionContext = new HttpActionContext();
            actionContext.ControllerContext = controllerContext;
            return actionContext;
        }

        private static HttpAuthenticationContext CreateProductUnderTest(HttpActionContext actionContext)
        {
            return new HttpAuthenticationContext(actionContext);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }
    }
}
