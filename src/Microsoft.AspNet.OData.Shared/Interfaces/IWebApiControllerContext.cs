// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Conventions;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// Contains information for a single HTTP operation.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be exposed publicly; it used for the internal
    /// implementations of SelectControl(). Any design which makes this class public
    /// should find an alternative design.
    /// </remarks>
    internal interface IWebApiControllerContext
    {
        /// <summary>
        /// The selected controller result.
        /// </summary>
        SelectControllerResult ControllerResult { get; }

        /// <summary>
        /// Gets the request.
        /// </summary>
        IWebApiRequestMessage Request { get; }

        /// <summary>
        /// Gets the route data.
        /// </summary>
        IDictionary<string, object> RouteData { get; }
    }
}
