using System.Web.Http.Controllers;
using System.Web.Http.Properties;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    [CLSCompliant(false)]
    public class ActionAttributesTest
    {
        [Theory]
        [InlineData("GET", "ActionAttributeTest/RetriveUsers", "RetriveUsers")]
        [InlineData("POST", "ActionAttributeTest/AddUsers", "AddUsers")]
        [InlineData("PUT", "ActionAttributeTest/UpdateUsers", "UpdateUsers")]
        [InlineData("DELETE", "ActionAttributeTest/RemoveUsers", "RemoveUsers")]
        [InlineData("PATCH", "ActionAttributeTest/Users", "Users")]
        [InlineData("HEAD", "ActionAttributeTest/Users", "Users")]
        [InlineData("GET", "ActionAttributeTest/Deny", "Deny")]
        [InlineData("POST", "ActionAttributeTest/Deny", "Deny")]
        [InlineData("PUT", "ActionAttributeTest/Deny", "Deny")]
        [InlineData("DELETE", "ActionAttributeTest/Deny", "Deny")]
        [InlineData("PATCH", "ActionAttributeTest/Deny", "Deny")]
        [InlineData("WHATEVER", "ActionAttributeTest/Deny", "Deny")]
        [InlineData("GET", "ActionAttributeTest/Approve", "Approve")]
        [InlineData("POST", "ActionAttributeTest/Approve", "Approve")]
        [InlineData("PUT", "ActionAttributeTest/Approve", "Approve")]
        [InlineData("DELETE", "ActionAttributeTest/Approve", "Approve")]
        [InlineData("PATCH", "ActionAttributeTest/Approve", "Approve")]
        [InlineData("WHATEVER", "ActionAttributeTest/Approve", "Approve")]
        public void SelectAction_OnRouteWithActionParameter(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ActionAttributeTestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);

            Assert.Equal<string>(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("POST", "ActionAttributeTest/RetriveUsers")]
        [InlineData("DELETE", "ActionAttributeTest/RetriveUsers")]
        [InlineData("WHATEVER", "ActionAttributeTest/RetriveUsers")]
        [InlineData("GET", "ActionAttributeTest/AddUsers")]
        [InlineData("PUT", "ActionAttributeTest/AddUsers")]
        [InlineData("WHATEVER", "ActionAttributeTest/AddUsers")]
        [InlineData("GET", "ActionAttributeTest/UpdateUsers")]
        [InlineData("WHATEVER", "ActionAttributeTest/UpdateUsers")]
        [InlineData("POST", "ActionAttributeTest/RemoveUsers")]
        [InlineData("DELETEME", "ActionAttributeTest/RemoveUsers")]
        [InlineData("GET", "ActionAttributeTest/Users")]
        [InlineData("POST", "ActionAttributeTest/Users")]
        [InlineData("PATCHING", "ActionAttributeTest/Users")]
        public void SelectAction_ThrowsMethodNotSupported_OnRouteWithActionParameter(string httpMethod, string requestUrl)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            Type controllerType = typeof(ActionAttributeTestController);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, controllerType.Name, controllerType);

            Assert.Throws<HttpResponseException>(() =>
                {
                    HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);
                },
                String.Format(SRResources.ApiControllerActionSelector_HttpMethodNotSupported, httpMethod));
        }

        [Theory]
        [InlineData("GET", "ActionAttributeTest/NonAction")]
        [InlineData("POST", "ActionAttributeTest/NonAction")]
        [InlineData("NonAction", "ActionAttributeTest/NonAction")]
        public void SelectAction_ThrowsNotFound_OnRouteWithActionParameter(string httpMethod, string requestUrl)
        {
            string routeUrl = "{controller}/{action}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            Type controllerType = typeof(ActionAttributeTestController);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, controllerType.Name, controllerType);


            Assert.Throws<HttpResponseException>(() =>
                {
                    HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);
                },
                String.Format(SRResources.ApiControllerActionSelector_ActionNotFound, controllerType.Name));
        }

        [Theory]
        [InlineData("GET", "ActionAttributeTest/", "RetriveUsers")]
        [InlineData("GET", "ActionAttributeTest/3", "RetriveUsers")]
        [InlineData("GET", "ActionAttributeTest/4?ssn=12345", "RetriveUsers")]
        [InlineData("POST", "ActionAttributeTest/", "AddUsers")]
        [InlineData("POST", "ActionAttributeTest/1", "AddUsers")]
        [InlineData("PUT", "ActionAttributeTest", "UpdateUsers")]
        [InlineData("PUT", "ActionAttributeTest/4", "UpdateUsers")]
        [InlineData("PUT", "ActionAttributeTest/4?extra=thing", "UpdateUsers")]
        [InlineData("DELETE", "ActionAttributeTest", "RemoveUsers")]
        [InlineData("PATCH", "ActionAttributeTest", "Users")]
        [InlineData("HEAD", "ActionAttributeTest/", "Users")]
        public void SelectAction_OnDefaultRoute(string httpMethod, string requestUrl, string expectedActionName)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };

            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, "test", typeof(ActionAttributeTestController));
            HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);

            Assert.Equal<string>(expectedActionName, descriptor.ActionName);
        }

        [Theory]
        [InlineData("OPTIONS", "ActionAttributeTest")]
        [InlineData("TRACE", "ActionAttributeTest")]
        [InlineData("NonAction", "ActionAttributeTest/")]
        [InlineData("DENY", "ActionAttributeTest")]
        [InlineData("APP", "ActionAttributeTest")]
        public void SelectAction_ThrowsMethodNotSupported_OnDefaultRoute(string httpMethod, string requestUrl)
        {
            string routeUrl = "{controller}/{id}";
            object routeDefault = new { id = RouteParameter.Optional };
            HttpControllerContext controllerContext = ApiControllerHelper.CreateControllerContext(httpMethod, requestUrl, routeUrl, routeDefault);
            Type controllerType = typeof(ActionAttributeTestController);
            controllerContext.ControllerDescriptor = new HttpControllerDescriptor(controllerContext.Configuration, controllerType.Name, controllerType);


            Assert.Throws<HttpResponseException>(() =>
                {
                    HttpActionDescriptor descriptor = ApiControllerHelper.SelectAction(controllerContext);
                },
                String.Format(SRResources.ApiControllerActionSelector_HttpMethodNotSupported, httpMethod));

        }
    }
}
