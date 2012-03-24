using System.Net.Http;
using System.Web.Http.Controllers;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// Defines the methods that are required for an <see cref="IHttpControllerActivator"/>.
    /// </summary>
    public interface IHttpControllerActivator
    {
        IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType);

        void Release(IHttpController controller, HttpControllerContext controllerContext);
    }
}
