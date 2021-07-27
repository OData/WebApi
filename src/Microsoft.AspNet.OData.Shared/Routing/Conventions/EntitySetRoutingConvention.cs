// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity sets.
    /// </summary>
    public partial class EntitySetRoutingConvention : NavigationSourceRoutingConvention
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
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMap actionMap)
        {
            if (odataPath.PathTemplate == "~/entityset")
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;

                if (ODataRequestMethod.Get == controllerContext.Request.GetRequestMethodOrPreflightMethod())
                {
                    // e.g. Try GetCustomers first, then fall back to Get action name
                    return actionMap.FindMatchingAction(
                        "Get" + entitySet.Name,
                        "Get");
                }
                else if (ODataRequestMethod.Post == controllerContext.Request.GetRequestMethodOrPreflightMethod())
                {
                    // e.g. Try PostCustomer first, then fall back to Post action name
                    return actionMap.FindMatchingAction(
                        "Post" + entitySet.EntityType().Name,
                        "Post");
                }
                else if (ODataRequestMethod.Patch == controllerContext.Request.Method)
                {
                    // e.g. Try PatchCustomers first, then fall back to Patch action name
                    return actionMap.FindMatchingAction(
                        "Patch" + entitySet.Name,
                        "Patch");
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/$count" &&
                ODataRequestMethod.Get == controllerContext.Request.GetRequestMethodOrPreflightMethod())
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;

                // e.g. Try GetCustomers first, then fall back to Get action name
                return actionMap.FindMatchingAction(
                    "Get" + entitySet.Name,
                    "Get");
            }
            else if (odataPath.PathTemplate == "~/entityset/cast")
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;
                IEdmCollectionType collectionType = (IEdmCollectionType)odataPath.EdmType;
                IEdmEntityType entityType = (IEdmEntityType)collectionType.ElementType.Definition;

                if (ODataRequestMethod.Get == controllerContext.Request.GetRequestMethodOrPreflightMethod())
                {
                    // e.g. Try GetCustomersFromSpecialCustomer first, then fall back to GetFromSpecialCustomer
                    return actionMap.FindMatchingAction(
                        "Get" + entitySet.Name + "From" + entityType.Name,
                        "GetFrom" + entityType.Name);
                }
                else if (ODataRequestMethod.Post == controllerContext.Request.GetRequestMethodOrPreflightMethod())
                {
                    // e.g. Try PostCustomerFromSpecialCustomer first, then fall back to PostFromSpecialCustomer
                    return actionMap.FindMatchingAction(
                        "Post" + entitySet.EntityType().Name + "From" + entityType.Name,
                        "PostFrom" + entityType.Name);
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/cast/$count" &&
                ODataRequestMethod.Get == controllerContext.Request.GetRequestMethodOrPreflightMethod())
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;
                IEdmCollectionType collectionType = (IEdmCollectionType)odataPath.Segments[1].EdmType;
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
