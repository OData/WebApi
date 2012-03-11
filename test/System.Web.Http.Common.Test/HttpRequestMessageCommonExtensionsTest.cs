using System.Net;
using System.Net.Http;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class HttpRequestMessageCommonExtensionsTest
    {
        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties(typeof(HttpRequestMessageCommonExtensions), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void CreateResponseThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpRequestMessageCommonExtensions.CreateResponse(null), "request");
        }

        [Fact]
        public void CreateResponseWithStatusThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpRequestMessageCommonExtensions.CreateResponse(null, HttpStatusCode.OK), "request");
        }

        [Fact]
        public void CreateResponse()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            HttpResponseMessage response = request.CreateResponse();

            // Assert
            Assert.Same(request, response.RequestMessage);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void CreateResponseWithStatus()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            HttpResponseMessage response = request.CreateResponse(HttpStatusCode.NotImplemented);

            // Assert
            Assert.Same(request, response.RequestMessage);
            Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
        }
    }
}
