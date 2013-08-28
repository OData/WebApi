// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Mvc.Async;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Routing
{
    public class AttributeRoutingMapperTest
    {
        [Fact]
        public void MapMvcAttributeRoutes_DoesNotTryToInferRouteNames()
        {
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MyController));
            var mapper = new AttributeRoutingMapper(new RouteBuilder());

            var routeEntries = mapper.MapMvcAttributeRoutes(controllerDescriptor);

            var routeEntry = Assert.Single(routeEntries);
            Assert.Null(routeEntry.Name);
        }

        [Fact]
        public void MapMvcAttributeRoutes_RespectsActionNameAttribute()
        {
            var controllerDescriptor = new ReflectedAsyncControllerDescriptor(typeof(MyController));
            var mapper = new AttributeRoutingMapper(new RouteBuilder());

            var routeEntries = mapper.MapMvcAttributeRoutes(controllerDescriptor);

            var routeEntry = Assert.Single(routeEntries);
            Assert.Equal("ActionName", routeEntry.Route.Defaults["action"]);
        }

        public class MyController : Controller
        {
            [HttpGet]
            [Route("")]
            [ActionName("ActionName")]
            public void MethodName()
            {
            }
        }
    }
}
