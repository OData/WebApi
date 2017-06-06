using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace WebStack.QA.Js.Server
{
    public class JsTestPageController : ApiController
    {
        public HttpResponseMessage GetByReference(string file = null)
        {
            var settings = this.Configuration.GetJsServerSettings();
            if (settings == null)
            {
                throw new InvalidOperationException("Js server settings must be set to use js server");
            }

            if (settings.Builder == null)
            {
                throw new InvalidOperationException("Page builder must be set to use js server");
            }

            var builder = new HtmlPageBuilder(settings.Builder);
            if (!string.IsNullOrEmpty(file))
            {
                builder.ScriptReferences.Add(file);
            }
            string content = builder.Build(settings.Root);

            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(content, Encoding.UTF8, "text/html");
            return response;
        }

        public HttpResponseMessage GetByCode(string code)
        {
            var settings = this.Configuration.GetJsServerSettings();
            if (settings == null)
            {
                throw new InvalidOperationException("Js server settings must be set to use js server");
            }

            if (settings.Builder == null)
            {
                throw new InvalidOperationException("Page builder must be set to use js server");
            }

            var builder = new HtmlPageBuilder(settings.Builder);
            if (!string.IsNullOrEmpty(code))
            {
                builder.ScriptCode.Add(Encoding.UTF8.GetString(Convert.FromBase64String(code)));
            }
            string content = builder.Build(settings.Root);

            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StringContent(content, Encoding.UTF8, "text/html");
            return response;
        }
    }
}
