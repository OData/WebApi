// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.Edm;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles action invocations.
    /// </summary>
    public class ActionRoutingConvention : EntitySetRoutingConvention
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
                }
            }

            return null;
        }

        private static IEdmActionImport GetAction(ODataPath odataPath)
        {
            ODataPathSegment odataSegment = odataPath.Segments.Last();
            IEdmActionImport action = null;
            ActionPathSegment actionSegment = odataSegment as ActionPathSegment;
            if (actionSegment != null)
            {
                action = actionSegment.Action;
            }

            return action;
        }
    }
}