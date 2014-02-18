// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles entity reference manipulations.
    /// </summary>
    public class RefRoutingConvention : EntitySetRoutingConvention
    {
        private const string DeleteRefActionNamePrefix = "DeleteRef";
        private const string CreateRefActionNamePrefix = "CreateRef";

        private const string EntitysetKeyNavigation =
            ODataSegmentKinds._ServiceBase + "/" + ODataSegmentKinds._EntitySet + "/" + ODataSegmentKinds._Key + "/"
            + ODataSegmentKinds._Navigation;

        private const string EntitysetKeyCastNavigation =
            ODataSegmentKinds._ServiceBase + "/" + ODataSegmentKinds._EntitySet + "/" + ODataSegmentKinds._Key + "/"
            + ODataSegmentKinds._Cast + "/" + ODataSegmentKinds._Navigation;

        // "~/entityset/key/navigation/$ref"
        private const string EntitysetKeyNavigationRef = EntitysetKeyNavigation + "/" + ODataSegmentKinds._Ref;

        // "~/entityset/key/cast/navigation/$ref"
        private const string EntitysetKeyCastNavigationRef = EntitysetKeyCastNavigation + "/" + ODataSegmentKinds._Ref;

        // "~/entityset/key/navigation/key/$ref"
        private const string EntitysetKeyNavigationKeyRef =
            EntitysetKeyNavigation + "/" + ODataSegmentKinds._Key + "/" + ODataSegmentKinds._Ref;

        // "~/entityset/key/cast/navigation/key/$ref"
        private const string EntitysetKeyCastNavigationKeyRef =
            EntitysetKeyCastNavigation + "/" + ODataSegmentKinds._Key + "/" + ODataSegmentKinds._Ref;

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

            HttpMethod requestMethod = controllerContext.Request.Method;
            IHttpRouteData routeData = controllerContext.RouteData;

            if (odataPath.PathTemplate == EntitysetKeyNavigationRef ||
                odataPath.PathTemplate == EntitysetKeyCastNavigationRef)
            {
                NavigationPathSegment navigationSegment = (NavigationPathSegment)odataPath.Segments[odataPath.Segments.Count - 2];
                IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringEntityType();

                string refActionName = FindRefActionName(actionMap, navigationProperty, declaringType, requestMethod);
                if (refActionName != null)
                {
                    routeData.Values[ODataRouteConstants.Key] = ((KeyValuePathSegment)odataPath.Segments[1]).Value;
                    routeData.Values[ODataRouteConstants.NavigationProperty] = navigationSegment.NavigationProperty.Name;
                    return refActionName;
                }
            }
            else if ((odataPath.PathTemplate == EntitysetKeyNavigationKeyRef ||
                odataPath.PathTemplate == EntitysetKeyCastNavigationKeyRef) && requestMethod == HttpMethod.Delete)
            {
                NavigationPathSegment navigationSegment = (NavigationPathSegment)odataPath.Segments[odataPath.Segments.Count - 3];
                IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringEntityType();

                string refActionName = FindRefActionName(actionMap, navigationProperty, declaringType, requestMethod);
                if (refActionName != null)
                {
                    routeData.Values[ODataRouteConstants.Key] = ((KeyValuePathSegment)odataPath.Segments[1]).Value;
                    routeData.Values[ODataRouteConstants.NavigationProperty] = navigationSegment.NavigationProperty.Name;
                    routeData.Values[ODataRouteConstants.RelatedKey] = ((KeyValuePathSegment)odataPath.Segments[odataPath.Segments.Count - 2]).Value;
                    return refActionName;
                }
            }

            return null;
        }

        private static string FindRefActionName(ILookup<string, HttpActionDescriptor> actionMap,
            IEdmNavigationProperty navigationProperty, IEdmEntityType declaringType, HttpMethod method)
        {
            string actionNamePrefix;
            if (method == HttpMethod.Delete)
            {
                actionNamePrefix = DeleteRefActionNamePrefix;
            }
            else
            {
                actionNamePrefix = CreateRefActionNamePrefix;
            }

            // Examples: CreateRefToOrdersFromCustomer, CreateRefToOrders, CreateRef.
            return actionMap.FindMatchingAction(
                        actionNamePrefix + "To" + navigationProperty.Name + "From" + declaringType.Name,
                        actionNamePrefix + "To" + navigationProperty.Name,
                        actionNamePrefix);
        }
    }
}