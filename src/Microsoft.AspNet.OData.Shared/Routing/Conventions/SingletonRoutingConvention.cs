// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles the singleton.
    /// </summary>
    public class SingletonRoutingConvention : NavigationSourceRoutingConvention
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

        private static string GetActionNamePrefix(HttpMethod method)
        {
            string actionNamePrefix;
            switch (method.Method.ToUpperInvariant())
            {
                case "GET":
                    actionNamePrefix = "Get";
                    break;
                case "PUT":
                    actionNamePrefix = "Put";
                    break;
                case "PATCH":
                case "MERGE":
                    actionNamePrefix = "Patch";
                    break;
                default:
                    return null;
            }

            return actionNamePrefix;
        }
    }
}
