using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class JsonpMediaTypeFormatter : JsonMediaTypeFormatter
    {
        private static readonly string jsonpCallbackQueryParameter = "callback";
        private static readonly string mediaTypeHeaderTextJavascript = "text/javascript";
        private static readonly string pathExtensionJsonp = "jsonp";

        public JsonpMediaTypeFormatter()
            : base()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaTypeHeaderTextJavascript));
            MediaTypeMappings.Add(new UriPathExtensionMapping(pathExtensionJsonp, "application/javascript"));
        }

        public HttpRequestMessage Request { get; set; }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            return new JsonpMediaTypeFormatter() { Request = request, SerializerSettings = SerializerSettings };
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
        {
            //JSONP applies only for GET Requests.
            if (this.Request.Method == HttpMethod.Get)
            {
                string jsonpCallback = GetJsonpCallbackMethodName(Request);

                if (!String.IsNullOrEmpty(jsonpCallback))
                {
                    return Task.Factory.StartNew(() =>
                    {
                        var streamWriter = new StreamWriter(writeStream);
                        streamWriter.Write(jsonpCallback + "(");
                        streamWriter.Flush();
                        base.WriteToStreamAsync(type, value, writeStream, content, transportContext).Wait();
                        streamWriter.Write(")");
                        streamWriter.Flush();
                    });
                }
            }

            return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }

        private string GetJsonpCallbackMethodName(HttpRequestMessage httpRequestMessage)
        {
            var queryStrings = HttpUtility.ParseQueryString(httpRequestMessage.RequestUri.Query);
            return queryStrings[jsonpCallbackQueryParameter];
        }
    }

    public class JsonpQueryableTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            var f = new JsonpMediaTypeFormatter();
            f.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Formatters.Clear();
            configuration.Formatters.Add(f);
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [Fact]
        public void QueryableShouldWorkWithJsonp()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/api/FilterTests/GetProducts?$top=1&callback=test");
            request.Headers.Accept.ParseAdd("text/javascript");
            var response = this.Client.SendAsync(request).Result;
            var payload = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(payload);
        }
    }
}
