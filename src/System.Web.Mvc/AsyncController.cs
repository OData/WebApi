namespace System.Web.Mvc
{
    // Controller now supports asynchronous operations.
    // This class only exists 
    // a) for backwards compat for callers that derive from it,
    // b) ActionMethodSelector can detect it to bind to ActionAsync/ActionCompleted patterns. 
    public abstract class AsyncController : Controller
    {
    }
}
