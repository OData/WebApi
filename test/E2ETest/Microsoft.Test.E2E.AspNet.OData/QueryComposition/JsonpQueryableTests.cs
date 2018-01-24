// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
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
                    return Task.Factory.StartNew(async () =>
                    {
                        var streamWriter = new StreamWriter(writeStream);
                        streamWriter.Write(jsonpCallback + "(");
                        streamWriter.Flush();
                        await base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
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

    public class JsonpQueryableTests : WebHostTestBase
    {
        public JsonpQueryableTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            var f = new JsonpMediaTypeFormatter();
            f.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Formatters.Clear();
            configuration.Formatters.Add(f);
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
        }

        [Fact]
        public async Task QueryableShouldWorkWithJsonp()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/api/FilterTests/GetProducts?$top=1&callback=test");
            request.Headers.Accept.ParseAdd("text/javascript");
            var response = await this.Client.SendAsync(request);
            var payload = await response.Content.ReadAsStringAsync();
            Console.WriteLine(payload);
        }
    }
}
