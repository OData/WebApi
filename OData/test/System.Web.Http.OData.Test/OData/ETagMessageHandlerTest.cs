// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData
{
    public class ETagMessageHandlerTest
    {
        [Fact]
        public void SendAsync_ThrowsIfRequestIsNull()
        {
            // Arrange
            ETagMessageHandler handler = new ETagMessageHandler();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { var result = handler.SendAsync(null).Result; }, "request");
        }

        [Fact]
        public void SendAsync_ThrowsInvalidOperationExceptionIfRequestDoesNotHaveConfiguration()
        {
            // Arrange
            ETagMessageHandler handler = new ETagMessageHandler();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://host/");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => { var result = handler.SendAsync(request).Result; },
                "Request message does not contain an HttpConfiguration object.");
        }

        [Fact]
        public void SendAsync_ReturnsNullIfInnerHandlerReturnsNull()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/Customers(3)");
            request.SetConfiguration(new HttpConfiguration());
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(null) };

            // Act
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public void SendAsync_DoesNotWriteETagIfResponseIsNotSuccessful()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/Customers(3)");
            request.SetConfiguration(new HttpConfiguration());
            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.NotFound);
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public void SendAsync_DoesNotWriteETagIfResponseIsNoContent()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/Customers(3)");
            request.SetConfiguration(new HttpConfiguration());
            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public void SendAsync_DoesNotWriteETagIfETagIsNull()
        {
            // Arrange
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "NonEtagEntity(3)");
            HttpResponseMessage originalResponse = SetupResponse(HttpStatusCode.OK,
                typeof(NonEtagEntity),
                new NonEtagEntity { Id = 3 });

            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public void SendAsync_DoesNotWriteETagIfContentIsNull()
        {
            // Arrange
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "Customers(3)");
            HttpResponseMessage originalResponse = SetupResponse(HttpStatusCode.OK,
                typeof(ETagCustomer),
                value: null);

            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public void SendAsync_DoesNotWriteETagIfContentIsntObjectContent()
        {
            // Arrange
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "Customers(3)");
            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.OK);
            originalResponse.Content = new StringContent("Some content");

            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Theory]
        [InlineData("Customers(3)", "System.Web.Http.OData.ETagCustomer")]
        [InlineData("Orders(3)/System.Web.Http.OData.ETagSpecialOrder", "System.Web.Http.OData.ETagSpecialOrder")]
        [InlineData("Customers(3)/Orders(3)", "System.Web.Http.OData.ETagOrder")]
        [InlineData("Customers(3)/Orders/System.Web.Http.OData.ETagSpecialOrder(3)", "System.Web.Http.OData.ETagSpecialOrder")]
        [InlineData("Customers(3)/Orders/System.Web.Http.OData.ETagSpecialOrder/System.Web.Http.OData.ETagOrder/System.Web.Http.OData.ETagSpecialOrder(3)",
            "System.Web.Http.OData.ETagSpecialOrder")]
        [InlineData("Customers(3)/Orders(3)/System.Web.Http.OData.ETagSpecialOrder", "System.Web.Http.OData.ETagSpecialOrder")]
        [InlineData("Customers(3)/Orders(3)/System.Web.Http.OData.ETagSpecialOrder/System.Web.Http.OData.ETagOrder/System.Web.Http.OData.ETagSpecialOrder",
            "System.Web.Http.OData.ETagSpecialOrder")]
        [InlineData("Customers(3)/Address", "System.Web.Http.OData.ETagAddress")]
        public void GetSingleEntityEntityType_ReturnsEntityTypeForSingleEntityResources(string odataPath, string typeName)
        {
            // Arrange
            IEdmModel model = SetupModel();
            IODataPathHandler pathHandler = new DefaultODataPathHandler();
            ODataPath path = pathHandler.Parse(model, odataPath);

            // Guard
            Assert.NotNull(path);

            // Act
            IEdmEntityType entityType = ETagMessageHandler.GetSingleEntityEntityType(path);

            // Assert
            Assert.NotNull(entityType);
            Assert.Equal(typeName, entityType.FullName());
        }

        [Fact]
        public void SendAsync_WritesETagToResponseHeaders()
        {
            // Arrange
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "Customers(3)");
            HttpResponseMessage originalResponse = SetupResponse(HttpStatusCode.OK,
                typeof(ETagCustomer),
                new ETagCustomer { Id = 3, Timestamp = new byte[] { (byte)3 } });
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.NotNull(response.Headers.ETag);
        }

        private static HttpRequestMessage SetupRequest(HttpMethod method, string odataPath)
        {
            HttpRequestMessage request;
            HttpConfiguration configuration = new HttpConfiguration();
            request = new HttpRequestMessage(method, "http://host/any");
            request.SetConfiguration(configuration);
            HttpRequestMessageProperties properties = request.ODataProperties();
            IEdmModel model = SetupModel();
            properties.Model = model;
            properties.PathHandler = new DefaultODataPathHandler();
            properties.Path = properties.PathHandler.Parse(model, odataPath);
            return request;
        }

        private static HttpResponseMessage SetupResponse(HttpStatusCode statusCode, Type type, object value)
        {
            HttpResponseMessage response;
            Mock<MediaTypeFormatter> formatterMock = new Mock<MediaTypeFormatter>();
            formatterMock.Setup(p => p.CanWriteType(It.IsAny<Type>())).Returns(true);
            response = new HttpResponseMessage(statusCode);
            response.Content = new ObjectContent(type, value, formatterMock.Object);
            return response;
        }

        private static IEdmModel SetupModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            // Different types of navigation sources and navigation properties.
            builder.EntitySet<ETagCustomer>("Customers");
            builder.EntitySet<ETagOrder>("Orders");
            builder.Entity<ETagSpecialOrder>().DerivesFrom<ETagOrder>();
            builder.EntitySet<ETagAddress>("Addresses");

            // Entity without ETag to test that we only write entities with ETag.
            builder.EntitySet<NonEtagEntity>("NonEtagEntity");

            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        public class ETagCustomer
        {
            public int Id { get; set; }

            [ConcurrencyCheck]
            public byte[] Timestamp { get; set; }

            public ICollection<ETagOrder> Orders { get; set; }

            public ETagAddress Address { get; set; }
        }

        public class ETagOrder
        {
            public int Id { get; set; }

            [ConcurrencyCheck]
            public int ConcurrencyCheck { get; set; }
        }

        public class ETagSpecialOrder : ETagOrder
        {
        }

        public class ETagAddress
        {
            public int Id { get; set; }

            [ConcurrencyCheck]
            public int ConcurrencyCheck { get; set; }
        }

        public class NonEtagEntity
        {
            public int Id { get; set; }
        }

        // It's easier to create a message handler for testing rather than mocking it.
        public class TestHandler : HttpMessageHandler
        {
            private HttpResponseMessage _response;
            public TestHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult<HttpResponseMessage>(_response);
            }
        }
    }
}
