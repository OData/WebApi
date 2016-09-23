// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles operating on entities by key.
    /// </summary>
    public class EntityRoutingConvention : NavigationSourceRoutingConvention
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

            if (odataPath.PathTemplate == "~/entityset/key" ||
                odataPath.PathTemplate == "~/entityset/key/cast")
            {
                string httpMethod = request.Method.ToUpperInvariant();
                string httpMethodName;
                switch (httpMethod)
                {
                    case ODataRouteConstants.HttpGet:
                        httpMethodName = "Get";
                        break;

                    case ODataRouteConstants.HttpPut:
                        httpMethodName = "Put";
                        break;

                    case ODataRouteConstants.HttpPatch:
                        httpMethodName = "Patch";
                        break;

                    case ODataRouteConstants.HttpDelete:
                        httpMethodName = "Delete";
                        break;
                    default:
                        return null;
                }

                Contract.Assert(httpMethodName != null);

                IEdmEntityType entityType = (IEdmEntityType)odataPath.EdmType;

                // e.g. Try GetCustomer first, then fallback on Get action name
                ActionDescriptor actionDescriptor = actionDescriptors.FindMatchingAction(
                    httpMethodName + entityType.Name,
                    httpMethodName);

                if (actionDescriptor != null)
                {
                    KeySegment keySegment = (KeySegment)odataPath.Segments[1];
                    routeContext.AddKeyValueToRouteData(keySegment);
                    return actionDescriptor;
                }
            }

            return null;
        }
    }
}
