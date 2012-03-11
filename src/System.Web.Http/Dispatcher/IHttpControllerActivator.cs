using System.Web.Http.Controllers;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Defines the methods that are required for an <see cref="IHttpControllerActivator"/>.
    /// </summary>
    public interface IHttpControllerActivator
    {
        IHttpController Create(HttpControllerContext controllerContext, Type controllerType);
    }
}
