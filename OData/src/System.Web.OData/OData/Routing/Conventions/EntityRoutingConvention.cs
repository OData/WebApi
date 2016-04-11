// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles operating on entities by key.
    /// </summary>
    public class EntityRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <inheritdoc/>
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
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

            if (odataPath.PathTemplate == "~/entityset/key" ||
                odataPath.PathTemplate == "~/entityset/key/cast")
            {
                HttpMethod httpMethod = controllerContext.Request.Method;
                string httpMethodName;

                switch (httpMethod.ToString().ToUpperInvariant())
                {
                    case "GET":
                        httpMethodName = "Get";
                        break;
                    case "PUT":
                        httpMethodName = "Put";
                        break;
                    case "PATCH":
                    case "MERGE":
                        httpMethodName = "Patch";
                        break;
                    case "DELETE":
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
