// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.TestCommon;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Routing.Conventions
{
    public class EntitySetRoutingConventionTest
    {
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string httpMethod)
        {
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "http://any/", "RoutingCustomers");
            ILookup<string, HttpActionDescriptor> emptyActionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/");
            controllerContext.Request.SetRouteData(new HttpRouteData(new HttpRoute()));

            string selectedAction = new EntitySetRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }

        [Theory]
        [InlineData("GET", "GetCustomersFromSpecialCustomer")]
        [InlineData("POST", "PostCustomerFromSpecialCustomer")]
        public void SelectAction_WithCast_Returns_ExpectedActionName(string method, string expected)
        {
            // Arrange
            CustomersModelWithInheritance model = new CustomersModelWithInheritance();
            ODataPath odataPath = new ODataPath(new EntitySetPathSegment(model.Customers),
                new CastPathSegment(model.SpecialCustomer));

            var controllerContext = new HttpControllerContext()
            {
                Request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/"),
                RouteData = new HttpRouteData(new HttpRoute())
            };

            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => expected);

            // Act
            string actionName = new EntitySetRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expected, actionName);
        }

        [Theory]
        [InlineData("GET", "GetCustomers")]
        [InlineData("GET", "Get")]
        public void SelectAction_ReturnsExpectedActionName_DollarCount(string method, string expected)
        {
            // Arrange
            var model = new CustomersModelWithInheritance();
            var odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new CountPathSegment());

            HttpControllerContext controllerContext = new HttpControllerContext()
            {
                Request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/"),
                RouteData = new HttpRouteData(new HttpRoute())
            };

            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => expected);

            // Act
            string actionName = new EntitySetRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Equal(expected, actionName);
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
            var odataPath = new ODataPath(new EntitySetPathSegment(model.Customers), new CountPathSegment());

            HttpControllerContext controllerContext = new HttpControllerContext()
            {
                Request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/"),
                RouteData = new HttpRouteData(new HttpRoute())
            };

            ILookup<string, HttpActionDescriptor> actionMap = new HttpActionDescriptor[1].ToLookup(desc => "PostCustomer");

            // Act
            string actionName = new EntitySetRoutingConvention().SelectAction(odataPath, controllerContext, actionMap);

            // Assert
            Assert.Null(actionName);
        }
    }
}
