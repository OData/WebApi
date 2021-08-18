//-----------------------------------------------------------------------------
// <copyright file="EntitySetRoutingConventionTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class EntitySetRoutingConventionTest
    {
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string method)
        {
            // Arrange
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "http://any/", "RoutingCustomers");
            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var emptyActionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new EntitySetRoutingConvention(), odataPath, request, emptyActionMap);

            // Assert
            Assert.Null(selectedAction);
            Assert.Empty(SelectActionHelper.GetRouteData(request).Values);
        }

        [Theory]
        [InlineData("GET", "GetCustomersFromSpecialCustomer")]
        [InlineData("POST", "PostCustomerFromSpecialCustomer")]
        public void SelectAction_WithCast_Returns_ExpectedActionName(string method, string expected)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();

            IEdmCollectionType collection = new EdmCollectionType(new EdmEntityTypeReference(model.SpecialCustomer, isNullable: false));

            ODataPath odataPath = new ODataPath(new EntitySetSegment(model.Customers),
                new TypeSegment(collection, model.Customers));

            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(expected);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new EntitySetRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expected, selectedAction);
        }

        [Theory]
        [InlineData("GET", "GetCustomers")]
        [InlineData("GET", "Get")]
        public void SelectAction_ReturnsExpectedActionName_DollarCount(string method, string expected)
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            var odataPath = new ODataPath(new EntitySetSegment(model.Customers), CountSegment.Instance);
            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(expected);

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new EntitySetRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expected, selectedAction);
        }

        [Theory]
        [InlineData("Put")]
        [InlineData("Post")]
        [InlineData("Patch")]
        [InlineData("Delete")]
        public void SelectAction_ReturnsNull_NotSupportedMethodForDollarCount(string method)
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            var odataPath = new ODataPath(new EntitySetSegment(model.Customers), CountSegment.Instance);
            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("PostCustomer");

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new EntitySetRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }
    }
}
