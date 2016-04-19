using System.Linq;
using System.Web.Http.Controllers;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace WebStack.QA.Test.OData.Common
{
    public class NavigationRoutingConvention2 : EntitySetRoutingConvention
    {
        public override string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if ((odataPath.PathTemplate == "~/entityset/key/navigation") || (odataPath.PathTemplate == "~/entityset/key/cast/navigation"))
            {
                NavigationPropertySegment segment = odataPath.Segments.Last<ODataPathSegment>() as NavigationPropertySegment;
                IEdmNavigationProperty navigationProperty = segment.NavigationProperty;
                IEdmEntityType declaringType = navigationProperty.DeclaringType as IEdmEntityType;
                if (declaringType != null)
                {
                    string prefix = ODataHelper.GetHttpPrefix(controllerContext.Request.Method.ToString());
                    if (string.IsNullOrEmpty(prefix))
                    {
                        return null;
                    }
                    KeySegment segment2 = odataPath.Segments[1] as KeySegment;
                    controllerContext.AddKeyValueToRouteData(segment2);
                    string key = prefix + navigationProperty.Name + "On" + declaringType.Name;
                    return (actionMap.Contains(key) ? key : (prefix + navigationProperty.Name));
                }
            }
            return null;
        }
    }
}
