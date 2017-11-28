// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation properties.
    /// </summary>
    public partial class NavigationRoutingConvention
    {
        /// <inheritdoc/>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        internal override string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            return SelectActionImpl(
                routeContext.HttpContext.ODataFeature().Path,
                new WebApiControllerContext(routeContext, controllerResult),
                new WebApiActionMap(actionDescriptors));
        }
    }
}
