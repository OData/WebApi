// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
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
                SingletonPathSegment singletonSegment = (SingletonPathSegment)odataPath.Segments[0];
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
                SingletonPathSegment singletonSegment = (SingletonPathSegment)odataPath.Segments[0];
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
