// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using System;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles function invocations.
    /// </summary>
    public partial class FunctionRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        internal override string SelectAction(RouteContext routeContext, SelectControllerResult controllerResult, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            throw new NotImplementedException();
        }
    }
}
