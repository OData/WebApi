// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
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
        ActionDescriptor SelectAction(RouteContext routeContext);
    }
}
