using System.Web.Routing;

namespace System.Web.Mvc.Async
{
    public interface IAsyncController : IController
    {
        IAsyncResult BeginExecute(RequestContext requestContext, AsyncCallback callback, object state);
        void EndExecute(IAsyncResult asyncResult);
    }
}
