// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles function invocations.
    /// </summary>
    public class FunctionRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap)
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

            if (controllerContext.Request.Method == HttpMethod.Get)
            {
                string actionName = null;
                OperationSegment function = null;
                switch (odataPath.PathTemplate)
                {
                    case "~/entityset/key/cast/function":
                    case "~/entityset/key/function":
                        function = odataPath.Segments.Last() as OperationSegment;
                        actionName = GetFunction(function).SelectAction(actionMap, isCollection: false);
                        if (actionName != null)
                        {
                            controllerContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments[1]);
                        }
                        break;
                    case "~/entityset/key/cast/function/$count":
                    case "~/entityset/key/function/$count":
                        function = odataPath.Segments[odataPath.Segments.Count - 2] as OperationSegment;
                        actionName = GetFunction(function).SelectAction(actionMap, isCollection: false);
                        if (actionName != null)
                        {
                            controllerContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments[1]);
                        }
                        break;
                    case "~/entityset/cast/function":
                    case "~/entityset/function":
                        function = odataPath.Segments.Last() as OperationSegment;
                        actionName = GetFunction(function).SelectAction(actionMap, isCollection: true);
                        break;
                    case "~/entityset/cast/function/$count":
                    case "~/entityset/function/$count":
                        function = odataPath.Segments[odataPath.Segments.Count - 2] as OperationSegment;
                        actionName = GetFunction(function).SelectAction(actionMap, isCollection: true);
                        break;
                    case "~/singleton/function":
                    case "~/singleton/cast/function":
                        function = odataPath.Segments.Last() as OperationSegment;
                        actionName = GetFunction(function).SelectAction(actionMap, isCollection: false);
                        break;
                    case "~/singleton/function/$count":
                    case "~/singleton/cast/function/$count":
                        function = odataPath.Segments[odataPath.Segments.Count - 2] as OperationSegment;
                        actionName = GetFunction(function).SelectAction(actionMap, isCollection: false);
                        break;
                }
                
                if (actionName != null)
                {
                    controllerContext.AddFunctionParameterToRouteData(function);
                    return actionName;
                }
            }

            return null;
        }

        private static IEdmFunction GetFunction(OperationSegment segment)
        {
            if (segment != null)
            {
                IEdmFunction function = segment.Operations.First() as IEdmFunction;
                return function;
            }

            return null;
        }
    }
}
