using System.Web.WebPages.Resources;

namespace System.Web.Helpers
{
    public static class AntiForgery
    {
        private static readonly AntiForgeryWorker _worker = new AntiForgeryWorker();

        public static HtmlString GetHtml()
        {
            if (HttpContext.Current == null)
            {
                throw new ArgumentException(WebPageResources.HttpContextUnavailable);
            }

            return GetHtml(new HttpContextWrapper(HttpContext.Current), salt: null, domain: null, path: null);
        }

        public static HtmlString GetHtml(HttpContextBase httpContext, string salt, string domain, string path)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            return _worker.GetHtml(httpContext, salt, domain, path);
        }

        public static void Validate()
        {
            if (HttpContext.Current == null)
            {
                throw new ArgumentException(WebPageResources.HttpContextUnavailable);
            }
            Validate(new HttpContextWrapper(HttpContext.Current), salt: null);
        }

        public static void Validate(HttpContextBase httpContext, string salt)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            _worker.Validate(httpContext, salt);
        }
    }
}
