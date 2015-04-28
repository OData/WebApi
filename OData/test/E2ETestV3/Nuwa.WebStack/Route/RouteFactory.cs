using System.Configuration;

namespace Nuwa.WebStack.Route
{
    public class RouteFactory : IRouteFactory
    {
        public string RouteTemplate
        {
            get
            {
                var retval = "api/{controller}/{action}";

                var routesInConfig = ConfigurationManager.AppSettings["default_route"];
                if (!string.IsNullOrEmpty(routesInConfig))
                {
                    retval = routesInConfig;
                }

                return retval;
            }
        }
    }
}
