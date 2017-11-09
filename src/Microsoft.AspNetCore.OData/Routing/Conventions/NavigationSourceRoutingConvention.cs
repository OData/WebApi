// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation sources
    /// (entity sets or singletons)
    /// </summary>
    public abstract partial class NavigationSourceRoutingConvention : IODataRoutingConvention
    {
        /// <inheritdoc/>
        public ControllerActionDescriptor SelectAction(RouteContext routeContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="routeContext">The route context.</param>
        /// <param name="actionDescriptors">The list of action descriptors.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the action descriptor of the selected action.
        /// </returns>
        internal abstract string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors);
    }
}
