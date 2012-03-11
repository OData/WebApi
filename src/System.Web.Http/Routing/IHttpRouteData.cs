using System.Collections.Generic;

namespace System.Web.Http.Routing
{
    public interface IHttpRouteData
    {
        IHttpRoute Route { get; }

        IDictionary<string, object> Values { get; }
    }
}
