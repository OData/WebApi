// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using Xunit;

namespace System.Web.Http.ContentNegotiation
{
    public class CustomFormatterTests : IDisposable
    {
        private HttpSelfHostServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;
        private HttpSelfHostConfiguration config = null;

        public CustomFormatterTests()
        {
            SetupHost();
        }

        public void Dispose()
        {
            this.CleanupHost();
        }

        [Fact]
        public void CustomFormatter_Overrides_SetResponseHeaders_During_Conneg()
        {
            Order reqOrdr = new Order() { OrderId = "100", OrderValue = 100.00 };
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<Order>(reqOrdr, new XmlMediaTypeFormatter())
            };
            request.RequestUri = new Uri(baseAddress + "/CustomFormatterTests/EchoOrder");
            request.Method = HttpMethod.Post;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plainwithversioninfo"));

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            response.EnsureSuccessStatusCode();

            IEnumerable<string> versionHdr = null;
            Assert.True(response.Headers.TryGetValues("Version", out versionHdr));
            Assert.Equal<string>("1.3.5.0", versionHdr.First());
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal<string>("text/plainwithversioninfo", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void CustomFormatter_Post_Returns_Request_String_Content()
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<string>("Hello World!", new PlainTextFormatter())
            };
            request.RequestUri = new Uri(baseAddress + "/CustomFormatterTests/EchoString");
            request.Method = HttpMethod.Post;

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal<string>("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal<string>("Hello World!", response.Content.ReadAsStringAsync().Result);
        }

        [Fact(Skip = "Test update needed, uses IKeyValueModel")]
        public void CustomFormatter_Post_Returns_Request_Integer_Content()
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<int>(100, new PlainTextFormatter())
            };

            request.RequestUri = new Uri(baseAddress + "/CustomFormatterTests/EchoInt");
            request.Method = HttpMethod.Post;

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal<string>("text/plain", response.Content.Headers.ContentType.MediaType);
            Assert.Equal<int>(100, Convert.ToInt32(response.Content.ReadAsStringAsync().Result));
        }


        [Fact(Skip = "Test update needed, uses IKeyValueModel")]
        public void CustomFormatter_Post_Returns_Request_ComplexType_Content()
        {
            Order reqOrdr = new Order() { OrderId = "100", OrderValue = 100.00 };
            HttpRequestMessage request = new HttpRequestMessage
            {
                Content = new ObjectContent<Order>(reqOrdr, new PlainTextFormatter())
            };
            request.RequestUri = new Uri(baseAddress + "/CustomFormatterTests/EchoOrder");
            request.Method = HttpMethod.Post;

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            response.EnsureSuccessStatusCode();
            Assert.NotNull(response.Content);
            Assert.NotNull(response.Content.Headers.ContentType);
            Assert.Equal<string>("text/plain", response.Content.Headers.ContentType.MediaType);
        }

        private void SetupHost()
        {
            httpClient = new HttpClient();
            baseAddress = String.Format("http://{0}", Environment.MachineName);
            config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "CustomFormatterTests", action = "EchoOrder" });
            config.Formatters.Add(new PlainTextFormatterWithVersionInfo());
            config.Formatters.Add(new PlainTextFormatter());

            server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
        }

        private void CleanupHost()
        {
            if (server != null)
            {
                server.CloseAsync().Wait();
            }
        }
    }

    public class PlainTextFormatterWithVersionInfo : MediaTypeFormatter
    {
        public PlainTextFormatterWithVersionInfo()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plainwithversioninfo"));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override void SetDefaultContentHeaders(Type objectType, HttpContentHeaders contentHeaders, string mediaType)
        {
            base.SetDefaultContentHeaders(objectType, contentHeaders, mediaType);
            contentHeaders.TryAddWithoutValidation("Version", "1.3.5.0");
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            string content = null;

            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(content);

            return tcs.Task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
        {
            var output = value.ToString();
            var writer = new StreamWriter(stream);
            writer.Write(output);
            writer.Flush();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);

            return tcs.Task;
        }
    }

    public class PlainTextFormatter : MediaTypeFormatter
    {
        public PlainTextFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream stream, HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            string content = null;

            using (var reader = new StreamReader(stream))
            {
                content = reader.ReadToEnd();
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(content);
            return tcs.Task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream stream, HttpContentHeaders contentHeaders, TransportContext transportContext)
        {
            var output = value == null ? String.Empty : value.ToString();
            var writer = new StreamWriter(stream);
            writer.Write(output);
            writer.Flush();

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }

    public class CustomFormatterTestsController : ApiController
    {
        [HttpPost]
        public string EchoString([FromBody] string input)
        {
            return input;
        }

        [HttpPost]
        public int EchoInt([FromBody] int input)
        {
            return input;
        }

        [HttpPost]
        public Order EchoOrder(Order order)
        {
            return order;
        }
    }

    public class Order : IEquatable<Order>, ICloneable
    {
        public string OrderId { get; set; }
        public double OrderValue { get; set; }

        public bool Equals(Order other)
        {
            return (this.OrderId.Equals(other.OrderId) && this.OrderValue.Equals(other.OrderValue));
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override string ToString()
        {
            return String.Format("OrderId:{0}, OrderValue:{1}", OrderId, OrderValue);
        }
    }
}
