//-----------------------------------------------------------------------------
// <copyright file="ETagMessageHandlerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Moq;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
    public class ETagMessageHandlerTest
    {
        [Fact]
        public async Task SendAsync_ThrowsIfRequestIsNull()
        {
            // Arrange
            ETagMessageHandler handler = new ETagMessageHandler();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => handler.SendAsync(null), "request");
        }

        [Fact]
        public async Task SendAsync_ThrowsInvalidOperationExceptionIfRequestDoesntHaveConfiguration()
        {
            // Arrange
            ETagMessageHandler handler = new ETagMessageHandler();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://host/");

            // Act & Assert
            await ExceptionAssert.ThrowsAsync<InvalidOperationException>(
                () => handler.SendAsync(request),
                "Request message does not contain an HttpConfiguration object.");
        }

        [Fact]
        public async Task SendAsync_ReturnsNullIfInnerHandlerReturnsNull()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/Customers(3)");
            request.SetConfiguration(new HttpConfiguration());
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(null) };

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public async Task SendAsync_DoesntWriteETagIfResponseIsNotSuccessful()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/Customers(3)");
            request.SetConfiguration(new HttpConfiguration());
            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.NotFound);
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public async Task SendAsync_DoesntWriteETagIfResponseIsNoContent()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/Customers(3)");
            request.SetConfiguration(new HttpConfiguration());
            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.NoContent);
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public async Task SendAsync_DoesntWriteETagIfETagIsNull()
        {
            // Arrange
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "NonEtagEntity(3)");
            HttpResponseMessage originalResponse = SetupResponse(HttpStatusCode.OK,
                typeof(NonEtagEntity),
                new NonEtagEntity { Id = 3 });

            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public async Task SendAsync_DoesntWriteETagIfContentIsNull()
        {
            // Arrange
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "Customers(3)");
            HttpResponseMessage originalResponse = SetupResponse(HttpStatusCode.OK,
                typeof(ETagCustomer),
                null);

            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Fact]
        public async Task SendAsync_DoesntWriteETagIfContentIsntObjectContent()
        {
            // Arrange
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "Customers(3)");
            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.OK);
            originalResponse.Content = new StringContent("Some content");

            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Null(response.Headers.ETag);
        }

        [Theory]
        [InlineData("Customers(3)", "Test.ETagCustomer")]
        [InlineData("Orders(3)/Test.ETagSpecialOrder", "Test.ETagSpecialOrder")]
        [InlineData("BestOrder", "Test.ETagOrder")]
        [InlineData("BestOrder/Test.ETagSpecialOrder", "Test.ETagSpecialOrder")]
        [InlineData("BestOrder/Test.ETagSpecialOrder/Test.ETagOrder/Test.ETagSpecialOrder", "Test.ETagSpecialOrder")]
        [InlineData("Customers(3)/Orders(3)", "Test.ETagOrder")]
        [InlineData("Customers(3)/Orders/Test.ETagSpecialOrder(3)", "Test.ETagSpecialOrder")]
        [InlineData("Customers(3)/Orders/Test.ETagSpecialOrder/Test.ETagOrder/Test.ETagSpecialOrder(3)", "Test.ETagSpecialOrder")]
        [InlineData("Customers(3)/Orders(3)/Test.ETagSpecialOrder", "Test.ETagSpecialOrder")]
        [InlineData("Customers(3)/Orders(3)/Test.ETagSpecialOrder/Test.ETagOrder/Test.ETagSpecialOrder", "Test.ETagSpecialOrder")]
        [InlineData("Customers(3)/Address", "Test.ETagAddress")]
        public void GetSingleEntityEntityType_ReturnsEntityTypeForSingleEntityResources(string odataPath, string typeName)
        {
            // Arrange
            IEdmModel model = SetupModel();
            IODataPathHandler pathHandler = new DefaultODataPathHandler();
            ODataPath path = pathHandler.Parse(model, "http://localhost/any", odataPath);
            // Guard
            Assert.NotNull(path);

            // Act
            IEdmEntityType entityType = ETagMessageHandler.GetSingleEntityEntityType(path);

            // Assert
            Assert.NotNull(entityType);
            Assert.Equal(typeName, entityType.FullName());
        }

        [Fact]
        public async Task SendAsync_WritesETagToResponseHeaders()
        {
            // Arrange
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "Customers(3)");
            HttpResponseMessage originalResponse = SetupResponse(HttpStatusCode.OK,
                typeof(ETagCustomer),
                new ETagCustomer { Id = 3, Timestamp = new byte[] { (byte)3 } });
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.NotNull(response.Headers.ETag);
        }

        [Fact]
        public async Task SendAsync_WritesETagToResponseHeaders_InUntyped()
        {
            // Arrange
            IEdmModel model = GetUnTypeEdmModel();
            HttpRequestMessage request = SetupRequest(HttpMethod.Get, "Customers(3)", model);

            IEdmEntityType entityType = model.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "Customer");
            EdmEntityObject customer = new EdmEntityObject(entityType);
            customer.TrySetPropertyValue("ID", 3);
            customer.TrySetPropertyValue("Name", "Sam");

            HttpResponseMessage originalResponse = SetupResponse(HttpStatusCode.OK, typeof(EdmEntityObject), customer);
            ETagMessageHandler handler = new ETagMessageHandler() { InnerHandler = new TestHandler(originalResponse) };

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal("\"J1NhbSc=\"", response.Headers.ETag.Tag);
        }

        private static HttpRequestMessage SetupRequest(HttpMethod method, string odataPath, IEdmModel edmModel = null)
        {
            HttpRequestMessage request;
            request = new HttpRequestMessage(method, "http://host/any");
            IEdmModel model = edmModel ?? SetupModel();
            request.EnableODataDependencyInjectionSupport(model);
            HttpRequestMessageProperties properties = request.ODataProperties();
            properties.Path = request.GetRequestContainer()
                .GetRequiredService<IODataPathHandler>()
                .Parse(model, "http://localhost/any", odataPath);
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
            ODataConventionModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            // Different types of navigation sources and navigation properties.
            builder.EntitySet<ETagCustomer>("Customers");
            builder.EntitySet<ETagOrder>("Orders");
            builder.EntityType<ETagSpecialOrder>().DerivesFrom<ETagOrder>();
            builder.Singleton<ETagOrder>("BestOrder");
            builder.EntitySet<ETagAddress>("Addresses");

            // Entity without ETag to test that we only write entities with ETag.
            builder.EntitySet<NonEtagEntity>("NonEtagEntity");

            // Just a simplification for referencing the types in cast segments.
            foreach (var type in builder.StructuralTypes)
            {
                type.Namespace = "Test";
            }

            IEdmModel model = builder.GetEdmModel();
            return model;
        }

        private static IEdmModel GetUnTypeEdmModel()
        {
            EdmModel model = new EdmModel();

            // entity type customer
            EdmEntityType customer = new EdmEntityType("NS", "Customer");
            customer.AddKeys(customer.AddStructuralProperty("ID", EdmPrimitiveTypeKind.Int32));
            IEdmStructuralProperty customerName = customer.AddStructuralProperty("Name", EdmPrimitiveTypeKind.String);
            model.AddElement(customer);

            // entity sets
            EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
            model.AddElement(container);
            EdmEntitySet customers = container.AddEntitySet("Customers", customer);

            model.SetOptimisticConcurrencyAnnotation(customers, new[] { customerName });
            return model;
        }

        public class ETagCustomer
        {
            public int Id { get; set; }

            [Timestamp]
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
#endif
