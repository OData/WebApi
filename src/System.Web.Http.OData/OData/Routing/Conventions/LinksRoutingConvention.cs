// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles link manipulations.
    /// </summary>
    public class LinksRoutingConvention : EntitySetRoutingConvention
    {
        private const string DeleteLinkActionNamePrefix = "DeleteLink";
        private const string CreateLinkActionNamePrefix = "CreateLink";

        /// <summary>
        /// Selects the action.
        /// </summary>
        /// <param name="odataPath">The odata path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns></returns>
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

            if (odataPath.PathTemplate == "~/entityset/key/$links/navigation" ||
                odataPath.PathTemplate == "~/entityset/key/cast/$links/navigation")
            {
                NavigationPathSegment navigationSegment = odataPath.Segments.Last() as NavigationPathSegment;
                IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringType as IEdmEntityType;

                string linksActionName = FindLinksActionName(actionMap, navigationProperty, declaringType, requestMethod);
                if (linksActionName != null)
                {
                    routeData.Values[ODataRouteConstants.Key] = (odataPath.Segments[1] as KeyValuePathSegment).Value;
                    routeData.Values[ODataRouteConstants.NavigationProperty] = navigationSegment.NavigationProperty.Name;
                    return linksActionName;
                }
            }
            else if ((odataPath.PathTemplate == "~/entityset/key/$links/navigation/key" ||
                odataPath.PathTemplate == "~/entityset/key/cast/$links/navigation/key") && requestMethod == HttpMethod.Delete)
            {
                NavigationPathSegment navigationSegment = odataPath.Segments[odataPath.Segments.Count - 2] as NavigationPathSegment;
                IEdmNavigationProperty navigationProperty = navigationSegment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringType as IEdmEntityType;

                string linksActionName = FindLinksActionName(actionMap, navigationProperty, declaringType, requestMethod);
                if (linksActionName != null)
                {
                    routeData.Values[ODataRouteConstants.Key] = (odataPath.Segments[1] as KeyValuePathSegment).Value;
                    routeData.Values[ODataRouteConstants.NavigationProperty] = navigationSegment.NavigationProperty.Name;
                    routeData.Values[ODataRouteConstants.RelatedKey] = (odataPath.Segments.Last() as KeyValuePathSegment).Value;
                    return linksActionName;
                }
            }

            return null;
        }

        private static string FindLinksActionName(ILookup<string, HttpActionDescriptor> actionMap,
            IEdmNavigationProperty navigationProperty, IEdmEntityType declaringType, HttpMethod method)
        {
            string actionNamePrefix;
            if (method == HttpMethod.Delete)
            {
                actionNamePrefix = DeleteLinkActionNamePrefix;
            }
            else
            {
                actionNamePrefix = CreateLinkActionNamePrefix;
            }

            return actionMap.FindMatchingAction(
                        actionNamePrefix + "To" + navigationProperty.Name + "From" + declaringType.Name,
                        actionNamePrefix + "To" + navigationProperty.Name,
                        actionNamePrefix);
        }
    }
}