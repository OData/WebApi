//-----------------------------------------------------------------------------
// <copyright file="TestEntitySetRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing.Conventions;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

#endif

namespace Microsoft.Test.E2E.AspNet.OData.Common
{
    public abstract class TestEntitySetRoutingConvention : EntitySetRoutingConvention
    {
        protected abstract string SelectAction(string requestMethod, ODataPath odataPath, TestControllerContext controllerContext, IList<string> actionMap);

#if NETCORE
        public override string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            string requestMethod = routeContext.HttpContext.Request.Method;
            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            TestControllerContext context = new TestControllerContext(routeContext);
            IList<string> actionLookup = actionDescriptors.Select(a => a.ActionName).Distinct().ToList();
            return SelectAction(requestMethod, odataPath, context, actionLookup);
        }
#else
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            string requestMethod = controllerContext.Request.Method.ToString().ToUpperInvariant();
            TestControllerContext context = new TestControllerContext(controllerContext);
            IList<string> actionLookup = actionMap.Select(g => g.Key).Distinct().ToList();
            return SelectAction(requestMethod, odataPath, context, actionLookup);
        }
#endif
    }
}
