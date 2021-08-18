//-----------------------------------------------------------------------------
// <copyright file="ODataRoutingAttributeTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETFX // ODataRoutingAttribute is only used in AspNet
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Xunit;

namespace Microsoft.AspNet.OData.Test
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
#endif
