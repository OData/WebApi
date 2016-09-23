// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation sources
    /// (entity sets or singletons)
    /// </summary>
    public abstract class NavigationSourceRoutingConvention : IODataRoutingConvention
    {
        /// <inheritdoc/>
        public virtual ActionDescriptor SelectAction(RouteContext routeContext)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            ODataPath odataPath = routeContext.HttpContext.Request.ODataFeature().Path;

            string controllerName = null;

            // entity set
            EntitySetSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetSegment;
            if (entitySetSegment != null)
            {
                controllerName = entitySetSegment.EntitySet.Name;
            }

            // singleton
            SingletonSegment singletonSegment = odataPath.Segments.FirstOrDefault() as SingletonSegment;
            if (singletonSegment != null)
            {
                controllerName = singletonSegment.Singleton.Name;
            }

            if (String.IsNullOrEmpty(controllerName))
            {
                return null;
            }

            IActionDescriptorCollectionProvider actionCollectionProvider =
                routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            Contract.Assert(actionCollectionProvider != null);

            IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                .Where(c => c.ControllerName == controllerName);

            return SelectAction(routeContext, actionDescriptors);
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="routeContext">The route context.</param>
        /// <param name="actionDescriptors">The list of action descriptors.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the action descriptor of the selected action.
        /// </returns>
        public abstract ActionDescriptor SelectAction(RouteContext routeContext, IEnumerable<ControllerActionDescriptor> actionDescriptors);
    }
}
