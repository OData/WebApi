using System.Diagnostics;
using System.Web.Mvc;
using System.Web.WebPages.Resources;

namespace System.Web.Helpers
{
    internal class AntiForgeryWorker
    {
        public AntiForgeryWorker()
        {
            Serializer = new AntiForgeryDataSerializer();
        }

        internal AntiForgeryDataSerializer Serializer { get; set; }

        private static HttpAntiForgeryException CreateValidationException()
        {
            return new HttpAntiForgeryException(WebPageResources.AntiForgeryToken_ValidationFailed);
        }

        public HtmlString GetHtml(HttpContextBase httpContext, string salt, string domain, string path)
        {
            Debug.Assert(httpContext != null);

            string formValue = GetAntiForgeryTokenAndSetCookie(httpContext, salt, domain, path);
            string fieldName = AntiForgeryData.GetAntiForgeryTokenName(null);

            TagBuilder builder = new TagBuilder("input");
            builder.Attributes["type"] = "hidden";
            builder.Attributes["name"] = fieldName;
            builder.Attributes["value"] = formValue;
            return new HtmlString(builder.ToString(TagRenderMode.SelfClosing));
        }

        private string GetAntiForgeryTokenAndSetCookie(HttpContextBase httpContext, string salt, string domain, string path)
        {
            string cookieName = AntiForgeryData.GetAntiForgeryTokenName(httpContext.Request.ApplicationPath);

            AntiForgeryData cookieToken = null;
            HttpCookie cookie = httpContext.Request.Cookies[cookieName];
            if (cookie != null)
            {
                try
                {
                    cookieToken = Serializer.Deserialize(cookie.Value);
                }
                catch (HttpAntiForgeryException)
                {
                }
            }

            if (cookieToken == null)
            {
                cookieToken = AntiForgeryData.NewToken();
                string cookieValue = Serializer.Serialize(cookieToken);

                HttpCookie newCookie = new HttpCookie(cookieName, cookieValue) { HttpOnly = true, Domain = domain };
                if (!String.IsNullOrEmpty(path))
                {
                    newCookie.Path = path;
                }
                httpContext.Response.Cookies.Set(newCookie);
            }

            AntiForgeryData formToken = new AntiForgeryData(cookieToken)
            {
                Salt = salt,
                Username = AntiForgeryData.GetUsername(httpContext.User)
            };
            return Serializer.Serialize(formToken);
        }

        public void Validate(HttpContextBase context, string salt)
        {
            Debug.Assert(context != null);

            string fieldName = AntiForgeryData.GetAntiForgeryTokenName(null);
            string cookieName = AntiForgeryData.GetAntiForgeryTokenName(context.Request.ApplicationPath);

            HttpCookie cookie = context.Request.Cookies[cookieName];
            if (cookie == null || String.IsNullOrEmpty(cookie.Value))
            {
                // error: cookie token is missing
                throw CreateValidationException();
            }
            AntiForgeryData cookieToken = Serializer.Deserialize(cookie.Value);

            string formValue = context.Request.Form[fieldName];
            if (String.IsNullOrEmpty(formValue))
            {
                // error: form token is missing
                throw CreateValidationException();
            }
            AntiForgeryData formToken = Serializer.Deserialize(formValue);

            if (!String.Equals(cookieToken.Value, formToken.Value, StringComparison.Ordinal))
            {
                // error: form token does not match cookie token
                throw CreateValidationException();
            }

            string currentUsername = AntiForgeryData.GetUsername(context.User);
            if (!String.Equals(formToken.Username, currentUsername, StringComparison.OrdinalIgnoreCase))
            {
                // error: form token is not valid for this user
                // (don't care about cookie token)
                throw CreateValidationException();
            }

            if (!String.Equals(salt ?? String.Empty, formToken.Salt, StringComparison.Ordinal))
            {
                // error: custom validation failed
                throw CreateValidationException();
            }
        }
    }
}
