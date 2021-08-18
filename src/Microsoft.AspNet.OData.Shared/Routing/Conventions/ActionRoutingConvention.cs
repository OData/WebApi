//-----------------------------------------------------------------------------
// <copyright file="ActionRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles action invocations.
    /// </summary>
    public partial class ActionRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMap actionMap)
        {
            if (ODataRequestMethod.Post == controllerContext.Request.GetRequestMethodOrPreflightMethod())
            {
                switch (odataPath.PathTemplate)
                {
                    case "~/entityset/key/cast/action":
                    case "~/entityset/key/action":
                        string actionName = GetAction(odataPath).SelectAction(actionMap, isCollection: false);
                        if (actionName != null)
                        {
                            KeySegment keySegment = (KeySegment)odataPath.Segments[1];
                            controllerContext.AddKeyValueToRouteData(keySegment);
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
            OperationSegment actionSegment = odataSegment as OperationSegment;
            if (actionSegment != null)
            {
                action = actionSegment.Operations.First() as IEdmAction;
            }

            return action;
        }
    }
}
