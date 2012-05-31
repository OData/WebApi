// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace System.Web.Http.SelfHost
{
    public class HttpSelfHostResponseTest : IDisposable
    {
        private HttpSelfHostServer server = null;
        private string baseAddress = null;
        private HttpClient httpClient = null;
        private NullResponseMessageHandler messageHandler = null;

        public HttpSelfHostResponseTest()
        {
            this.SetupHost();
        }

        public void SetupHost()
        {
            baseAddress = String.Format("http://localhost:90/");

            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "NullResponse" });

            messageHandler = new NullResponseMessageHandler();
            config.MessageHandlers.Add(messageHandler);

            server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();

            httpClient = new HttpClient();
        }

        public void Dispose()
        {
            httpClient.Dispose();
            server.CloseAsync().Wait();
        }

        [Fact]
        public void Get_Returns_500_And_No_Content_For_Null_HttpResponseMessage_From_MessageHandler()
        {
            // Arrange
            messageHandler.ReturnNull = true;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "NullResponse/GetNormalResponse"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Get;

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength);
        }

        [Fact]
        public void Post_Returns_500_And_No_Content_For_Null_HttpResponseMessage_From_MessageHandler()
        {
            // Arrange
            messageHandler.ReturnNull = true;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "NullResponse/PostNormalResponse"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Post;
            request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Equal(0, response.Content.Headers.ContentLength);
        }

        [Fact]
        public void Get_Returns_500_And_Error_Content_For_Null_HttpResponseMessage_From_Action()
        {
            // Arrange
            messageHandler.ReturnNull = false;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "NullResponse/GetNullResponseFromAction"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Get;

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("\"Message\":\"An error has occurred.\"", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Post_Returns_500_And_Error_Content_For_Null_HttpResponseMessage_From_Action()
        {
            // Arrange
            messageHandler.ReturnNull = false;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "NullResponse/PostNullResponseFromAction"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Post;
            request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("\"Message\":\"An error has occurred.\"", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Get_Returns_500_And_Error_Content_For_Null_Task_From_Action()
        {
            // Arrange
            messageHandler.ReturnNull = false;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "NullResponse/GetNullTaskFromAction"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Get;

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("\"Message\":\"An error has occurred.\"", response.Content.ReadAsStringAsync().Result);
        }

        [Fact]
        public void Post_Returns_500_And_Error_Content_For_Null_Task_From_Action()
        {
            // Arrange
            messageHandler.ReturnNull = false;
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(Path.Combine(baseAddress, "NullResponse/PostNullTaskFromAction"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Method = HttpMethod.Post;
            request.Content = new ObjectContent<NullResponseCustomer>(new NullResponseCustomer() { Name = "Sue", Age = 39 }, new JsonMediaTypeFormatter());

            // Action
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

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
