using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using System.Web.Http.Routing;

namespace WebStack.QA.Test.OData.Common
{
    public class LinkRoutingConvention2 : EntitySetRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath == null)
            {
                throw new ArgumentNullException("odataPath");
            }
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (actionMap == null)
            {
                throw new ArgumentNullException("actionMap");
            }
            HttpMethod requestMethod = controllerContext.Request.Method;
            if (odataPath.PathTemplate == "~/entityset/key/$links/navigation" 
                || odataPath.PathTemplate == "~/entityset/key/cast/$links/navigation"
                || odataPath.PathTemplate == "~/entityset/key/$links/navigation/key"
                || odataPath.PathTemplate == "~/entityset/key/cast/$links/navigation/key")
            {
                var actionName = string.Empty;
                if ((requestMethod == HttpMethod.Post) || (requestMethod == HttpMethod.Put))
                {
                    actionName += "CreateLinkTo";
                }
                else if (requestMethod == HttpMethod.Delete)
                {
                    actionName += "DeleteLinkTo";
                }
                else
                {
                    return null;
                }
                var navigationSegment = odataPath.Segments.OfType<NavigationPathSegment>().Last();
                actionName += navigationSegment.NavigationPropertyName;

                var castSegment = odataPath.Segments[2] as CastPathSegment;
                if (castSegment != null)
                {
                    var actionCastName = string.Format("{0}On{1}", actionName, castSegment.CastType.Name);
                    if (actionMap.Contains(actionCastName))
                    {
                        AddLinkInfoToRouteData(controllerContext.RouteData, odataPath);
                        return actionCastName;
                    }
                }

                if (actionMap.Contains(actionName))
                {
                    AddLinkInfoToRouteData(controllerContext.RouteData, odataPath);
                    return actionName;
                }
            }
            return null;
        }

        private static void AddLinkInfoToRouteData(IHttpRouteData routeData, ODataPath odataPath)
        {
            KeyValuePathSegment keyValueSegment = odataPath.Segments.OfType<KeyValuePathSegment>().First();
            routeData.Values[ODataRouteConstants.Key] = keyValueSegment.Value;
            KeyValuePathSegment relatedKeySegment = odataPath.Segments.Last() as KeyValuePathSegment;
            if (relatedKeySegment != null)
            {
                routeData.Values[ODataRouteConstants.RelatedKey] = relatedKeySegment.Value;
            }
        }
    }
}
