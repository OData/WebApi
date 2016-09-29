// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity reference manipulations.
    /// </summary>
    public class RefRoutingConvention : NavigationSourceRoutingConvention
    {
        private const string DeleteRefActionNamePrefix = "DeleteRef";
        private const string CreateRefActionNamePrefix = "CreateRef";
        private const string GetRefActionNamePrefix = "GetRef";

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

            if (!IsSupportedRequestMethod(httpMethod))
            {
                return null;
            }

            if (odataPath.PathTemplate == "~/entityset/key/navigation/$ref" ||
                odataPath.PathTemplate == "~/entityset/key/cast/navigation/$ref" ||
                odataPath.PathTemplate == "~/singleton/navigation/$ref" ||
                odataPath.PathTemplate == "~/singleton/cast/navigation/$ref")
            {
                NavigationPropertyLinkSegment navigationLinkSegment = (NavigationPropertyLinkSegment)odataPath.Segments.Last();
                IEdmNavigationProperty navigationProperty = navigationLinkSegment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringEntityType();

                ActionDescriptor refActionName = FindRefActionName(actionDescriptors, navigationProperty, declaringType, httpMethod);
                if (refActionName != null)
                {
                    if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                    {
                        routeContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments[1]);
                    }

                    routeContext.RouteData.Values[ODataRouteConstants.NavigationProperty] = navigationLinkSegment.NavigationProperty.Name;
                    return refActionName;
                }
            }
            else if ((httpMethod == ODataRouteConstants.HttpDelete) && (
                odataPath.PathTemplate == "~/entityset/key/navigation/key/$ref" ||
                odataPath.PathTemplate == "~/entityset/key/cast/navigation/key/$ref" ||
                odataPath.PathTemplate == "~/singleton/navigation/key/$ref" ||
                odataPath.PathTemplate == "~/singleton/cast/navigation/key/$ref"))
            {
                // the second key segment is the last segment in the path.
                // So the previous of last segment is the navigation property link segment.
                NavigationPropertyLinkSegment navigationLinkSegment = (NavigationPropertyLinkSegment)odataPath.Segments[odataPath.Segments.Count - 2];
                IEdmNavigationProperty navigationProperty = navigationLinkSegment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringEntityType();

                ActionDescriptor refActionName = FindRefActionName(actionDescriptors, navigationProperty, declaringType, httpMethod);
                if (refActionName != null)
                {
                    if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                    {
                        routeContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments[1]);
                    }

                    routeContext.RouteData.Values[ODataRouteConstants.NavigationProperty] = navigationLinkSegment.NavigationProperty.Name;
                    routeContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments.Last(e => e is KeySegment), ODataRouteConstants.RelatedKey);
                    return refActionName;
                }
            }

            return null;
        }

        private static ActionDescriptor FindRefActionName(IEnumerable<ControllerActionDescriptor> actionDescriptors,
            IEdmNavigationProperty navigationProperty, IEdmEntityType declaringType, string method)
        {
            string actionNamePrefix;
            if (method == ODataRouteConstants.HttpDelete)
            {
                actionNamePrefix = DeleteRefActionNamePrefix;
            }
            else if (method == ODataRouteConstants.HttpGet)
            {
                actionNamePrefix = GetRefActionNamePrefix;
            }
            else
            {
                actionNamePrefix = CreateRefActionNamePrefix;
            }

            // Examples: CreateRefToOrdersFromCustomer, CreateRefToOrders, CreateRef.
            return actionDescriptors.FindMatchingAction(
                        actionNamePrefix + "To" + navigationProperty.Name + "From" + declaringType.Name,
                        actionNamePrefix + "To" + navigationProperty.Name,
                        actionNamePrefix);
        }

        private static bool IsSupportedRequestMethod(string method)
        {
            return (method == ODataRouteConstants.HttpDelete ||
                method == ODataRouteConstants.HttpPut ||
                method == ODataRouteConstants.HttpPost ||
                method == ODataRouteConstants.HttpGet);
        }
    }
}
