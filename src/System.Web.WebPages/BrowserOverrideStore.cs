namespace System.Web.WebPages
{
    /// <summary>
    /// The current BrowserOverrideStore is used to get and set the user agent of a request.
    /// For an example see CookieBasedBrowserOverrideStore.
    /// </summary>
    public abstract class BrowserOverrideStore
    {
        public abstract string GetOverriddenUserAgent(HttpContextBase httpContext);
        public abstract void SetOverriddenUserAgent(HttpContextBase httpContext, string userAgent);
    }
}
