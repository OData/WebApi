using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;

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
            if (odataPath.PathTemplate == "~/entityset/key/navigation/$ref"
                || odataPath.PathTemplate == "~/entityset/key/cast/navigation/$ref"
                || odataPath.PathTemplate == "~/entityset/key/navigation/key/$ref"
                || odataPath.PathTemplate == "~/entityset/key/cast/navigation/key/$ref")
            {
                var actionName = string.Empty;
                if ((requestMethod == HttpMethod.Post) || (requestMethod == HttpMethod.Put))
                {
                    actionName += "CreateRefTo";
                }
                else if (requestMethod == HttpMethod.Delete)
                {
                    actionName += "DeleteRefTo";
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
