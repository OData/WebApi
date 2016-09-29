// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity sets.
    /// </summary>
    public class EntitySetRoutingConvention : NavigationSourceRoutingConvention
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

            if (odataPath.PathTemplate == "~/entityset")
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;

                if (httpMethod == ODataRouteConstants.HttpGet)
                {
                    // e.g. Try GetCustomers first, then fall back to Get action name
                    return actionDescriptors.FindMatchingAction(
                        "Get" + entitySet.Name,
                        "Get");
                }
                else if (httpMethod == ODataRouteConstants.HttpPost)
                {
                    // e.g. Try PostCustomer first, then fall back to Post action name
                    return actionDescriptors.FindMatchingAction(
                        "Post" + entitySet.EntityType().Name,
                        "Post");
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/$count" &&
                httpMethod == ODataRouteConstants.HttpGet)
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;

                // e.g. Try GetCustomers first, then fall back to Get action name
                return actionDescriptors.FindMatchingAction(
                    "Get" + entitySet.Name,
                    "Get");
            }
            else if (odataPath.PathTemplate == "~/entityset/cast")
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;
                IEdmCollectionType collectionType = (IEdmCollectionType)odataPath.EdmType;
                IEdmEntityType entityType = (IEdmEntityType)collectionType.ElementType.Definition;

                if (httpMethod == ODataRouteConstants.HttpGet)
                {
                    // e.g. Try GetCustomersFromSpecialCustomer first, then fall back to GetFromSpecialCustomer
                    return actionDescriptors.FindMatchingAction(
                        "Get" + entitySet.Name + "From" + entityType.Name,
                        "GetFrom" + entityType.Name);
                }
                else if (httpMethod == ODataRouteConstants.HttpPost)
                {
                    // e.g. Try PostCustomerFromSpecialCustomer first, then fall back to PostFromSpecialCustomer
                    return actionDescriptors.FindMatchingAction(
                        "Post" + entitySet.EntityType().Name + "From" + entityType.Name,
                        "PostFrom" + entityType.Name);
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/cast/$count" &&
                httpMethod == ODataRouteConstants.HttpGet)
            {
                EntitySetSegment entitySetSegment = (EntitySetSegment)odataPath.Segments[0];
                IEdmEntitySetBase entitySet = entitySetSegment.EntitySet;
                IEdmCollectionType collectionType = (IEdmCollectionType)odataPath.Segments[1].EdmType;
                IEdmEntityType entityType = (IEdmEntityType)collectionType.ElementType.Definition;

                // e.g. Try GetCustomersFromSpecialCustomer first, then fall back to GetFromSpecialCustomer
                return actionDescriptors.FindMatchingAction(
                    "Get" + entitySet.Name + "From" + entityType.Name,
                    "GetFrom" + entityType.Name);
            }

            return null;
        }
    }
}
