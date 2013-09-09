// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Description
{
    public class ApiExplorerTest
    {
        [Fact]
        public void Descriptions_RecognizesDirectRoutes()
        {
            var config = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor(config, "ApiExplorerValues", typeof(ApiExplorerValuesController));
            var action = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(ApiExplorerValuesController).GetMethod("Get"));
            var actions = new ReflectedHttpActionDescriptor[] { action };
            config.Routes.Add("Route", HttpRouteBuilder.BuildDirectRoute(routeTemplate, 0, actions));

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            ApiDescription description = Assert.Single(descriptions);
            Assert.Equal(HttpMethod.Get, description.HttpMethod);
            Assert.Equal(routeTemplate, description.RelativePath);
            Assert.Equal(action, description.ActionDescriptor);
        }

        [Fact]
        public void Descriptions_RecognizesIgnoreApiForDirectRoutes_Action()
        {
            var config = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor(config, "ApiExplorerValues", typeof(ApiExplorerValuesController));
            var actions = new ReflectedHttpActionDescriptor[] 
            {
                new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(ApiExplorerValuesController).GetMethod("Get")),
                new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(ApiExplorerValuesController).GetMethod("Post")),
            };
            config.Routes.Add("Route", HttpRouteBuilder.BuildDirectRoute(routeTemplate, 0, actions));

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            ApiDescription description = Assert.Single(descriptions);
            Assert.Equal(HttpMethod.Get, description.HttpMethod);
            Assert.Equal(routeTemplate, description.RelativePath);
        }

        [Fact]
        public void Descriptions_RecognizesIgnoreApiForDirectRoutes_Controller()
        {
            var config = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor(config, "IgnoreApiValues", typeof(IgnoreApiValuesController));
            var actions = new ReflectedHttpActionDescriptor[] 
            {
                new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(IgnoreApiValuesController).GetMethod("Get")),
                new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(IgnoreApiValuesController).GetMethod("Post")),
            };
            config.Routes.Add("Route", HttpRouteBuilder.BuildDirectRoute(routeTemplate, 0, actions));

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            Assert.Empty(descriptions);
        }

        public class ApiExplorerValuesController : ApiController
        {
            public void Get() { }

            [ApiExplorerSettings(IgnoreApi = true)]
            public void Post() { }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public class IgnoreApiValuesController : ApiController
        {
            public void Get() { }
            public void Post() { }
        }

        [Fact]
        public void Descriptions_RecognizesCompositeRoutes()
        {
            var config = new HttpConfiguration();
            var routeTemplate = "api/values";
            var controllerDescriptor = new HttpControllerDescriptor(config, "AttributeApiExplorerValues", typeof(AttributeApiExplorerValuesController));
            var action = new ReflectedHttpActionDescriptor(controllerDescriptor, typeof(AttributeApiExplorerValuesController).GetMethod("Action"));
            var actions = new ReflectedHttpActionDescriptor[] { action };

            var routeCollection = new HttpSubRouteCollection();
            routeCollection.Add("testroute", HttpRouteBuilder.BuildDirectRoute(routeTemplate, 0, actions));

            RouteCollectionRoute route = new RouteCollectionRoute();
            route.EnsureInitialized(() => routeCollection);

            config.Routes.Add("Route", route);

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            ApiDescription description = Assert.Single(descriptions);
            Assert.Equal(HttpMethod.Get, description.HttpMethod);
            Assert.Equal(routeTemplate, description.RelativePath);
            Assert.Equal(action, description.ActionDescriptor);
        }

        public class AttributeApiExplorerValuesController : ApiController
        {
            [Route("")]
            [HttpGet]
            public void Action() { }
        }
    }
}
