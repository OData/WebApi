namespace System.Web.Helpers.AntiXsrf
{
    // Provides an abstraction around how tokens are persisted and retrieved for a request
    internal interface ITokenStore
    {
        AntiForgeryToken GetCookieToken(HttpContextBase httpContext);
        AntiForgeryToken GetFormToken(HttpContextBase httpContext);
        void SaveCookieToken(HttpContextBase httpContext, AntiForgeryToken token);
    }
}
