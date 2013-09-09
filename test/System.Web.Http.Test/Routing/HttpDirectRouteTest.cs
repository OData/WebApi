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

            var route = HttpRouteBuilder.BuildDirectRoute("route", 0, actions);

            var actualActions = route.DataTokens[RouteKeys.ActionsDataTokenKey];
            Assert.Equal(actions, actualActions);
        }

        [Fact]
        public void GetRouteData_AddsDefaultValuesAsOptional()
        {
            var actions = new ReflectedHttpActionDescriptor[] { new ReflectedHttpActionDescriptor() };
            var route = HttpRouteBuilder.BuildDirectRoute("movies/{id}", 0, actions);
            route.Defaults.Add("id", RouteParameter.Optional);

            var routeData = route.GetRouteData("", new HttpRequestMessage(HttpMethod.Get, "http://localhost/movies"));

            Assert.Equal(RouteParameter.Optional, routeData.Values["id"]);
        }
    }
}
