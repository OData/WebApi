// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity sets.
    /// </summary>
    public class EntitySetRoutingConvention : NavigationSourceRoutingConvention
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

            if (odataPath.PathTemplate == "~/entityset")
            {
                EntitySetPathSegment entitySetSegment = (EntitySetPathSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySetBase;
                HttpMethod httpMethod = controllerContext.Request.Method;

                if (httpMethod == HttpMethod.Get)
                {
                    // e.g. Try GetCustomers first, then fall back to Get action name
                    return actionMap.FindMatchingAction(
                        "Get" + entitySet.Name,
                        "Get");
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    // e.g. Try PostCustomer first, then fall back to Post action name
                    return actionMap.FindMatchingAction(
                        "Post" + entitySet.EntityType().Name,
                        "Post");
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/$count" &&
                controllerContext.Request.Method == HttpMethod.Get)
            {
                EntitySetPathSegment entitySetSegment = (EntitySetPathSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySetBase;

                // e.g. Try GetCustomers first, then fall back to Get action name
                return actionMap.FindMatchingAction(
                    "Get" + entitySet.Name,
                    "Get");
            }
            else if (odataPath.PathTemplate == "~/entityset/cast")
            {
                EntitySetPathSegment entitySetSegment = (EntitySetPathSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySetBase;
                IEdmCollectionType collectionType = (IEdmCollectionType)odataPath.EdmType;
                IEdmEntityType entityType = (IEdmEntityType)collectionType.ElementType.Definition;
                HttpMethod httpMethod = controllerContext.Request.Method;

                if (httpMethod == HttpMethod.Get)
                {
                    // e.g. Try GetCustomersFromSpecialCustomer first, then fall back to GetFromSpecialCustomer
                    return actionMap.FindMatchingAction(
                        "Get" + entitySet.Name + "From" + entityType.Name,
                        "GetFrom" + entityType.Name);
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    // e.g. Try PostCustomerFromSpecialCustomer first, then fall back to PostFromSpecialCustomer
                    return actionMap.FindMatchingAction(
                        "Post" + entitySet.EntityType().Name + "From" + entityType.Name,
                        "PostFrom" + entityType.Name);
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/cast/$count" &&
                controllerContext.Request.Method == HttpMethod.Get)
            {
                EntitySetPathSegment entitySetSegment = (EntitySetPathSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySetBase;
                IEdmCollectionType collectionType = (IEdmCollectionType)odataPath.Segments[1].GetEdmType(
                    entitySetSegment.GetEdmType(previousEdmType: null));
                IEdmEntityType entityType = (IEdmEntityType)collectionType.ElementType.Definition;

                // e.g. Try GetCustomersFromSpecialCustomer first, then fall back to GetFromSpecialCustomer
                return actionMap.FindMatchingAction(
                    "Get" + entitySet.Name + "From" + entityType.Name,
                    "GetFrom" + entityType.Name);
            }

            return null;
        }
    }
}
