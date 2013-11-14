// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;

namespace System.Web.Http
{
    public class HttpRouteCollectionTest
    {
        [Fact]
        public void HttpRouteCollection_Dispose_UniquifiesHandlers()
        {
            // Arrange
            var collection = new HttpRouteCollection();

            var handler1 = new Mock<HttpMessageHandler>();
            handler1.Protected().Setup("Dispose", true).Verifiable();

            var handler2 = new Mock<HttpMessageHandler>();
            handler1.Protected().Setup("Dispose", true).Verifiable();

            var route1 = new Mock<IHttpRoute>();
            route1.SetupGet(r => r.Handler).Returns(handler1.Object);

            var route2 = new Mock<IHttpRoute>();
            route2.SetupGet(r => r.Handler).Returns(handler1.Object);

            var route3 = new Mock<IHttpRoute>();
            route3.SetupGet(r => r.Handler).Returns(handler2.Object);

            collection.Add("route1", route1.Object);
            collection.Add("route2", route2.Object);
            collection.Add("route3", route3.Object);

            // Act
            collection.Dispose();

            // Assert
            handler1.Protected().Verify("Dispose", Times.Once(), true);
            handler2.Protected().Verify("Dispose", Times.Once(), true);
        }
    }
}
