using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace WebStack.QA.Js.Server
{
    public class ResourceController : ApiController
    {
        public HttpResponseMessage GetScript(string resourceName)
        {
            var settings = this.Configuration.GetJsServerSettings();
            if (settings == null)
            {
                throw new InvalidOperationException("Js server settings must be set to use js server");
            }

            Stream stream = settings.Loader.ReadAsStream(resourceName);

            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/javascript");
            return response;
        }

        public HttpResponseMessage GetCss(string resourceName)
        {
            var settings = this.Configuration.GetJsServerSettings();
            if (settings == null)
            {
                throw new InvalidOperationException("Js server settings must be set to use js server");
            }

            Stream stream = settings.Loader.ReadAsStream(resourceName);

            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/css");
            return response;
        }
    }
}
