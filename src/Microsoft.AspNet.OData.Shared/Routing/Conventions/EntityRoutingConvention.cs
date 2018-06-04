﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles operating on entities by key.
    /// </summary>
    public partial class EntityRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMap actionMap)
        {
            if (odataPath.PathTemplate == "~/entityset/key" ||
                odataPath.PathTemplate == "~/entityset/key/cast")
            {
                string httpMethodName;

                switch (controllerContext.Request.Method)
                {
                    case ODataRequestMethod.Get:
                        httpMethodName = "Get";
                        break;
                    case ODataRequestMethod.Put:
                        httpMethodName = "Put";
                        break;
                    case ODataRequestMethod.Patch:
                    case ODataRequestMethod.Merge:
                        httpMethodName = "Patch";
                        break;
                    case ODataRequestMethod.Delete:
                        httpMethodName = "Delete";
                        break;
                    default:
                        return null;
                }

                Contract.Assert(httpMethodName != null);

                IEdmEntityType entityType = (IEdmEntityType)odataPath.EdmType;

                // e.g. Try GetCustomer first, then fallback on Get action name
                string actionName = actionMap.FindMatchingAction(
                    httpMethodName + entityType.Name,
                    httpMethodName);

                if (actionName != null)
                {
                    KeySegment keySegment = (KeySegment)odataPath.Segments[1];
                    controllerContext.AddKeyValueToRouteData(keySegment);
                    return actionName;
                }
            }
            return null;
        }
    }
}
