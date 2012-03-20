using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    internal static class HttpRouteDataExtensions
    {
        public static RouteData ToRouteData(this IHttpRouteData httpRouteData)
        {
            if (httpRouteData == null)
            {
                throw Error.ArgumentNull("httpRouteData");
            }

            HostedHttpRouteData hostedHttpRouteData = httpRouteData as HostedHttpRouteData;
            if (hostedHttpRouteData != null)
            {
                return hostedHttpRouteData.OriginalRouteData;
            }

            Route route = httpRouteData.Route.ToRoute();
            return new RouteData(route, HttpControllerRouteHandler.Instance);
        }
    }
}
