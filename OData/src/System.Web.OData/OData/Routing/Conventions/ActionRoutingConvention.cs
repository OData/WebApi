// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles action invocations.
    /// </summary>
    public class ActionRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
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

            if (controllerContext.Request.Method == HttpMethod.Post)
            {
                switch (odataPath.PathTemplate)
                {
                    case "~/entityset/key/cast/action":
                    case "~/entityset/key/action":
                        string actionName = GetAction(odataPath).SelectAction(actionMap, isCollection: false);
                        if (actionName != null)
                        {
                            controllerContext.AddKeyValueToRouteData(odataPath);
                        }
                        return actionName;
                    case "~/entityset/cast/action":
                    case "~/entityset/action":
                        return GetAction(odataPath).SelectAction(actionMap, isCollection: true);
                    case "~/singleton/action":
                    case "~/singleton/cast/action":
                        return GetAction(odataPath).SelectAction(actionMap, isCollection: false);
                }
            }

            return null;
        }

        private static IEdmAction GetAction(ODataPath odataPath)
        {
            ODataPathSegment odataSegment = odataPath.Segments.Last();
            IEdmAction action = null;
            BoundActionPathSegment actionSegment = odataSegment as BoundActionPathSegment;
            if (actionSegment != null)
            {
                action = actionSegment.Action;
            }

            return action;
        }
    }
}
