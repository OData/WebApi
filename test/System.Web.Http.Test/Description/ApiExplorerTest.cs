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
            config.Routes.Add("Route", new HttpDirectRoute(routeTemplate, actions));

            var descriptions = new ApiExplorer(config).ApiDescriptions;

            ApiDescription description = Assert.Single(descriptions);
            Assert.Equal(HttpMethod.Get, description.HttpMethod);
            Assert.Equal(routeTemplate, description.RelativePath);
            Assert.Equal(action, description.ActionDescriptor);
        }

        public class ApiExplorerValuesController : ApiController
        {
            public void Get() { }
        }
    }
}
