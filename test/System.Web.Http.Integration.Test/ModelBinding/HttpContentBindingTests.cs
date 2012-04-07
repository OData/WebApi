// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.SelfHost;
using Xunit;
using Xunit.Extensions;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Tests actions that directly use HttpRequestMessage parameters
    /// </summary>
    public class HttpContentBindingTests : IDisposable
    {
        public HttpContentBindingTests()
        {
            this.SetupHost();
        }

        public void Dispose()
        {
            this.CleanupHost();
        }

        [Theory]
        [InlineData("application/xml")]
        [InlineData("text/xml")]
        [InlineData("application/json")]
        [InlineData("text/json")]
        public void Action_Directly_Reads_HttpRequestMessage(string mediaType)
        {
            Order order = new Order() { OrderId = "99", OrderValue = 100.0 };
            var formatter = new MediaTypeFormatterCollection().FindWriter(typeof(Order), new MediaTypeHeaderValue(mediaType));
            HttpRequestMessage request = new HttpRequestMessage()
            {
                Content = new ObjectContent<Order>(order, formatter, mediaType),
                RequestUri = new Uri(baseAddress + "/HttpContentBinding/HandleMessage"),
                Method = HttpMethod.Post
            };

            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            Order receivedOrder = response.Content.ReadAsAsync<Order>().Result;
            Assert.Equal(order.OrderId, receivedOrder.OrderId);
            Assert.Equal(order.OrderValue, receivedOrder.OrderValue);
        }

        private HttpSelfHostServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;

        private void SetupHost()
        {
            httpClient = new HttpClient();

            baseAddress = String.Format("http://{0}", Environment.MachineName);

            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "HttpContentBinding", action = "HandleMessage" });

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

    public class Order
    {
        public string OrderId { get; set; }
        public double OrderValue { get; set; }
    }

    public class HttpContentBindingController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage HandleMessage()
        {
            Order order = Request.Content.ReadAsAsync<Order>().Result;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<Order>(order, new JsonMediaTypeFormatter())
            };
        }
    }
}