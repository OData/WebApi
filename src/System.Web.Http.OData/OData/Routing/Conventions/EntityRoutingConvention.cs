// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles operating on entities by key.
    /// </summary>
    public class EntityRoutingConvention : EntitySetRoutingConvention
    {
        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
        /// </returns>
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

                IEdmEntityType entityType = odataPath.EdmType as IEdmEntityType;

                // e.g. Try GetCustomer first, then fallback on Get action name
                string actionName = actionMap.FindMatchingAction(
                    httpMethodName + entityType.Name,
                    httpMethodName);

                if (actionName != null)
                {
                    KeyValuePathSegment keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
                    controllerContext.RouteData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
                    return actionName;
                }
            }
            return null;
        }
    }
}