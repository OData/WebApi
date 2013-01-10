// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
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
        private const string DeleteLinkActionName = "DeleteLink";
        private const string CreateLinkActionName = "CreateLink";

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
            if (odataPath.PathTemplate == "~/entityset/key/$links/navigation")
            {
                if (requestMethod == HttpMethod.Post || requestMethod == HttpMethod.Put)
                {
                    if (actionMap.Contains(CreateLinkActionName))
                    {
                        AddLinkInfoToRouteData(controllerContext.RouteData, odataPath);
                        return CreateLinkActionName;
                    }
                }
                else if (requestMethod == HttpMethod.Delete)
                {
                    if (actionMap.Contains(DeleteLinkActionName))
                    {
                        AddLinkInfoToRouteData(controllerContext.RouteData, odataPath);
                        return DeleteLinkActionName;
                    }
                }
            }
            else if (odataPath.PathTemplate == "~/entityset/key/$links/navigation/key" && requestMethod == HttpMethod.Delete)
            {
                if (actionMap.Contains(DeleteLinkActionName))
                {
                    AddLinkInfoToRouteData(controllerContext.RouteData, odataPath);
                    KeyValuePathSegment relatedKeySegment = odataPath.Segments[4] as KeyValuePathSegment;
                    controllerContext.RouteData.Values[ODataRouteConstants.RelatedKey] = relatedKeySegment.Value;
                    return DeleteLinkActionName;
                }
            }
            return null;
        }

        private static void AddLinkInfoToRouteData(IHttpRouteData routeData, ODataPath odataPath)
        {
            KeyValuePathSegment keyValueSegment = odataPath.Segments[1] as KeyValuePathSegment;
            routeData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
            NavigationPathSegment navigationSegment = odataPath.Segments[3] as NavigationPathSegment;
            routeData.Values[ODataRouteConstants.NavigationProperty] = navigationSegment.NavigationProperty.Name;
        }
    }
}