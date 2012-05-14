// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.WebHost
{
    public class HttpControllerHandlerTest
    {
        public static TheoryDataSet<HttpMethod> AllHttpMethods
        {
            get
            {
                return new TheoryDataSet<HttpMethod>
                {       
                    HttpMethod.Get,
                    HttpMethod.Post,
                    HttpMethod.Put,
                    HttpMethod.Delete,
                    HttpMethod.Head,
                    HttpMethod.Options,
                    HttpMethod.Trace
                };
            }
        }

        public static TheoryDataSet<HttpMethod> HttpMethodsWithContent
        {
            get
            {
                return new TheoryDataSet<HttpMethod>
                {       
                    HttpMethod.Post,
                    HttpMethod.Put,
                    HttpMethod.Delete,
                };
            }
        }

        [Theory]
        [PropertyData("AllHttpMethods")]
        public void ConvertRequest_Creates_HttpRequestMessage_For_All_HttpMethods(HttpMethod httpMethod)
        {
            // Arrange
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBase(httpMethod.Method, new MemoryStream());

            // Act
            HttpRequestMessage request = HttpControllerHandler.ConvertRequest(contextMock.Object);

            // Assert
            Assert.Equal(httpMethod, request.Method);
        }

        [Fact]
        public void ConvertRequest_Copies_Headers_And_Content_Headers()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBase("Get", new MemoryStream());
            HttpRequestBase requestBase = contextMock.Object.Request;
            NameValueCollection nameValues = requestBase.Headers;
            nameValues["myHeader"] = "myValue";
            nameValues["Content-Type"] = "application/mine";

            // Act
            HttpRequestMessage request = HttpControllerHandler.ConvertRequest(contextMock.Object);
            string[] headerValues = request.Headers.GetValues("myHeader").ToArray();

            // Assert
            Assert.Equal("myValue", headerValues[0]);
            Assert.Equal("application/mine", request.Content.Headers.ContentType.MediaType);
        }

        [Theory]
        [PropertyData("HttpMethodsWithContent")]
        public void ConvertRequest_Creates_Request_With_Content_For_Content_Methods(HttpMethod httpMethod)
        {
            // Arrange
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBase(httpMethod.Method, new MemoryStream());

            // Act
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(contextMock.Object);

            // Assert
            Assert.NotNull(actualRequest.Content);
        }

        [Fact]
        public void ConvertRequest_Uses_HostBufferPolicySelector_To_Select_Buffered_Stream()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = CreateMockHttpContextBase("Post", new MemoryStream(new byte[] { 5 }));
            MemoryStream memoryStream = new MemoryStream();

            // Act
            HttpRequestMessage actualRequest = HttpControllerHandler.ConvertRequest(contextMock.Object);
            actualRequest.Content.CopyToAsync(memoryStream).Wait();
            byte[] actualBuffer = memoryStream.GetBuffer();

            // Assert
            Assert.Equal(5, actualBuffer[0]);
        }

        [Fact]
        public void ConvertResponse_IfResponseHasNoCacheControlDefined_SetsNoCacheCacheabilityOnAspNetResponse()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            HttpResponseMessage response = new HttpResponseMessage();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            HttpControllerHandler.ConvertResponse(contextMock.Object, response, request);

            // Assert
            contextMock.Verify(c => c.Response.Cache.SetCacheability(HttpCacheability.NoCache));
        }

        [Fact]
        public void ConvertResponse_IfResponseHasCacheControlDefined_DoesNotSetCacheCacheabilityOnAspNetResponse()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            HttpResponseMessage response = new HttpResponseMessage();
            HttpRequestMessage request = new HttpRequestMessage();
            response.Headers.CacheControl = new CacheControlHeaderValue { Public = true };

            // Act
            HttpControllerHandler.ConvertResponse(contextMock.Object, response, request);

            // Assert
            contextMock.Verify(c => c.Response.Cache.SetCacheability(HttpCacheability.NoCache), Times.Never());
        }

        [Fact]
        public Task ConvertResponse_DisposesRequestAndResponse()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet((hcb) => hcb.Response.OutputStream).Returns(Stream.Null);

            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();

            // Act
            return HttpControllerHandler.ConvertResponse(contextMock.Object, response, request).ContinueWith(
                _ =>
                {
                    // Assert
                    Assert.ThrowsObjectDisposed(() => request.Method = HttpMethod.Get, typeof(HttpRequestMessage).FullName);
                    Assert.ThrowsObjectDisposed(() => response.StatusCode = HttpStatusCode.OK, typeof(HttpResponseMessage).FullName);
                });
        }

        [Fact]
        public Task ConvertResponse_DisposesRequestAndResponseWithContent()
        {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet((hcb) => hcb.Response.OutputStream).Returns(Stream.Null);

            HttpRequestMessage request = new HttpRequestMessage() { Content = new StringContent("request") };
            HttpResponseMessage response = new HttpResponseMessage() { Content = new StringContent("response") };

            // Act
            return HttpControllerHandler.ConvertResponse(contextMock.Object, response, request).ContinueWith(
                _ =>
                {
                    // Assert
                    Assert.ThrowsObjectDisposed(() => request.Method = HttpMethod.Get, typeof(HttpRequestMessage).FullName);
                    Assert.ThrowsObjectDisposed(() => response.StatusCode = HttpStatusCode.OK, typeof(HttpResponseMessage).FullName);
                });
        }

        [Fact]
        public void SuppressFormsAuthenticationRedirect_DoesntRequireSuppressRedirect() {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            IDictionary contextItems = new Hashtable();
            contextMock.SetupGet(hcb => hcb.Response.StatusCode).Returns(200);
            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);

            PropertyInfo suppressRedirect = typeof(HttpResponseBase).GetProperty(SuppressFormsAuthRedirectModule.SuppressFormsAuthenticationRedirectPropertyName, BindingFlags.Instance | BindingFlags.Public);

            // Act
            HttpControllerHandler.EnsureSuppressFormsAuthenticationRedirect(contextMock.Object);

            // Assert
            if (suppressRedirect == null) {
                // .NET 4.0
                Assert.False(contextItems.Contains(SuppressFormsAuthRedirectModule.DisableAuthenticationRedirectKey));
            }
            else {
                // .NET 4.5
                Assert.False((bool)suppressRedirect.GetValue(contextMock.Object.Response, null));
            }
        }

        [Fact]
        public void SuppressFormsAuthenticationRedirect_RequireSuppressRedirect() {
            // Arrange
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            IDictionary contextItems = new Hashtable();
            contextMock.SetupGet(hcb => hcb.Response.StatusCode).Returns(401);
            contextMock.SetupGet(hcb => hcb.Items).Returns(contextItems);

            PropertyInfo suppressRedirect = typeof(HttpResponseBase).GetProperty(SuppressFormsAuthRedirectModule.SuppressFormsAuthenticationRedirectPropertyName, BindingFlags.Instance | BindingFlags.Public);

            // Act
            HttpControllerHandler.EnsureSuppressFormsAuthenticationRedirect(contextMock.Object);

            // Assert
            if (suppressRedirect == null) {
                // .NET 4.0
                Assert.True(contextItems.Contains(SuppressFormsAuthRedirectModule.DisableAuthenticationRedirectKey));
                Assert.True((bool)contextItems[SuppressFormsAuthRedirectModule.DisableAuthenticationRedirectKey]);
            }
            else {
                // .NET 4.5
                Assert.True((bool)suppressRedirect.GetValue(contextMock.Object.Response, null));
            }
        }

        private static Mock<HttpContextBase> CreateMockHttpContextBase(string httpMethod, Stream bufferedStream)
        {
            Mock<HttpRequestBase> requestBaseMock = new Mock<HttpRequestBase>() { CallBase = true };
            requestBaseMock.SetupGet(m => m.HttpMethod).Returns(httpMethod);
            requestBaseMock.SetupGet(m => m.Url).Returns(new Uri("Http://localhost"));
            requestBaseMock.SetupGet(m => m.Headers).Returns(new NameValueCollection());
            requestBaseMock.Setup(m => m.InputStream).Returns(bufferedStream);

            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock };
            contextMock.SetupGet(m => m.Request).Returns(requestBaseMock.Object);

            return contextMock;
        }
    }
}
