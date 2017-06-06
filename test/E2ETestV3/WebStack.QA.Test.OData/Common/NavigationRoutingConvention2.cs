using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;

namespace WebStack.QA.Test.OData.Common
{
    public class NavigationRoutingConvention2 : EntitySetRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if ((odataPath.PathTemplate == "~/entityset/key/navigation") || (odataPath.PathTemplate == "~/entityset/key/cast/navigation"))
            {
                NavigationPathSegment segment = odataPath.Segments.Last<ODataPathSegment>() as NavigationPathSegment;
                IEdmNavigationProperty navigationProperty = segment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringType as IEdmEntityType;
                if (declaringType != null)
                {
                    string prefix = ODataHelper.GetHttpPrefix(controllerContext.Request.Method.ToString());
                    if (string.IsNullOrEmpty(prefix))
                    {
                        return null;
                    }
                    KeyValuePathSegment segment2 = odataPath.Segments[1] as KeyValuePathSegment;
                    controllerContext.RouteData.Values.Add(ODataRouteConstants.Key, segment2.Value);
                    string key = prefix + navigationProperty.Name + "On" + declaringType.Name;
                    return (actionMap.Contains(key) ? key : (prefix + navigationProperty.Name));
                }
            }
            return null;
        }
    }
}
