namespace System.Web.Mvc.Async
{
    public interface IAsyncActionInvoker : IActionInvoker
    {
        IAsyncResult BeginInvokeAction(ControllerContext controllerContext, string actionName, AsyncCallback callback, object state);
        bool EndInvokeAction(IAsyncResult asyncResult);
    }
}
