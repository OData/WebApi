// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Adapters;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation properties.
    /// </summary>
    public partial class NavigationRoutingConvention
    {
        /// <inheritdoc/>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            ValidateSelectActionParameters(odataPath, controllerContext, actionMap);
            return SelectActionImpl(
                odataPath,
                new WebApiControllerContext(controllerContext, GetControllerResult(controllerContext)),
                new WebApiActionMap(actionMap));
        }
    }
}
