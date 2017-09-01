// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles function invocations.
    /// </summary>
    public partial class FunctionRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity",
            Justification = "These are simple conversion function and cannot be split up.")]
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
