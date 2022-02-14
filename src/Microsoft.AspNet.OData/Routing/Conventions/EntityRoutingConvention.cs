//-----------------------------------------------------------------------------
// <copyright file="EntityRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Adapters;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles operating on entities by key.
    /// </summary>
    public partial class EntityRoutingConvention
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
