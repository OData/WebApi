using System.Web.Routing;

namespace System.Web.Mvc
{
    public interface IControllerActivator
    {
        IController Create(RequestContext requestContext, Type controllerType);
    }
}
