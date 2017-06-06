using System.Web.Http;
using System.Web.OData.Extensions;

namespace WebApiPerformance.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.MapODataServiceRoute(
              routeName: "OData",
              routePrefix: null,
              model: TestRepo.GetModel());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
