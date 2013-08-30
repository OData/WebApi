// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Specialized;
using System.Web.Mvc;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Routing
{
    public class AttributeRoutingTest
    {
        private const string ResultKey = "MS_Test_Result";

        [Theory]
        [InlineData(typeof(DerivedController), "~/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedController), "~/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedController), "~/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedController), "~/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived", "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedPrefixController), "~/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/derived/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedPrefixController), "~/derived/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedPrefixController), "~/derived/BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived", "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/base/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/base/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/base/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedFromBasePrefixController), "~/BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived", "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/base/BaseMethodWithRoute", "BaseMethodWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/base/BaseVirtualMethodWithRoute", "BaseVirtualMethodWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/base/BaseVirtualMethodWithRouteToBeOverridenWithRoute", "BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/derived/DerivedMethodWithRoute", "DerivedMethodWithRoute")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/derived/BaseVirtualMethodToBeOverridenWithRoute_Derived", "BaseVirtualMethodToBeOverridenWithRoute_Derived")]
        [InlineData(typeof(DerivedWithPrefixFromBasePrefixController), "~/derived/BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived", "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
        public void AttributeRouting_WithInheritance_MethodOverrides(Type derivedController, string path, string expectedAction)
        {
            // Arrange
            var controllerTypes = new[] { derivedController, derivedController.BaseType };
            var routes = new RouteCollection();
            routes.MapMvcAttributeRoutes(controllerTypes);

            HttpContextBase context = GetContext(path);
            RouteData routeData = routes.GetRouteData(context);
            RequestContext requestContext = new RequestContext(context, routeData);
            MvcHandler handler = new MvcHandler(requestContext);

            // Act
            handler.ProcessRequest(context);

            // Assert
            ContentResult result = Assert.IsType<ContentResult>(context.Items[ResultKey]);
            Assert.Equal(expectedAction, result.Content);
        }

        private HttpContextBase GetContext(string path)
        {
            // mock HttpRequest
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(request => request.Url).Returns(new Uri("http://localhost/" + path.Substring(2)));
            requestMock.Setup(request => request.Form).Returns(new NameValueCollection());
            requestMock.Setup(request => request.ServerVariables).Returns(new NameValueCollection());
            requestMock.Setup(request => request.AppRelativeCurrentExecutionFilePath).Returns(path);

            // mock HttpResponse
            Mock<HttpResponseBase> responseBase = new Mock<HttpResponseBase>();

            // mock HttpContext
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.Setup(context => context.Items).Returns(new Hashtable());
            contextMock.Setup(context => context.Request).Returns(requestMock.Object);
            contextMock.Setup(context => context.Response).Returns(responseBase.Object);
            return contextMock.Object;
        }

        // stores the response from the action on the request context for later inspection.
        public class ResponseStoringController : Controller
        {
            protected override void OnActionExecuted(ActionExecutedContext filterContext)
            {
                filterContext.RequestContext.HttpContext.Items[ResultKey] = filterContext.Result;
            }
        }

        #region Inheritance Controllers

        public class BaseController : ResponseStoringController
        {
            [Route("BaseMethodWithRoute")]
            public string BaseMethodWithRoute()
            {
                return "BaseMethodWithRoute";
            }

            public virtual string BaseVirtualMethodToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodToBeOverridenWithRoute";
            }

            [Route("BaseVirtualMethodWithRoute")]
            public virtual string BaseVirtualMethodWithRoute()
            {
                return "BaseVirtualMethodWithRoute";
            }

            [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
            public virtual string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodWithRouteToBeOverridenWithRoute";
            }
        }

        [RoutePrefix("base")]
        public class BasePrefixController : ResponseStoringController
        {
            [Route("BaseMethodWithRoute")]
            public string BaseMethodWithRoute()
            {
                return "BaseMethodWithRoute";
            }

            public virtual string BaseVirtualMethodToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodToBeOverridenWithRoute";
            }

            [Route("BaseVirtualMethodWithRoute")]
            public virtual string BaseVirtualMethodWithRoute()
            {
                return "BaseVirtualMethodWithRoute";
            }

            [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute")]
            public virtual string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodWithRouteToBeOverridenWithRoute";
            }
        }

        public class DerivedController : BaseController
        {
            [Route("DerivedMethodWithRoute")]
            public string DerivedMethodWithRoute()
            {
                return "DerivedMethodWithRoute";
            }

            public override string BaseVirtualMethodWithRoute()
            {
                return "BaseVirtualMethodWithRoute_Derived";
            }

            [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
            public override string BaseVirtualMethodToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
            }

            [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
            public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
            }
        }

        public class DerivedFromBasePrefixController : BasePrefixController
        {
            [Route("DerivedMethodWithRoute")]
            public string DerivedMethodWithRoute()
            {
                return "DerivedMethodWithRoute";
            }

            public override string BaseVirtualMethodWithRoute()
            {
                return "BaseVirtualMethodWithRoute_Derived";
            }

            [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
            public override string BaseVirtualMethodToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
            }

            [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
            public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
            }
        }

        [RoutePrefix("derived")]
        public class DerivedPrefixController : BaseController
        {
            [Route("DerivedMethodWithRoute")]
            public string DerivedMethodWithRoute()
            {
                return "DerivedMethodWithRoute";
            }

            public override string BaseVirtualMethodWithRoute()
            {
                return "BaseVirtualMethodWithRoute_Derived";
            }

            [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
            public override string BaseVirtualMethodToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
            }

            [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
            public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
            }
        }

        [RoutePrefix("derived")]
        public class DerivedWithPrefixFromBasePrefixController : BasePrefixController
        {
            [Route("DerivedMethodWithRoute")]
            public string DerivedMethodWithRoute()
            {
                return "DerivedMethodWithRoute";
            }

            public override string BaseVirtualMethodWithRoute()
            {
                return "BaseVirtualMethodWithRoute_Derived";
            }

            [Route("BaseVirtualMethodToBeOverridenWithRoute_Derived")]
            public override string BaseVirtualMethodToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodToBeOverridenWithRoute_Derived";
            }

            [Route("BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived")]
            public override string BaseVirtualMethodWithRouteToBeOverridenWithRoute()
            {
                return "BaseVirtualMethodWithRouteToBeOverridenWithRoute_Derived";
            }
        }

        #endregion
    }
}
