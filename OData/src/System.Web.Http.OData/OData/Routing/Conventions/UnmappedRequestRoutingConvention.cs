// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that always selects the action named HandleUnmappedRequest if that action is present.
    /// </summary>
    public class UnmappedRequestRoutingConvention : EntitySetRoutingConvention
    {
        private const string UnmappedRequestActionName = "HandleUnmappedRequest";

        /// <summary>
        /// Selects the action.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns></returns>
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

            if (actionMap.Contains(UnmappedRequestActionName))
            {
                return UnmappedRequestActionName;
            }
            return null;
        }
    }
}