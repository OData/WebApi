// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.SelfHost
{
    public class HttpSelfHostServerTest : IDisposable
    {
        private string machineName = Environment.MachineName;
        private HttpSelfHostServer _bufferServer;
        private HttpSelfHostServer _streamServer;

        public HttpSelfHostServerTest()
        {
            SetupHosts();
        }

        public void Dispose()
        {
            CleanupHosts();
        }

        private void SetupHosts()
        {
            try
            {
                HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(BaseUri(TransferMode.Buffered));
                config.Routes.MapHttpRoute("Default", "{controller}/{action}");
                _bufferServer = new HttpSelfHostServer(config);
                SafeOpen(_bufferServer);

                config = new HttpSelfHostConfiguration(BaseUri(TransferMode.Streamed));
                config.Routes.MapHttpRoute("Default", "{controller}/{action}");
                config.TransferMode = TransferMode.Streamed;
                _streamServer = new HttpSelfHostServer(config);
                SafeOpen(_streamServer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("HttpSelfHostServerTests.SetupHosts failed: " + ex.GetBaseException());
                throw;
            }
        }

        private void CleanupHosts()
        {
            SafeClose(_bufferServer);
            SafeClose(_streamServer);
        }

        // HttpSelfHostServer has a small latency between CloseAsync.Wait
        // completing and other async tasks still running.  Theory driven
        // tests run quickly enough they sometimes attempt to open when the
        // prior test is still finishing those async tasks.
        private static void SafeOpen(HttpSelfHostServer server)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    server.OpenAsync().Wait();
                    return;
                }
                catch (Exception)
                {
                    if (i == 9)
                    {
                        System.Diagnostics.Debug.WriteLine("HttpSelfHostServerTests.SafeOpen failed to open server at " + server.Configuration.VirtualPathRoot);
                        throw;
                    }

                    Thread.Sleep(200);
                }
            }
        }

        private void SafeClose(HttpSelfHostServer server)
        {
            try
            {
                server.CloseAsync().Wait();
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("HttpSelfHostServerTests.SafeOpen failed to close server at " + server.Configuration.VirtualPathRoot);
            }
        }

        private string BaseUri(TransferMode transferMode)
        {
            return transferMode == TransferMode.Streamed
                ? String.Format("http://{0}:8081/stream", machineName)
                : String.Format("http://{0}:8081", machineName);
        }

        private HttpSelfHostServer GetServer(TransferMode transferMode)
        {
            return transferMode == TransferMode.Streamed
                       ? _streamServer
                       : _bufferServer;
        }

        [Theory]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/EchoString", TransferMode.Streamed)]
        public void SendAsync_Direct_Returns_OK_For_Successful_ObjectContent_Write(string uri, TransferMode transferMode)
        {
            // Arrange & Act
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
            HttpResponseMessage response = new HttpClient(GetServer(transferMode)).GetAsync(BaseUri(transferMode) + uri).Result;
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
            Assert.Throws<InvalidOperationException>(
                () => new HttpClient(GetServer(transferMode)).GetAsync(BaseUri(transferMode) + uri).Wait());
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
            Task<HttpResponseMessage> task = new HttpClient().GetAsync(BaseUri(transferMode) + uri);

            // Act & Assert
            Assert.Throws<HttpRequestException>(() => task.Wait());
        }

        [Theory(Skip = "This currently fails on CI machine only")]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Streamed)]
        public void SendAsync_Direct_Throws_When_StreamContent_Throws(string uri, TransferMode transferMode)
        {
            // Arrange & Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => new HttpClient(GetServer(transferMode)).GetAsync(BaseUri(transferMode) + uri).Wait());
        }

        [Theory]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowBeforeWriteStream", TransferMode.Streamed)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Buffered)]
        [InlineData("/SelfHostServerTest/ThrowAfterWriteStream", TransferMode.Streamed)]
        public void SendAsync_ServiceModel_Throws_When_StreamContent_Throws(string uri, TransferMode transferMode)
        {
            // Arrange
            Task task = new HttpClient().GetAsync(BaseUri(transferMode) + uri);

            // Act & Assert
            Assert.Throws<HttpRequestException>(() => task.Wait());
        }

        internal class ThrowsBeforeTaskObjectContent : ObjectContent
        {
            public ThrowsBeforeTaskObjectContent() : base(typeof(string), "testContent", new JsonMediaTypeFormatter())
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
            public ThrowBeforeWriteStream() : base(new MemoryStream(Encoding.UTF8.GetBytes("ThrowBeforeWriteStream")))
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                throw new InvalidOperationException("ThrowBeforeWriteStream");
            }
        }

        internal class ThrowAfterWriteStream : StreamContent
        {
            public ThrowAfterWriteStream() : base(new MemoryStream(Encoding.UTF8.GetBytes("ThrowAfterWriteStream")))
            {
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                base.SerializeToStreamAsync(stream, context).Wait();
                throw new InvalidOperationException("ThrowAfterWriteStream");
            }
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
