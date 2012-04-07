// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    [CLSCompliant(false)]
    public class ApiControllerActionSelectorTest
    {
        [Theory]
        [InlineData("GET", "Test", "GetUsers")]
        [InlineData("GET", "Test/2", "GetUser")]
        [InlineData("GET", "Test/3?name=mario", "GetUserByNameAndId")]
        [InlineData("GET", "Test/3?name=mario&ssn=123456", "GetUserByNameIdAndSsn")]
        [InlineData("GET", "Test?name=mario&ssn=123456", "GetUserByNameAndSsn")]
        [InlineData("GET", "Test?name=mario&ssn=123456&age=3", "GetUserByNameAgeAndSsn")]
        [InlineData("GET", "Test/4?name=mario&age=20", "GetUserByNameAndId")]
        [InlineData("GET", "Test/5?random=9", "GetUser")]
        [InlineData("Post", "Test", "PostUser")]
        [InlineData("Post", "Test?name=mario&age=10", "PostUserByNameAndAge")]
        /// Note: Normally the following would not match DeleteUserByIdAndOptName because it has 'id' and 'age' as parameters while the DeleteUserByIdAndOptName action has 'id' and 'name'. 
        /// However, because the default value is provided on action parameter 'name', having the 'id' in the request was enough to match the action.
        [InlineData("Delete", "Test/6?age=10", "DeleteUserByIdAndOptName")]
        public void Route_Parameters_Default(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test", "GetUsers")]
        [InlineData("GET", "Test/2", "GetUsersByName")]
        [InlineData("GET", "Test/luigi?ssn=123456", "GetUserByNameAndSsn")]
        [InlineData("GET", "Test/luigi?ssn=123456&id=2&ssn=12345", "GetUserByNameIdAndSsn")]
        [InlineData("GET", "Test?age=10&ssn=123456", "GetUsers")]
        [InlineData("GET", "Test?id=3&ssn=123456&name=luigi", "GetUserByNameIdAndSsn")]
        [InlineData("POST", "Test/luigi?age=20", "PostUserByNameAndAge")]
        public void Route_Parameters_Non_Id(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{name}";
            object routeDefault = new { name = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test/3?NAME=mario", "GetUserByNameAndId")]
        [InlineData("GET", "Test/3?name=mario&SSN=123456", "GetUserByNameIdAndSsn")]
        [InlineData("GET", "Test?nAmE=mario&ssn=123456&AgE=3", "GetUserByNameAgeAndSsn")]
        [InlineData("Delete", "Test/6?AGe=10", "DeleteUserByIdAndOptName")]
        public void Route_Parameters_Casing(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{ID}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test/GetUsers", "GetUsers")]
        [InlineData("GET", "Test/GetUser", "GetUser")]
        [InlineData("GET", "Test/GetUser?id=3", "GetUser")]
        [InlineData("GET", "Test/GetUser/4?id=3", "GetUser")]
        [InlineData("GET", "Test/GetUserByNameAgeAndSsn", "GetUserByNameAgeAndSsn")]
        [InlineData("GET", "Test/GetUserByNameAndSsn", "GetUserByNameAndSsn")]
        [InlineData("POST", "Test/PostUserByNameAndAddress", "PostUserByNameAndAddress")]
        public void Route_Action(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test/getusers", "GetUsers")]
        [InlineData("GET", "Test/getuseR", "GetUser")]
        [InlineData("GET", "Test/Getuser?iD=3", "GetUser")]
        [InlineData("GET", "Test/GetUser/4?Id=3", "GetUser")]
        [InlineData("GET", "Test/GetUserByNameAgeandSsn", "GetUserByNameAgeAndSsn")]
        [InlineData("GET", "Test/getUserByNameAndSsn", "GetUserByNameAndSsn")]
        [InlineData("POST", "Test/PostUserByNameAndAddress", "PostUserByNameAndAddress")]
        public void Route_Action_Name_Casing(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "Test", "GetUsers")]
        [InlineData("GET", "Test/?name=peach", "GetUsersByName")]
        [InlineData("GET", "Test?name=peach", "GetUsersByName")]
        [InlineData("GET", "Test?name=peach&ssn=123456", "GetUserByNameAndSsn")]
        [InlineData("GET", "Test?name=peach&ssn=123456&age=3", "GetUserByNameAgeAndSsn")]
        public void Route_No_Action(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}";

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("GET", "ParameterAttribute/2", "GetUser")]
        [InlineData("GET", "ParameterAttribute?id=2", "GetUser")]
        [InlineData("GET", "ParameterAttribute?myId=2", "GetUserByMyId")]
        [InlineData("POST", "ParameterAttribute/3?name=user", "PostUserNameFromUri")]
        [InlineData("POST", "ParameterAttribute/3", "PostUserNameFromBody")]
        [InlineData("DELETE", "ParameterAttribute/3?name=user", "DeleteUserWithNullableIdAndName")]
        [InlineData("DELETE", "ParameterAttribute?address=userStreet", "DeleteUser")]
        public void ModelBindingParameterAttribute_AreAppliedWhenSelectingActions(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "ParameterAttribute", typeof(ParameterAttributeController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal(expectedActionName, descriptor.ActionName);
        }

        [Fact]
        public void RequestToAmbiguousAction_OnDefaultRoute()
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            string httpMethod = "Post";
            string requestUrl = "Test?name=mario";

            // This would result in ambiguous match because complex parameter is not considered for matching.
            // Therefore, PostUserByNameAndAddress(string name, Address address) would conflicts with PostUserByName(string name)
            Assert.Throws<HttpResponseException>(() =>
                {
                    HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
                    context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
                    HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);
                });
        }

        [Fact]
        public void RequestToActionWithNotSupportedHttpMethod_OnRouteWithAction()
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            string requestUrl = "Test/GetUsers";
            string httpMethod = "POST";

            var exception = Assert.Throws<HttpResponseException>(() =>
            {
                HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
                context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
                HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);
            });

            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<string>>(exception.Response.Content);
            Assert.Equal("The requested resource does not support http method 'POST'.", content.Value);
        }

        [Fact]
        public void RequestToActionWith_HttpMethodDefinedByAttributeAndActionName()
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            string requestUrl = "Test";
            string httpMethod = "PATCH";

            HttpControllerContext context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(context);

            Assert.Equal<string>("PutUser", descriptor.ActionName);

            // When you have the HttpMethod attribute, the convention should not be applied.
            httpMethod = "PUT";
            var exception = Assert.Throws<HttpResponseException>(() =>
            {
                context = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
                context.ControllerDescriptor = new HttpControllerDescriptor(context.Configuration, "test", typeof(TestController));
                ApiControllerHelper.SelectAction(context);
            });

            Assert.Equal(HttpStatusCode.MethodNotAllowed, exception.Response.StatusCode);
            var content = Assert.IsType<ObjectContent<string>>(exception.Response.Content);
            Assert.Equal("The requested resource does not support http method 'PUT'.", content.Value);
        }
    }
}
