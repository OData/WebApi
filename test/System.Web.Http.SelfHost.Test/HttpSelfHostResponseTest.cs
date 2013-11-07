// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.SelfHost
{
    public class HttpSelfHostResponseTest : IDisposable
    {
        private HttpSelfHostServer _server;
        private string _baseAddress;
        private HttpClient _httpClient;
        private NullResponseMessageHandler _messageHandler;

        private PortReserver _testPort = new PortReserver();

        public HttpSelfHostResponseTest()
        {
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(_testPort.BaseUri);
            _baseAddress = _testPort.BaseUri;

            config.HostNameComparisonMode = HostNameComparisonMode.Exact;
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "NullResponse" });

            _messageHandler = new NullResponseMessageHandler();
            config.MessageHandlers.Add(_messageHandler);

            _server = new HttpSelfHostServer(config);
            _server.OpenAsync().Wait();

            _httpClient = new HttpClient();
        }

        public void Dispose()
        {
            _testPort.Dispose();
            _httpClient.Dispose();
            _server.CloseAsync().Wait();
        }

        [Fact]
        public void Get_Returns_500_And_No_Content_For_Null_HttpResponseMessage_From_MessageHandler()
        {
            // Arrange
            _messageHandler.ReturnNull = true;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(_baseAddress, "NullResponse/GetNormalResponse"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Get;

            // Action
            HttpResponseMessage response = _httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength);
        }

        [Fact]
        public void Post_Returns_500_And_No_Content_For_Null_HttpResponseMessage_From_MessageHandler()
        {
            // Arrange
            _messageHandler.ReturnNull = true;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(_baseAddress, "NullResponse/PostNormalResponse"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Post;
            request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

            // Action
            HttpResponseMessage response = _httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength);
        }

        [Fact]
        public void Get_Returns_500_And_Error_Content_For_Null_HttpResponseMessage_From_Action()
        {
            // Arrange
            _messageHandler.ReturnNull = false;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(_baseAddress, "NullResponse/GetNullResponseFromAction"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Get;

            // Action
            HttpResponseMessage response = _httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("\"Message\":\"An error has occurred.\"", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Post_Returns_500_And_Error_Content_For_Null_HttpResponseMessage_From_Action()
        {
            // Arrange
            _messageHandler.ReturnNull = false;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(_baseAddress, "NullResponse/PostNullResponseFromAction"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Post;
            request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

            // Action
            HttpResponseMessage response = _httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("\"Message\":\"An error has occurred.\"", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Get_Returns_500_And_Error_Content_For_Null_Task_From_Action()
        {
            // Arrange
            _messageHandler.ReturnNull = false;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(_baseAddress, "NullResponse/GetNullTaskFromAction"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Get;

            // Action
            HttpResponseMessage response = _httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("\"Message\":\"An error has occurred.\"", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Post_Returns_500_And_Error_Content_For_Null_Task_From_Action()
        {
            // Arrange
            _messageHandler.ReturnNull = false;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(_baseAddress, "NullResponse/PostNullTaskFromAction"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Post;
            request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

            // Action
            HttpResponseMessage response = _httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("\"Message\":\"An error has occurred.\"", response.Content.ReadAsStringAsync().Result);
        }
    }

    public class NullResponseController : ApiController
    {
        [HttpGet]
        public NullResponseCustomer GetNormalResponse()
        {
            return new NullResponseCustomer() { Name = "Fred", Age = 39 };
        }

        [HttpPost]
        public NullResponseCustomer PostNormalResponse(NullResponseCustomer customer)
        {
            return customer;
        }

        [HttpGet]
        public HttpResponseMessage GetNullResponseFromAction()
        {
            return null;
        }

        [HttpPost]
        public HttpResponseMessage PostNullResponseFromAction(NullResponseCustomer customer)
        {
            return null;
        }

        [HttpGet]
        public Task<HttpResponseMessage> GetNullTaskFromAction()
        {
            return null;
        }

        [HttpPost]
        public Task<HttpResponseMessage> PostNullTaskFromAction(NullResponseCustomer customer)
        {
            return null;
        }
    }

    public class NullResponseCustomer
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class NullResponseMessageHandler : DelegatingHandler
    {
        public bool ReturnNull { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, Threading.CancellationToken cancellationToken)
        {
            Task<HttpResponseMessage> t = base.SendAsync(request, cancellationToken);

            if (!ReturnNull)
            {
                return t;
            }

            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            tcs.SetResult(null);
            return tcs.Task;
        }
    }
}
