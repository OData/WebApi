// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity sets.
    /// </summary>
    public partial class EntitySetRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
        /// </returns>
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (actionMap == null)
            {
                throw Error.ArgumentNull("actionMap");
            }

            return SelectActionImpl(
                odataPath,
                new WebApiControllerContext(controllerContext, GetControllerResult(controllerContext)),
                new WebApiActionMap(actionMap));
        }
    }
}
