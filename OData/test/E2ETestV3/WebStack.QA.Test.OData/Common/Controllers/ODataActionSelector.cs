using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace WebStack.QA.Test.OData.Common.Controllers
{
    /// <summary>
    /// This Action selector works only for routes that map to NavigationProperties.
    /// It allows the "navigationProperty" parameter from the RouteData to be use in the Action name.
    /// 
    /// For all other routes it delegates to the base ApiControllerActionSelector.
    /// </summary>
    public class ODataActionSelector : ApiControllerActionSelector
    {
        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            if (controllerContext.RouteData.Route == controllerContext.Configuration.Routes[ODataRouteNames.PropertyNavigation])
            {
                // remove any trailing () that might have been picked up by the Routing, this avoid us having to 
                // specify two routes, with/without the trailing ().
                string navigationProperty = controllerContext.RouteData.Values["navigationProperty"].ToString();
                navigationProperty = navigationProperty.Replace("()", string.Empty);

                if (controllerContext.Request.Method == HttpMethod.Get)
                {
                    // pick the Action with the name GetNavigationProperty
                    controllerContext.RouteData.Values.Add("Action", String.Format("Get{0}", navigationProperty));
                }
                else if (controllerContext.Request.Method == HttpMethod.Put)
                {
                    controllerContext.RouteData.Values.Add("Action", String.Format("Put{0}", navigationProperty));
                }
                else if (controllerContext.Request.Method == HttpMethod.Post)
                {
                    controllerContext.RouteData.Values.Add("Action", String.Format("Post{0}", navigationProperty));
                }
            }

            return base.SelectAction(controllerContext);
        }
    }
}
