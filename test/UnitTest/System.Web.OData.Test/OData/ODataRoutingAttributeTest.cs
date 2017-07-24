﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class ODataRoutingAttributeTest
    {
        [Fact]
        public void Initialize_RegistersActionSelector()
        {
            var config = new HttpConfiguration();
            var controllerSettings = new HttpControllerSettings(config);
            var controllerDescriptor = new HttpControllerDescriptor();
            controllerDescriptor.Configuration = config;

            new ODataRoutingAttribute().Initialize(controllerSettings, controllerDescriptor);

            Assert.IsType<ODataActionSelector>(controllerSettings.Services.GetActionSelector());
        }
    }
}
