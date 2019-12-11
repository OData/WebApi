// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
#else
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
#if NETCORE
#if NETCOREAPP2_1
    public class JsonpMediaTypeFormatter : JsonOutputFormatter
#else
    public class JsonpMediaTypeFormatter : NewtonsoftJsonOutputFormatter
#endif
#else
    public class JsonpMediaTypeFormatter : JsonMediaTypeFormatter
#endif
    {
        // Using const to avoid CS0414.
        private const string jsonpCallbackQueryParameter = "callback";
        private const string mediaTypeHeaderTextJavascript = "text/javascript";
        private const string pathExtensionJsonp = "jsonp";

        public static JsonpMediaTypeFormatter Create(WebRouteConfiguration configuration)
        {
#if NETCORE
#if NETCOREAPP2_1
            var options = configuration.ServiceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value;
            var charPool = configuration.ServiceProvider.GetRequiredService<ArrayPool<char>>();
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            return new JsonpMediaTypeFormatter(options.SerializerSettings, charPool);
#else
            var options = configuration.ServiceProvider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value;
            var charPool = configuration.ServiceProvider.GetRequiredService<ArrayPool<char>>();
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            return new JsonpMediaTypeFormatter(options.SerializerSettings, charPool, new MvcOptions());
#endif
#else
            return new JsonpMediaTypeFormatter();
#endif
        }

#if NETCORE
#if NETCOREAPP2_1
        private JsonpMediaTypeFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool)
        : base(serializerSettings, charPool)
#else
        private JsonpMediaTypeFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool, MvcOptions options)
        : base(serializerSettings, charPool, options)
#endif
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaTypeHeaderTextJavascript));
            //MediaTypeMappings.Add(new UriPathExtensionMapping(pathExtensionJsonp, "application/javascript"));
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            //JSONP applies only for GET Requests.
            if (context.HttpContext.Request.Method == HttpMethod.Get.ToString())
            {
                if (context.HttpContext.Request.Query.ContainsKey(jsonpCallbackQueryParameter))
                {
                    string jsonpCallback = context.HttpContext.Request.Query[jsonpCallbackQueryParameter];
                    if (!String.IsNullOrEmpty(jsonpCallback))
                    {
                        return WriteJsonp(
                            jsonpCallback,
                            context.HttpContext.Response.Body,
                            () => base.WriteResponseBodyAsync(context, selectedEncoding));

                    }
                }
            }

            return base.WriteResponseBodyAsync(context, selectedEncoding);
        }
#else
        private JsonpMediaTypeFormatter()
            : base()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaTypeHeaderTextJavascript));
            MediaTypeMappings.Add(new UriPathExtensionMapping(pathExtensionJsonp, "application/javascript"));
            SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
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
                var queryStrings = HttpUtility.ParseQueryString(Request.RequestUri.Query);
                string jsonpCallback = queryStrings[jsonpCallbackQueryParameter];
                if (!String.IsNullOrEmpty(jsonpCallback))
                {
                    return WriteJsonp(
                        jsonpCallback,
                        writeStream,
                        () => base.WriteToStreamAsync(type, value, writeStream, content, transportContext));
                }
            }

            return base.WriteToStreamAsync(type, value, writeStream, content, transportContext);
        }
#endif

        public Task WriteJsonp(string jsonpCallback, Stream writeStream, Func<Task> baseWrite)
        {
            return Task.Factory.StartNew(async () =>
            {
                var streamWriter = new StreamWriter(writeStream);
                streamWriter.Write(jsonpCallback + "(");
                streamWriter.Flush();
                await baseWrite();
                streamWriter.Write(")");
                streamWriter.Flush();
            });
        }
    }

    public class JsonpQueryableTests : WebHostTestBase
    {
        public JsonpQueryableTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var f = JsonpMediaTypeFormatter.Create(configuration);
            configuration.RemoveNonODataFormatters();
            configuration.InsertFormatter(f);
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
