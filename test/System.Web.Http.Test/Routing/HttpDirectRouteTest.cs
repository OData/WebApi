// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http.Routing
{
    public class HttpDirectRouteTest
    {
        [Fact]
        public void Ctor_SetsActionsDataToken()
        {
            var actions = new ReflectedHttpActionDescriptor[0];

            var route = new HttpDirectRoute("route", actions);

            Assert.Equal(actions, route.Actions);
        }

        [Fact]
        public void GetRouteData_AddsDefaultValuesAsNull()
        {
            var actions = new ReflectedHttpActionDescriptor[] { new ReflectedHttpActionDescriptor() };
            var route = new HttpDirectRoute("movies/{id}", actions);
            route.Defaults.Add("id", RouteParameter.Optional);

            var routeData = route.GetRouteData("", new HttpRequestMessage(HttpMethod.Get, "http://localhost/movies"));

            Assert.Null(routeData.Values["id"]);
        }
    }
}
