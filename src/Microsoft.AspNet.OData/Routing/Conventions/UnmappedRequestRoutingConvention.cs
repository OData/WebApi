// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Adapters;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that always selects the action named HandleUnmappedRequest if that action is present.
    /// </summary>
    public partial class UnmappedRequestRoutingConvention
    {
        /// <inheritdoc/>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            ValidateSelectActionParameters(odataPath, controllerContext, actionMap);
            return SelectActionImpl(new WebApiActionMap(actionMap));
        }
    }
}
