// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation sources
    /// (entity sets or singletons)
    /// </summary>
    public abstract partial class NavigationSourceRoutingConvention : IODataRoutingConvention
    {
        /// <inheritdoc/>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            HttpRequest request = routeContext.HttpContext.Request;

            SelectControllerResult controllerResult = SelectControllerImpl(odataPath);
            if (controllerResult != null)
            {
                // Get a IActionDescriptorCollectionProvider from the global service provider.
                IActionDescriptorCollectionProvider actionCollectionProvider =
                    routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
                Contract.Assert(actionCollectionProvider != null);

                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == controllerResult.ControllerName);

                if (actionDescriptors != null)
                {
                    string actionName = SelectAction(routeContext, controllerResult, actionDescriptors);
                    if (!String.IsNullOrEmpty(actionName))
                    {
                        return actionDescriptors.Where(
                            c => String.Equals(c.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="routeContext">The route context.</param>
        /// <param name="controllerResult">The result of selecting a controller.</param>
        /// <param name="actionDescriptors">The list of action descriptors.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the action descriptor of the selected action.
        /// </returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public abstract string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors);
    }
}
