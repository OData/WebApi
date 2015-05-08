﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Routing.Conventions
{
    public class EntitySetRoutingConventionTest
    {
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        public void SelectAction_ReturnsNull_IfActionIsMissing(string httpMethod)
        {
            ODataPath odataPath = new DefaultODataPathHandler().Parse(ODataRoutingModel.GetModel(), "RoutingCustomers");
            ILookup<string, HttpActionDescriptor> emptyActionMap = new HttpActionDescriptor[0].ToLookup(desc => (string)null);
            HttpControllerContext controllerContext = new HttpControllerContext();
            controllerContext.Request = new HttpRequestMessage(new HttpMethod(httpMethod), "http://localhost/");
            controllerContext.Request.SetRouteData(new HttpRouteData(new HttpRoute()));

            string selectedAction = new EntitySetRoutingConvention().SelectAction(odataPath, controllerContext, emptyActionMap);

            Assert.Null(selectedAction);
            Assert.Empty(controllerContext.Request.GetRouteData().Values);
        }
    }
}
