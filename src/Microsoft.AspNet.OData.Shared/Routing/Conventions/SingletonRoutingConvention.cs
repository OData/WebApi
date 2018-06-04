﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles the singleton.
    /// </summary>
    public partial class SingletonRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext,
            IWebApiActionMap actionMap)
        {
            if (odataPath.PathTemplate == "~/singleton")
            {
                SingletonSegment singletonSegment = (SingletonSegment)odataPath.Segments[0];
                string httpMethodName = GetActionNamePrefix(controllerContext.Request.Method);

                if (httpMethodName != null)
                {
                    // e.g. Try Get{SingletonName} first, then fallback on Get action name
                    return actionMap.FindMatchingAction(
                        httpMethodName + singletonSegment.Singleton.Name,
                        httpMethodName);
                }
            }
            else if (odataPath.PathTemplate == "~/singleton/cast")
            {
                SingletonSegment singletonSegment = (SingletonSegment)odataPath.Segments[0];
                IEdmEntityType entityType = (IEdmEntityType)odataPath.EdmType;
                string httpMethodName = GetActionNamePrefix(controllerContext.Request.Method);

                if (httpMethodName != null)
                {
                    // e.g. Try Get{SingletonName}From{EntityTypeName} first, then fallback on Get action name
                    return actionMap.FindMatchingAction(
                        httpMethodName + singletonSegment.Singleton.Name + "From" + entityType.Name,
                        httpMethodName + "From" + entityType.Name);
                }
            }

            return null;
        }

        private static string GetActionNamePrefix(ODataRequestMethod method)
        {
            string actionNamePrefix;
            switch (method)
            {
                case ODataRequestMethod.Get:
                    actionNamePrefix = "Get";
                    break;
                case ODataRequestMethod.Put:
                    actionNamePrefix = "Put";
                    break;
                case ODataRequestMethod.Patch:
                case ODataRequestMethod.Merge:
                    actionNamePrefix = "Patch";
                    break;
                default:
                    return null;
            }

            return actionNamePrefix;
        }
    }
}
