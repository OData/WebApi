using System.Web.Routing;

namespace System.Web.Mvc
{
    public interface IController
    {
        void Execute(RequestContext requestContext);
    }
}
