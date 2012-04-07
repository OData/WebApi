// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Web.Mvc.Properties;
using System.Web.Routing;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Mvc.Html.Test
{
    public class ChildActionExtensionsTest
    {
        Mock<HtmlHelper> htmlHelper;
        Mock<HttpContextBase> httpContext;
        Mock<RouteBase> route;
        Mock<IViewDataContainer> viewDataContainer;

        RouteData originalRouteData;
        RouteCollection routes;
        ViewContext viewContext;
        VirtualPathData virtualPathData;

        public ChildActionExtensionsTest()
        {
            route = new Mock<RouteBase>();
            route.Setup(r => r.GetVirtualPath(It.IsAny<RequestContext>(), It.IsAny<RouteValueDictionary>()))
                .Returns(() => virtualPathData);

            virtualPathData = new VirtualPathData(route.Object, "~/VirtualPath");

            routes = new RouteCollection();
            routes.Add(route.Object);

            originalRouteData = new RouteData();

            string returnValue = "";
            httpContext = new Mock<HttpContextBase>();
            httpContext.Setup(hc => hc.Request.ApplicationPath).Returns("~");
            httpContext.Setup(hc => hc.Response.ApplyAppPathModifier(It.IsAny<string>()))
                .Callback<string>(s => returnValue = s)
                .Returns(() => returnValue);
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()));

            viewContext = new ViewContext
            {
                RequestContext = new RequestContext(httpContext.Object, originalRouteData)
            };

            viewDataContainer = new Mock<IViewDataContainer>();

            htmlHelper = new Mock<HtmlHelper>(viewContext, viewDataContainer.Object, routes);
        }

        [Fact]
        public void GuardClauses()
        {
            // Act & Assert
            Assert.ThrowsArgumentNull(
                () => ChildActionExtensions.ActionHelper(null /* htmlHelper */, "abc", null /* controllerName */, null /* routeValues */, null /* textWriter */),
                "htmlHelper"
                );

            Assert.ThrowsArgumentNullOrEmpty(
                () => ChildActionExtensions.ActionHelper(htmlHelper.Object, null /* actionName */, null /* controllerName */, null /* routeValues */, null /* textWriter */),
                "actionName"
                );

            Assert.ThrowsArgumentNullOrEmpty(
                () => ChildActionExtensions.ActionHelper(htmlHelper.Object, String.Empty /* actionName */, null /* controllerName */, null /* routeValues */, null /* textWriter */),
                "actionName"
                );
        }

        [Fact]
        public void ServerExecuteCalledWithWrappedChildActionMvcHandler()
        {
            // Arrange
            IHttpHandler callbackHandler = null;
            TextWriter callbackTextWriter = null;
            bool callbackPreserveForm = false;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>(
                    (handler, textWriter, preserveForm) =>
                    {
                        callbackHandler = handler;
                        callbackTextWriter = textWriter;
                        callbackPreserveForm = preserveForm;
                    });
            TextWriter stringWriter = new StringWriter();

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, stringWriter);

            // Assert
            Assert.NotNull(callbackHandler);
            HttpHandlerUtil.ServerExecuteHttpHandlerWrapper wrapper = callbackHandler as HttpHandlerUtil.ServerExecuteHttpHandlerWrapper;
            Assert.NotNull(wrapper);
            Assert.NotNull(wrapper.InnerHandler);
            ChildActionExtensions.ChildActionMvcHandler childHandler = wrapper.InnerHandler as ChildActionExtensions.ChildActionMvcHandler;
            Assert.NotNull(childHandler);
            Assert.Same(stringWriter, callbackTextWriter);
            Assert.True(callbackPreserveForm);
        }

        [Fact]
        public void RouteDataTokensIncludesParentActionViewContext()
        {
            // Arrange
            MvcHandler mvcHandler = null;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, null /* textWriter */);

            // Assert
            Assert.Same(viewContext, mvcHandler.RequestContext.RouteData.DataTokens[ControllerContext.ParentActionViewContextToken]);
        }

        [Fact]
        public void RouteValuesIncludeNewActionName()
        {
            // Arrange
            MvcHandler mvcHandler = null;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("actionName", routeData.Values["action"]);
        }

        [Fact]
        public void RouteValuesIncludeOldControllerNameWhenControllerNameIsNullOrEmpty()
        {
            // Arrange
            originalRouteData.Values["controller"] = "oldController";
            MvcHandler mvcHandler = null;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("oldController", routeData.Values["controller"]);
        }

        [Fact]
        public void RouteValuesIncludeNewControllerNameWhenControllNameIsNotEmpty()
        {
            // Arrange
            originalRouteData.Values["controller"] = "oldController";
            MvcHandler mvcHandler = null;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", "newController", null /* routeValues */, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("newController", routeData.Values["controller"]);
        }

        [Fact]
        public void PassedRouteValuesOverrideParentRequestRouteValues()
        {
            // Arrange
            originalRouteData.Values["name1"] = "value1";
            originalRouteData.Values["name2"] = "value2";
            MvcHandler mvcHandler = null;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, new RouteValueDictionary { { "name2", "newValue2" } }, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Equal("value1", routeData.Values["name1"]);
            Assert.Equal("newValue2", routeData.Values["name2"]);

            Assert.Equal("newValue2", (routeData.Values[ChildActionValueProvider.ChildActionValuesKey] as DictionaryValueProvider<object>).GetValue("name2").RawValue);
        }

        [Fact]
        public void NoChildActionValuesDictionaryCreatedIfNoRouteValuesPassed()
        {
            // Arrange
            MvcHandler mvcHandler = null;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, null, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.Null(routeData.Values[ChildActionValueProvider.ChildActionValuesKey]);
        }

        [Fact]
        public void RouteValuesDoesNotIncludeExplicitlyPassedAreaName()
        {
            // Arrange
            Route route = routes.MapRoute("my-area", "my-area");
            route.DataTokens["area"] = "myArea";
            MvcHandler mvcHandler = null;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, new RouteValueDictionary { { "area", "myArea" } }, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.False(routeData.Values.ContainsKey("area"));
            Assert.Null((routeData.Values[ChildActionValueProvider.ChildActionValuesKey] as DictionaryValueProvider<object>).GetValue("area"));
        }

        [Fact]
        public void RouteValuesIncludeExplicitlyPassedAreaNameIfAreasNotInUse()
        {
            // Arrange
            Route route = routes.MapRoute("my-area", "my-area");
            MvcHandler mvcHandler = null;
            httpContext.Setup(hc => hc.Server.Execute(It.IsAny<IHttpHandler>(), It.IsAny<TextWriter>(), It.IsAny<bool>()))
                .Callback<IHttpHandler, TextWriter, bool>((handler, _, __) => mvcHandler = (MvcHandler)((HttpHandlerUtil.ServerExecuteHttpHandlerWrapper)handler).InnerHandler);

            // Act
            ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, new RouteValueDictionary { { "area", "myArea" } }, null /* textWriter */);

            // Assert
            RouteData routeData = mvcHandler.RequestContext.RouteData;
            Assert.True(routeData.Values.ContainsKey("area"));
            Assert.Equal("myArea", (routeData.Values[ChildActionValueProvider.ChildActionValuesKey] as DictionaryValueProvider<object>).GetValue("area").RawValue);
        }

        [Fact]
        public void NoMatchingRouteThrows()
        {
            // Arrange
            routes.Clear();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => ChildActionExtensions.ActionHelper(htmlHelper.Object, "actionName", null /* controllerName */, null /* routeValues */, null /* textWriter */),
                MvcResources.Common_NoRouteMatched
                );
        }
    }
}
