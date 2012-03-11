namespace System.Web.Mvc
{
    public interface IViewLocationCache
    {
        string GetViewLocation(HttpContextBase httpContext, string key);
        void InsertViewLocation(HttpContextBase httpContext, string key, string virtualPath);
    }
}
