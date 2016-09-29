// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles action invocations.
    /// </summary>
    public class ActionRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        public override ActionDescriptor SelectAction(RouteContext routeContext, IEnumerable<ControllerActionDescriptor> actionDescriptors)
        {
            if (routeContext == null)
            {
                throw Error.ArgumentNull("routeContext");
            }

            // TODO: do we need to match the action parameters? to match ODataActionParameters or one by one?

            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;
            string httpMethod = request.Method.ToUpperInvariant();
            if (httpMethod == ODataRouteConstants.HttpPost)
            {
                switch (odataPath.PathTemplate)
                {
                    case "~/entityset/key/cast/action":
                    case "~/entityset/key/action":
                        ControllerActionDescriptor descriptor = GetAction(odataPath).SelectAction(actionDescriptors, isCollection: false);
                        if (descriptor != null)
                        {
                            KeySegment keySegment = (KeySegment)odataPath.Segments[1];
                            routeContext.AddKeyValueToRouteData(keySegment);
                        }
                        return descriptor;

                    case "~/entityset/cast/action":
                    case "~/entityset/action":
                        return GetAction(odataPath).SelectAction(actionDescriptors, isCollection: true);

                    case "~/singleton/action":
                    case "~/singleton/cast/action":
                        return GetAction(odataPath).SelectAction(actionDescriptors, isCollection: false);
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
