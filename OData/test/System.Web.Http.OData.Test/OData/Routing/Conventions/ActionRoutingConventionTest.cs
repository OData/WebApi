// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing.Conventions
{
    public class ActionRoutingConventionTest
    {
        [Theory]
        [InlineData("RoutingCustomers(1)/GetRelatedRoutingCustomers")]
        [InlineData("RoutingCustomers/GetProducts")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string path)
        {
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), path);
            ILookup<string, HttpActionDescriptor> emptyActionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/");
            controllerContext.Request.SetRouteData(new HttpRouteData(new HttpRoute()));

            string selectedAction = new ActionRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }
    }
}
