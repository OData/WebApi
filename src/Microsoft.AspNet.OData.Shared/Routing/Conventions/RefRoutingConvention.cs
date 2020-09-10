// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity reference manipulations.
    /// </summary>
    public partial class RefRoutingConvention : NavigationSourceRoutingConvention
    {
        private const string DeleteRefActionNamePrefix = "DeleteRef";
        private const string CreateRefActionNamePrefix = "CreateRef";
        private const string GetRefActionNamePrefix = "GetRef";

        /// <inheritdoc/>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMap actionMap)
        {
            ODataRequestMethod requestMethod = controllerContext.Request.GetRequestMethodOrPreflightMethod();

            if (!IsSupportedRequestMethod(requestMethod))
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

                string refActionName = FindRefActionName(actionMap, navigationProperty, declaringType, requestMethod);
                if (refActionName != null)
                {
                    if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                    {
                        controllerContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments[1]);
                    }

                    controllerContext.RouteData.Add(ODataRouteConstants.NavigationProperty, navigationLinkSegment.NavigationProperty.Name);
                    return refActionName;
                }
            }
            else if ((ODataRequestMethod.Delete == requestMethod) && (
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

                string refActionName = FindRefActionName(actionMap, navigationProperty, declaringType, requestMethod);
                if (refActionName != null)
                {
                    if (odataPath.PathTemplate.StartsWith("~/entityset/key", StringComparison.Ordinal))
                    {
                        controllerContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments[1]);
                    }

                    controllerContext.RouteData.Add(ODataRouteConstants.NavigationProperty, navigationLinkSegment.NavigationProperty.Name);
                    controllerContext.AddKeyValueToRouteData((KeySegment)odataPath.Segments.Last(e => e is KeySegment), ODataRouteConstants.RelatedKey);
                    return refActionName;
                }
            }

            return null;
        }

        private static string FindRefActionName(IWebApiActionMap actionMap,
            IEdmNavigationProperty navigationProperty, IEdmEntityType declaringType, ODataRequestMethod method)
        {
            string actionNamePrefix;
            switch (method)
            {
                case ODataRequestMethod.Delete:
                    actionNamePrefix = DeleteRefActionNamePrefix;
                    break;

                case ODataRequestMethod.Get:
                    actionNamePrefix = GetRefActionNamePrefix;
                    break;

                default:
                    actionNamePrefix = CreateRefActionNamePrefix;
                    break;
            }

            // Examples: CreateRefToOrdersFromCustomer, CreateRefToOrders, CreateRef.
            return actionMap.FindMatchingAction(
                        actionNamePrefix + "To" + navigationProperty.Name + "From" + declaringType.Name,
                        actionNamePrefix + "To" + navigationProperty.Name,
                        actionNamePrefix);
        }

        private static bool IsSupportedRequestMethod(ODataRequestMethod method)
        {
            return (ODataRequestMethod.Delete == method ||
                ODataRequestMethod.Put == method ||
                ODataRequestMethod.Post == method ||
                ODataRequestMethod.Get == method);
        }
    }
}