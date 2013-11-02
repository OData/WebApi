// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class SubRouteCollectionTest
    {
        [Fact]
        public void SubRouteCollection_Throws_OnDuplicateNamedRoute()
        {
            // Arrange
            var collection = new SubRouteCollection();
            var route1 = new HttpRoute("api/Person");
            var route2 = new HttpRoute("api/Car");

            collection.Add(new RouteEntry("route", route1));

            var expectedError =
                "A route named 'route' is already in the route collection. Route names must be unique." + Environment.NewLine +
                Environment.NewLine +
                "Duplicates:" + Environment.NewLine +
                "api/Car" + Environment.NewLine +
                "api/Person";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => collection.Add(new RouteEntry("route", route2)), expectedError);
        }
    }
}
