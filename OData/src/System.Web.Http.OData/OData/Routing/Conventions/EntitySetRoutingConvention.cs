// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity sets.
    /// </summary>
    public class EntitySetRoutingConvention : IODataRoutingConvention
    {
        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller
        /// </returns>
        public string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }
            
            EntitySetPathSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetPathSegment;
            if (entitySetSegment != null)
            {
                return entitySetSegment.EntitySet.Name;
            }

            return null;
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
        /// </returns>
        public virtual string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
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

            if (odataPath.PathTemplate == "~/entityset")
            {
                EntitySetPathSegment entitySetSegment = odataPath.Segments[0] as EntitySetPathSegment;
                IEdmEntitySet entitySet = entitySetSegment.EntitySet;
                HttpMethod httpMethod = controllerContext.Request.Method;

                if (httpMethod == HttpMethod.Get)
                {
                    // e.g. Try GetCustomers first, then fallback on Get action name
                    return actionMap.FindMatchingAction(
                        "Get" + entitySet.Name,
                        "Get");
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    // e.g. Try PostCustomer first, then fallback on Post action name
                    return actionMap.FindMatchingAction(
                        "Post" + entitySet.ElementType.Name,
                        "Post");
                }
            }
            return null;
        }
    }
}