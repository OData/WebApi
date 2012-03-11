using System.Collections.Generic;
using System.Net.Http;

namespace System.Web.Http.Routing
{
    public interface IHttpRouteConstraint
    {
        bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection);
    }
}
