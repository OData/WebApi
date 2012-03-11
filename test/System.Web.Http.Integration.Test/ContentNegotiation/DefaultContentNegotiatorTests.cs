using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Moq;
using Xunit;

namespace System.Web.Http.ContentNegotiation
{
    public class DefaultContentNegotiatorTests : ContentNegotiationTestBase
    {
        [Fact]
        public void Custom_ContentNegotiator_Used_In_Response()
        {
            // Arrange
            configuration.Formatters.Clear();
            MediaTypeWithQualityHeaderValue requestContentType = new MediaTypeWithQualityHeaderValue("application/xml");
            MediaTypeHeaderValue responseContentType = null;

            Mock<IContentNegotiator> selector = new Mock<IContentNegotiator>();
            MediaTypeHeaderValue mediaType;
            selector.Setup(s => s.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(), It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType)).Returns(new XmlMediaTypeFormatter());

            configuration.ServiceResolver.SetService(typeof(IContentNegotiator), selector.Object);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, baseUri);
            request.Headers.Accept.Add(requestContentType);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            responseContentType = response.Content.Headers.ContentType;

            // Assert
            selector.Verify(s => s.Negotiate(It.IsAny<Type>(), It.IsAny<HttpRequestMessage>(), It.IsAny<IEnumerable<MediaTypeFormatter>>(), out mediaType), Times.AtLeastOnce());
        }
    }
}
