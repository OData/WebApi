// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that always selects the action named HandleUnmappedRequest if that action is present.
    /// </summary>
    public class UnmappedRequestRoutingConvention : NavigationSourceRoutingConvention
    {
        private const string UnmappedRequestActionName = "HandleUnmappedRequest";

        /// <inheritdoc/>
        public override ActionDescriptor SelectAction(RouteContext routeContext, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            return actionDescriptors.FindMatchingAction(UnmappedRequestActionName);
        }
    }
}
