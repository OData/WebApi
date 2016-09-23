// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles OData metadata requests.
    /// </summary>
    public class MetadataRoutingConvention : IODataRoutingConvention
    {
        /// <inheritdoc/>
        public ActionDescriptor SelectAction(RouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            ODataPath odataPath = routeContext.HttpContext.Request.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;

            if (request.Method != ODataRouteConstants.HttpGet)
            {
                return null;
            }

            string controllerName = null;
            string actionName = null;
            if (odataPath.PathTemplate == "~")
            {
                controllerName = "Metadata";
                actionName = "GetServiceDocument";
            }
            else if (odataPath.PathTemplate == "~/$metadata")
            {
                controllerName = "Metadata";
                actionName = "GetMetadata";
            }

            IActionDescriptorCollectionProvider actionCollectionProvider =
                routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            Contract.Assert(actionCollectionProvider != null);

            return
                actionCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .FirstOrDefault(
                        c =>
                            c.ControllerName == controllerName &&
                            String.Equals(c.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
