// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Test.AspNet.OData.Factories;
using Xunit;

namespace Microsoft.Test.AspNet.OData.Routing.Conventions
{
    public class EntityRoutingConventionTest
    {
        [Theory]
        [InlineData("GET")]
        [InlineData("PUT")]
        [InlineData("PATCH")]
        [InlineData("DELETE")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string httpMethod)
        {
            // Arrange
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "http://any/", "RoutingCustomers(10)");
            var request = RequestFactory.Create(new HttpMethod(httpMethod), "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new EntityRoutingConvention(), odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }
    }
}
