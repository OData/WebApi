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
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles function invocations.
    /// </summary>
    public class FunctionRoutingConvention : NavigationSourceRoutingConvention
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
            string httpMethod = request.Method.ToUpperInvariant();

            if (httpMethod == ODataRouteConstants.HttpGet)
            {
                ActionDescriptor actionDescriptor = null;
                OperationSegment function = null;
                switch (odataPath.PathTemplate)
                {
                    case "~/entityset/key/cast/function":
                    case "~/entityset/key/function":
                        function = odataPath.Segments.Last() as OperationSegment;
                        actionDescriptor = GetFunction(function).SelectAction(actionDescriptors, isCollection: false);
                        if (actionDescriptor != null)
                        {
                            routeContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments[1]);
                        }
                        break;

                    case "~/entityset/key/cast/function/$count":
                    case "~/entityset/key/function/$count":
                        function = odataPath.Segments[odataPath.Segments.Count - 2] as OperationSegment;
                        actionDescriptor = GetFunction(function).SelectAction(actionDescriptors, isCollection: false);
                        if (actionDescriptor != null)
                        {
                            routeContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments[1]);
                        }
                        break;

                    case "~/entityset/cast/function":
                    case "~/entityset/function":
                        function = odataPath.Segments.Last() as OperationSegment;
                        actionDescriptor = GetFunction(function).SelectAction(actionDescriptors, isCollection: true);
                        break;

                    case "~/entityset/cast/function/$count":
                    case "~/entityset/function/$count":
                        function = odataPath.Segments[odataPath.Segments.Count - 2] as OperationSegment;
                        actionDescriptor = GetFunction(function).SelectAction(actionDescriptors, isCollection: true);
                        break;

                    case "~/singleton/function":
                    case "~/singleton/cast/function":
                        function = odataPath.Segments.Last() as OperationSegment;
                        actionDescriptor = GetFunction(function).SelectAction(actionDescriptors, isCollection: false);
                        break;

                    case "~/singleton/function/$count":
                    case "~/singleton/cast/function/$count":
                        function = odataPath.Segments[odataPath.Segments.Count - 2] as OperationSegment;
                        actionDescriptor = GetFunction(function).SelectAction(actionDescriptors, isCollection: false);
                        break;
                }
                
                if (actionDescriptor != null)
                {
                    routeContext.AddFunctionParameterToRouteData(function);
                    return actionDescriptor;
                }
            }

            return null;
        }

        private static IEdmFunction GetFunction(OperationSegment segment)
        {
            IEdmFunction function = segment?.Operations.First() as IEdmFunction;
            return function;
        }
    }
}
