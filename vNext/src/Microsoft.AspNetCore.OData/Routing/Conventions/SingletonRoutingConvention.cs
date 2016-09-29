// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles the singleton.
    /// </summary>
    public class SingletonRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        public override ActionDescriptor SelectAction(RouteContext routeContext, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            ODataPath odataPath = routeContext.HttpContext.Request.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;

            if (odataPath.PathTemplate == "~/singleton")
            {
                SingletonSegment singletonSegment = (SingletonSegment)odataPath.Segments[0];
                string httpMethodName = GetActionNamePrefix(request.Method);

                if (httpMethodName != null)
                {
                    // e.g. Try Get{SingletonName} first, then fallback on Get action name
                    return actionDescriptors.FindMatchingAction(
                        httpMethodName + singletonSegment.Singleton.Name,
                        httpMethodName);
                }
            }
            else if (odataPath.PathTemplate == "~/singleton/cast")
            {
                SingletonSegment singletonSegment = (SingletonSegment)odataPath.Segments[0];
                IEdmEntityType entityType = (IEdmEntityType)odataPath.EdmType;
                string httpMethodName = GetActionNamePrefix(request.Method);

                if (httpMethodName != null)
                {
                    // e.g. Try Get{SingletonName}From{EntityTypeName} first, then fallback on Get action name
                    return actionDescriptors.FindMatchingAction(
                        httpMethodName + singletonSegment.Singleton.Name + "From" + entityType.Name,
                        httpMethodName + "From" + entityType.Name);
                }
            }

            return null;
        }

        private static string GetActionNamePrefix(string method)
        {
            string actionNamePrefix;
            switch (method.ToUpperInvariant())
            {
                case ODataRouteConstants.HttpGet:
                    actionNamePrefix = "Get";
                    break;

                case ODataRouteConstants.HttpPut:
                    actionNamePrefix = "Put";
                    break;

                case ODataRouteConstants.HttpPatch:
                    actionNamePrefix = "Patch";
                    break;

                default:
                    return null;
            }

            return actionNamePrefix;
        }
    }
}
