// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.Edm;

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
                switch (odataPath.PathTemplate)
                {
                    case "~/entityset/key/cast/function":
                    case "~/entityset/key/function":
                        actionName = GetFunction(odataPath).SelectAction(actionMap, isCollection: false);
                        if (actionName != null)
                        {
                            controllerContext.AddKeyValueToRouteData(odataPath);
                        }
                        break;
                    case "~/entityset/cast/function":
                    case "~/entityset/function":
                        actionName = GetFunction(odataPath).SelectAction(actionMap, isCollection: true);
                        break;
                    case "~/singleton/function":
                    case "~/singleton/cast/function":
                        actionName = GetFunction(odataPath).SelectAction(actionMap, isCollection: false);
                        break;
                }

                if (actionName != null)
                {
                    controllerContext.AddFunctionParameterToRouteData(odataPath.Segments.Last() as BoundFunctionPathSegment);
                    return actionName;
                }
            }

            return null;
        }

        private static IEdmFunction GetFunction(ODataPath odataPath)
        {
            ODataPathSegment odataSegment = odataPath.Segments.Last();
            IEdmFunction function = null;
            BoundFunctionPathSegment functionSegment = odataSegment as BoundFunctionPathSegment;
            if (functionSegment != null)
            {
                function = functionSegment.Function;
            }

            return function;
        }
    }
}
