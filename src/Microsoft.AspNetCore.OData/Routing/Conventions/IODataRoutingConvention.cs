//-----------------------------------------------------------------------------
// <copyright file="IODataRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Provides an abstraction for selecting a controller and an action for OData requests.
    /// </summary>
    public interface IODataRoutingConvention
    {
        /// <summary>
        /// Selects the controller and action for OData requests.
        /// </summary>
        /// <param name="routeContext">The route context.</param>
        /// <returns>
        /// <c>null</c> if the request isn't handled by this convention;
        ///  otherwise, the <see cref="ActionDescriptor"/> of the selected controller.
        /// </returns>
        IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext);
    }
}
