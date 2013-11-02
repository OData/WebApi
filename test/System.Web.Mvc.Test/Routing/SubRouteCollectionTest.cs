// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Mvc.Routing
{
    public class SubRouteCollectionTest
    {
        [Fact]
        public void SubRouteCollection_Throws_OnDuplicateNamedRoute()
        {
            // Arrange
            var collection = new SubRouteCollection();
            var route1 = new Route("Home/Index", new Mock<IRouteHandler>().Object);
            var route2 = new Route("Person/Index", new Mock<IRouteHandler>().Object);

            collection.Add(new RouteEntry("route", route1));

            var expectedError =
                "A route named 'route' is already in the route collection. Route names must be unique." + Environment.NewLine +
                Environment.NewLine +
                "Duplicates:" + Environment.NewLine +
                "Person/Index" + Environment.NewLine +
                "Home/Index";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => collection.Add(new RouteEntry("route", route2)), expectedError);
        }
    }
}
