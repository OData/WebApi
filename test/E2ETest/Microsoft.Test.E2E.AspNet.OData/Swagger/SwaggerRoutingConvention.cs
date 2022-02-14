//-----------------------------------------------------------------------------
// <copyright file="SwaggerRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Swagger
{
    public class SwaggerRoutingConvention : IODataRoutingConvention
    {
#if NETCORE
        public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        {
            ODataPath odataPath = routeContext.HttpContext.Request.ODataFeature().Path;
            if (odataPath != null && odataPath.PathTemplate == "~/$swagger")
            {
                IActionDescriptorCollectionProvider actionCollectionProvider =
                    routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();

                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == "Swagger");

                return actionDescriptors.Where(
                    c => String.Equals(c.ActionName, "GetSwagger", StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }
#else
        public string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            if (odataPath != null && odataPath.PathTemplate == "~/$swagger")
            {
                return "Swagger";
            }

            return null;
        }

        public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath != null && odataPath.PathTemplate == "~/$swagger")
            {
                return "GetSwagger";
            }

            return null;
        }
#endif
    }
}
