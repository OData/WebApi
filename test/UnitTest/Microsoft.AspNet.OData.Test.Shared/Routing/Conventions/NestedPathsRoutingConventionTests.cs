using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using Xunit;

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    public class NestedPathsRoutingConventionTests
    {
        [Theory]
        [InlineData("RoutingCustomers", "Get")]
        [InlineData("RoutingCustomers", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)", "Get")]
        [InlineData("RoutingCustomers(1)", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Name", "Get")]
        [InlineData("RoutingCustomers(1)/Name", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Address", "Get")]
        [InlineData("RoutingCustomers(1)/Address", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Products", "Get")]
        [InlineData("RoutingCustomers(1)/Products", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Products(2)/Name", "Get")]
        [InlineData("RoutingCustomers(1)/Products(2)/Name", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Products/$count", "Get")]
        [InlineData("RoutingCustomers(1)/Products/$count", "GetRoutingCustomers")]

        public void SelectAction_ReturnsGetAction_IfActionHasNestedPathsAttribute(string path, string expectedAction)
        {
            // Arrange
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "http://any", path);
            var request = RequestFactory.Create(new HttpMethod("GET"), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(expectedAction);
            actionMap.First().MethodInfo = GetMethodWithNestedPathsAttribute();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NestedPathsRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Equal(expectedAction, selectedAction);
        }

        [Theory]
        [InlineData("RoutingCustomers")]
        [InlineData("RoutingCustomers(1)")]
        [InlineData("RoutingCustomers(1)/Name")]
        [InlineData("RoutingCustomers(1)/Products")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string path)
        {
            // Arrange
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "http://any", path);
            var request = RequestFactory.Create(new HttpMethod("GET"), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NestedPathsRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Theory]
        [InlineData("RoutingCustomers", "Get")]
        [InlineData("RoutingCustomers", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)", "Get")]
        [InlineData("RoutingCustomers(1)", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Name", "Get")]
        [InlineData("RoutingCustomers(1)/Name", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Address", "Get")]
        [InlineData("RoutingCustomers(1)/Address", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Products", "Get")]
        [InlineData("RoutingCustomers(1)/Products", "GetRoutingCustomers")]
        [InlineData("RoutingCustomers(1)/Products(2)/Name", "Get")]
        [InlineData("RoutingCustomers(1)/Products(2)/Name", "GetRoutingCustomers")]
        public void SelectAction_ReturnsNull_IfActionDoesNotHaveNestedPathsAttribute(string path, string action)
        {
            // Arrange
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "http://any", path);
            var request = RequestFactory.Create(new HttpMethod("GET"), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap(action);
            actionMap.First().MethodInfo = GetMethodWithoutNestedPathsAttribute();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NestedPathsRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Theory]
        [InlineData("POST", "RoutingCustomers")]
        [InlineData("PUT", "RoutingCustomers(1)")]
        [InlineData("DELETE", "RoutingCustomers(1)")]
        [InlineData("POST", "RoutingCustomers(1)/Products")]
        public void SelectAction_ReturnsNull_IfRequestMethodIsNotGet(string method, string path)
        {
            // Arrange
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "http://any", path);
            var request = RequestFactory.Create(new HttpMethod(method), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("Get");
            actionMap.First().MethodInfo = GetMethodWithNestedPathsAttribute();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NestedPathsRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }

        [Theory]
        // $ref requests are currently not supported
        [InlineData("RoutingCustomers(1)/Products(1)/$ref")]
        // $value currently not supported
        [InlineData("RoutingCustomers(1)/Products(1)/$value")]
        public void SelectAction_ReturnsNull_IfUnsupportPathSegment(string path)
        {
            // Arrange
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "http://any", path);
            var request = RequestFactory.Create(new HttpMethod("GET"), "http://localhost/");
            var actionMap = SelectActionHelper.CreateActionMap("Get");
            actionMap.First().MethodInfo = GetMethodWithNestedPathsAttribute();

            // Act
            string selectedAction = SelectActionHelper.SelectAction(new NestedPathsRoutingConvention(), odataPath, request, actionMap);

            // Assert
            Assert.Null(selectedAction);
        }


        MethodInfo GetMethodWithNestedPathsAttribute()
        {
            return this.GetType().GetMethod("MethodWithNestedPathsAttribute", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        MethodInfo GetMethodWithoutNestedPathsAttribute()
        {
            return this.GetType().GetMethod("MethodWithoutNestedPathsAttribute", BindingFlags.Instance | BindingFlags.NonPublic);
        }


        [EnableNestedPaths]
        void MethodWithNestedPathsAttribute()
        {

        }

        void MethodWithoutNestedPathsAttribute()
        {

        }


    }
}
