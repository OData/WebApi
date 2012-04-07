// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
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
                Assert.False((bool)suppressRedirect.GetValue(contextMock.Object, null));
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
                Assert.True((bool)suppressRedirect.GetValue(contextMock.Object, null));
            }
        }

        [Theory]
        [PropertyData("OutputBufferingTestData")]
        public void IsOutputBufferingNecessary_Returns_Correct_Value(HttpContent content, bool isBuffered)
        {
            // Arrange & Act & Assert
            Assert.Equal(isBuffered, HttpControllerHandler.IsOutputBufferingNecessary(content));
        }

        [Theory]
        [PropertyData("OutputBufferingTestData")]
        public void IsOutputBufferingNecessary_Causes_Content_Length_Header_To_Be_Set(HttpContent content, bool isBuffered)
        {
            // Arrange & Act
            HttpControllerHandler.IsOutputBufferingNecessary(content);
            IEnumerable<string> contentLengthEnumerable;
            bool isContentLengthInHeaders = content.Headers.TryGetValues("Content-Length", out contentLengthEnumerable);
            string[] contentLengthStrings = isContentLengthInHeaders ? contentLengthEnumerable.ToArray() : new string[0];
            long? contentLength = content.Headers.ContentLength;

            // Assert
            if (contentLength.HasValue && contentLength.Value >= 0)
            {
                // Setting the header is HttpContentHeader's responsibility, but we assert
                // it has happened because it is IsOutputBufferingNecessary's responsibility
                // to cause that to happen.   HttpControllerHandler relies on this.
                Assert.True(isContentLengthInHeaders);
                Assert.Equal(contentLength.Value, long.Parse(contentLengthStrings[0]));
            }
        }

        public static IEnumerable<object[]> OutputBufferingTestData
        {
            get
            {
                string testString = "testString";
                Mock<Stream> mockStream = new Mock<Stream>() { CallBase = true };
                return new TheoryDataSet<HttpContent, bool>()
                {
                    // Known length HttpContents other than OC should not buffer
                    { new StringContent(testString), false },
                    { new ByteArrayContent(Encoding.UTF8.GetBytes(testString)), false },
                    { new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(testString))), false},

                    // StreamContent (unknown length) should not buffer
                    { new StreamContent(mockStream.Object), false},

                    // ObjectContent (unknown length) should buffer
                    { new ObjectContent<string>(testString, new XmlMediaTypeFormatter()), true }
                };
            }
        }
    }
}
