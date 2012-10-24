// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TestCommon;

namespace System.Web.Http.SelfHost
{
    public class HttpSelfHostServerTest : IDisposable
    {
        private const int testPort = 50231;
        private const string machineName = "localhost";

        private HttpSelfHostServer server = null;

        public static int TestPort
        {
            get { return testPort; }
        }

        public void Dispose()
        {
            if (server != null)
            {
                server.CloseAsync().Wait();
            }
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Streamed)]
        public void SendAsync_Direct_Returns_OK_For_Successful_ObjectContent_Write(string uri, TransferMode transferMode)
        {
            // Arrange & Act
            server = CreateServer(transferMode);
            HttpResponseMessage response = new HttpClient().GetAsync(BaseUri(transferMode) + uri).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("\"echoString\"", responseString);
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Streamed)]
        public void SendAsync_ServiceModel_Returns_OK_For_Successful_ObjectContent_Write(string uri, TransferMode transferMode)
        {
            // Arrange
            server = CreateServer(transferMode);
            bool shouldChunk = transferMode == TransferMode.Streamed;

            // Act
            HttpResponseMessage response = new HttpClient().GetAsync(BaseUri(transferMode) + uri).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            IEnumerable<string> headerValues = null;
            bool isChunked = response.Headers.TryGetValues("Transfer-Encoding", out headerValues) && headerValues != null &&
                             headerValues.Any((v) => String.Equals(v, "chunked", StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("\"echoString\"", responseString);
            Assert.Equal(shouldChunk, isChunked);
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoStream", TransferMode.Streamed)]
        public void SendAsync_Direct_Returns_OK_For_Successful_Stream_Write(string uri, TransferMode transferMode)
        {
            // Arrange & Act
            server = CreateServer(transferMode);
            HttpResponseMessage response = new HttpClient(server).GetAsync(BaseUri(transferMode) + uri).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            IEnumerable<string> headerValues = null;
            bool isChunked = response.Headers.TryGetValues("Transfer-Encoding", out headerValues) && headerValues != null &&
                             headerValues.Any((v) => String.Equals(v, "chunked", StringComparison.OrdinalIgnoreCase));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("echoStream", responseString);
            Assert.False(isChunked);    // stream never chunk, buffered or streamed
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoStream", TransferMode.Streamed)]
        public void SendAsync_ServiceModel_Returns_OK_For_Successful_Stream_Write(string uri, TransferMode transferMode)
        {
            // Arrange & Act
            server = CreateServer(transferMode);
            HttpResponseMessage response = new HttpClient().GetAsync(BaseUri(transferMode) + uri).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("echoStream", responseString);
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeTask", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeTask", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWrite", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWrite", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWrite", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWrite", TransferMode.Streamed)]
        public void SendAsync_Direct_Throws_When_ObjectContent_CopyToAsync_Throws(string uri, TransferMode transferMode)
        {
            // Arrange & Act & Assert
            server = CreateServer(transferMode);
            Assert.Throws<InvalidOperationException>(
                () => new HttpClient(server).GetAsync(BaseUri(transferMode) + uri).Wait());
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeTask", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeTask", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWrite", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWrite", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWrite", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWrite", TransferMode.Streamed)]
        public void SendAsync_ServiceModel_Closes_Connection_When_ObjectContent_CopyToAsync_Throws(string uri, TransferMode transferMode)
        {
            // Arrange
            server = CreateServer(transferMode);
            Task<HttpResponseMessage> task = new HttpClient().GetAsync(BaseUri(transferMode) + uri);

            // Act & Assert
            Assert.Throws<HttpRequestException>(() => task.Wait());
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Streamed)]
        public void SendAsync_Direct_Throws_When_StreamContent_Throws(string uri, TransferMode transferMode)
        {
            // Arrange & Act & Assert
            server = CreateServer(transferMode);
            Assert.Throws<InvalidOperationException>(
                () => new HttpClient(server).GetAsync(BaseUri(transferMode) + uri).Wait());
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Streamed)]
        public void SendAsync_ServiceModel_Throws_When_StreamContent_Throws(string uri, TransferMode transferMode)
        {
            // Arrange
            server = CreateServer(transferMode);
            Task task = new HttpClient().GetAsync(BaseUri(transferMode) + uri);

            // Act & Assert
            Assert.Throws<HttpRequestException>(() => task.Wait());
        }

        internal class ThrowsBeforeTaskObjectContent : ObjectContent
        {
            public ThrowsBeforeTaskObjectContent()
                : base(typeof(string), "testContent", new JsonMediaTypeFormatter())
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                throw new InvalidOperationException("ThrowBeforeTask");
            }
        }

        internal class ThrowBeforeWriteObjectContent : ObjectContent
        {
            public ThrowBeforeWriteObjectContent()
                : base(typeof(string), "testContent", new JsonMediaTypeFormatter())
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return Task.Factory.StartNew(() =>
                        {
                            throw new InvalidOperationException("ThrowBeforeWrite");
                        });

            }
        }

        internal class ThrowAfterWriteObjectContent : ObjectContent
        {
            public ThrowAfterWriteObjectContent()
                : base(typeof(string), "testContent", new JsonMediaTypeFormatter())
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return Task.Factory.StartNew(() =>
                                                 {
                                                     byte[] buffer =
                                                         Encoding.UTF8.GetBytes("ThrowAfterWrite");
                                                     stream.Write(buffer, 0, buffer.Length);
                                                     throw new InvalidOperationException("ThrowAfterWrite");
                                                 });
            }
        }

        internal class ThrowBeforeWriteStream : StreamContent
        {
            public ThrowBeforeWriteStream()
                : base(new MemoryStream(Encoding.UTF8.GetBytes("ThrowBeforeWriteStream")))
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                throw new InvalidOperationException("ThrowBeforeWriteStream");
            }
        }

        internal class ThrowAfterWriteStream : StreamContent
        {
            public ThrowAfterWriteStream()
                : base(new MemoryStream(Encoding.UTF8.GetBytes("ThrowAfterWriteStream")))
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                base.SerializeToStreamAsync(stream, context).Wait();
                throw new InvalidOperationException("ThrowAfterWriteStream");
            }
        }

        private static HttpSelfHostServer CreateServer(TransferMode transferMode)
        {
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(BaseUri(transferMode));
            config.HostNameComparisonMode = HostNameComparisonMode.Exact;
            config.Routes.MapHttpRoute("Default", "{controller}/{action}");
            config.TransferMode = transferMode;

            HttpSelfHostServer server = new HttpSelfHostServer(config);
            server.OpenAsync().Wait();
            return server;
        }

        private static string BaseUri(TransferMode transferMode)
        {
            return transferMode == TransferMode.Streamed
                ? String.Format("http://{0}:{1}/stream", machineName, TestPort)
                : String.Format("http://{0}:{1}", machineName, TestPort);
        }
    }

    public class SelfHostServerTestController : ApiController
    {
        [HttpGet]
        public string EchoString()
        {
            return "echoString";
        }

        [HttpGet]
        public HttpResponseMessage EchoStream()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("echoStream")))
                       };
        }

        [HttpGet]
        public HttpResponseMessage ThrowBeforeTask()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = Request,
                    Content = new HttpSelfHostServerTest.ThrowsBeforeTaskObjectContent()
                };
        }

        [HttpGet]
        public HttpResponseMessage ThrowBeforeWrite()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = Request,
                Content = new HttpSelfHostServerTest.ThrowBeforeWriteObjectContent()
            };
        }

        [HttpGet]
        public HttpResponseMessage ThrowAfterWrite()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = Request,
                Content = new HttpSelfHostServerTest.ThrowAfterWriteObjectContent()
            };
        }

        [HttpGet]
        public HttpResponseMessage ThrowBeforeWriteStream()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = Request,
                Content = new HttpSelfHostServerTest.ThrowBeforeWriteStream()
            };
        }

        [HttpGet]
        public HttpResponseMessage ThrowAfterWriteStream()
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = Request,
                Content = new HttpSelfHostServerTest.ThrowAfterWriteStream()
            };
        }
    }
}
