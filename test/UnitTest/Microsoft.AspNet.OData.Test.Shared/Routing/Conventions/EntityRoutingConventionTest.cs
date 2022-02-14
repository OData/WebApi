//-----------------------------------------------------------------------------
// <copyright file="EntityRoutingConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
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
