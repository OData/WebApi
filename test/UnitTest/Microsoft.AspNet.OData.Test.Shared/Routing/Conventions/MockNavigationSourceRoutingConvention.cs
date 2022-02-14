//-----------------------------------------------------------------------------
// <copyright file="MockNavigationSourceRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Routing.Conventions;

namespace Microsoft.AspNet.OData.Test.Routing.Conventions
{
    class MockNavigationSourceRoutingConvention : NavigationSourceRoutingConvention
    {
#if NETCORE
        public override string SelectAction(
            AspNetCore.Routing.RouteContext routeContext, 
            SelectControllerResult controllerResult,
            System.Collections.Generic.IEnumerable<AspNetCore.Mvc.Controllers.ControllerActionDescriptor> actionDescriptors)
        {
            return null;
        }
#else
        public override string SelectAction(
            Microsoft.AspNet.OData.Routing.ODataPath odataPath,
            System.Web.Http.Controllers.HttpControllerContext controllerContext,
            System.Linq.ILookup<string, System.Web.Http.Controllers.HttpActionDescriptor> actionMap)
        {
            return null;
        }
#endif
    }
}
