// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Test
{
    public class ODataNullValueMessageHandlerTest
    {
        [Fact]
        public void SendAsync_ThrowsIfRequestIsNull()
        {
            // Arrange
            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => { HttpResponseMessage result = handler.SendAsync(null).Result; }, "request");
        }

        [Fact]
        public void SendAsync_ReturnsNullIfResponseIsNull()
        {
            // Arrange
            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler
                {
                    InnerHandler = new TestMessageHandler(null)
                };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/any");
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment("EntitySet"));

            // Act 
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Null(response);
        }

        [Fact]
        public void SendAsync_ReturnsOriginalResponseIfContentIsNull()
        {
            // Arrange
            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.OK);

            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler
            {
                InnerHandler = new TestMessageHandler(originalResponse)
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/any");
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment("EntitySet"));

            // Act 
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Same(originalResponse, response);
        }

        [Fact]
        public void SendAsync_ReturnsOriginalResponseIfNoObjectContent()
        {
            // Arrange
            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.OK);
            originalResponse.Content = new StringContent("test");

            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler
            {
                InnerHandler = new TestMessageHandler(originalResponse)
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/any");
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment("EntitySet"));

            // Act 
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Same(originalResponse, response);
        }

        [Fact]
        public void SendAsync_ReturnsOriginalResponseIfObjectContentHasValue()
        {
            // Arrange
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            formatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);

            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.OK);
            originalResponse.Content = new ObjectContent(typeof(string), "value", formatter.Object);

            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler
            {
                InnerHandler = new TestMessageHandler(originalResponse)
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/any");
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment("EntitySet"));

            // Act 
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Same(originalResponse, response);
        }

        [Theory]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Redirect)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public void SendAsync_ReturnsOriginalResponseIfStatusCodeIsNotOk(HttpStatusCode status)
        {
            // Arrange
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            formatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);

            HttpResponseMessage originalResponse = new HttpResponseMessage(status);
            originalResponse.Content = new ObjectContent(typeof(string), null, formatter.Object);

            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler
            {
                InnerHandler = new TestMessageHandler(originalResponse)
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/any");
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment("EntitySet"));

            // Act
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Same(originalResponse, response);
        }

        [Theory]
        [InlineData("Delete")]
        [InlineData("Post")]
        [InlineData("Put")]
        [InlineData("Patch")]
        public void SendAsync_ReturnsOriginalResponseIfRequestIsNotGet(string method)
        {
            // Arrange
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            formatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);

            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.OK);
            originalResponse.Content = new ObjectContent(typeof(string), null, formatter.Object);

            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler
            {
                InnerHandler = new TestMessageHandler(originalResponse)
            };

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), "http://localhost/any");
            request.ODataProperties().Path = new ODataPath(new EntitySetPathSegment("EntitySet"));

            // Act 
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Same(originalResponse, response);
        }

        [Fact]
        public void SendAsync_ReturnsOriginalResponseIfRequestDoesNotHaveODataPath()
        {
            // Arrange
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            formatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);

            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.OK);
            originalResponse.Content = new ObjectContent(typeof(string), null, formatter.Object);

            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler
            {
                InnerHandler = new TestMessageHandler(originalResponse)
            };

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/any");

            // Act 
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Same(originalResponse, response);
        }

        [Fact]
        public void SendAsync_ReturnsNotFoundForNullEntityResponse()
        {
            // Arrange
            Mock<MediaTypeFormatter> formatter = new Mock<MediaTypeFormatter>();
            formatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);

            HttpResponseMessage originalResponse = new HttpResponseMessage(HttpStatusCode.OK);
            originalResponse.Content = new ObjectContent(typeof(string), null, formatter.Object);

            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler
            {
                InnerHandler = new TestMessageHandler(originalResponse)
            };

            ODataPath path = new DefaultODataPathHandler().Parse(BuildModel(), "http://localhost/any", "Customers(3)");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/any");
            request.SetConfiguration(new HttpConfiguration());
            request.ODataProperties().Path = path;

            // Act 
            HttpResponseMessage response = handler.SendAsync(request).Result;

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("Customers", null)]
        [InlineData("Customers(3)", HttpStatusCode.NotFound)]
        [InlineData("Customers(3)/Id", HttpStatusCode.NoContent)]
        [InlineData("Customers(3)/Id/$value", HttpStatusCode.NoContent)]
        [InlineData("Customers(3)/ComplexProperty", HttpStatusCode.NoContent)]
        [InlineData("Customers(3)/PrimitiveCollection", null)]
        [InlineData("Customers(3)/ComplexCollection", null)]
        [InlineData("Customers(3)/NavigationProperty", HttpStatusCode.NoContent)]
        [InlineData("Customers(3)/CollectionNavigationProperty", null)]
        [InlineData("Customers(3)/CollectionNavigationProperty(3)", HttpStatusCode.NoContent)]
        [InlineData("Customers(3)", HttpStatusCode.NotFound)]
        [InlineData("Navigations/Test.SpecialNavigation", null)]
        [InlineData("Navigations(3)/Test.SpecialNavigation", HttpStatusCode.NotFound)]
        [InlineData("Navigations/Test.SpecialNavigation(3)", HttpStatusCode.NotFound)]
        [InlineData("Navigations/Test.SpecialNavigation(3)/Test.Navigation", HttpStatusCode.NotFound)]
        [InlineData("Customers(3)/NavigationProperty/Test.SpecialNavigation", HttpStatusCode.NoContent)]
        [InlineData("Customers(3)/CollectionNavigationProperty/Test.SpecialNavigation(3)/Test.Navigation", HttpStatusCode.NoContent)]
        [InlineData("BestNavigation", HttpStatusCode.NotFound)]
        [InlineData("BestNavigation/Test.SpecialNavigation/Test.Navigation", HttpStatusCode.NotFound)]
        public void GetResponseStatusCode_ReturnsNoContentForProperties_AndNotFoundForEntities(string odataPath,
            HttpStatusCode? expected)
        {
            // Arrange
            IEdmModel model = BuildModel();
            IODataPathHandler pathHandler = new DefaultODataPathHandler();
            ODataPath path = pathHandler.Parse(model, "http://localhost/any", odataPath);
            // Guard
            Assert.NotNull(path);

            // Act
            HttpStatusCode? statusCode = ODataNullValueMessageHandler.GetUpdatedResponseStatusCodeOrNull(path);

            // Assert
            Assert.Equal(expected, statusCode);
        }

        private static IEdmModel BuildModel()
        {
            var mb = new ODataConventionModelBuilder();
            mb.EntitySet<Customer>("Customers");
            mb.EntitySet<Navigation>("Navigations");
            mb.Singleton<Navigation>("BestNavigation");
            mb.EntityType<SpecialNavigation>().DerivesFrom<Navigation>();

            // This is just a convenience for unit testing.
            foreach (StructuralTypeConfiguration structuralType in mb.StructuralTypes)
            {
                structuralType.Namespace = "Test";
            }

            return mb.GetEdmModel();
        }

        public class Customer
        {
            public int Id { get; set; }
            public Complex ComplexProperty { get; set; }
            public ICollection<int> PrimitiveCollection { get; set; }
            public ICollection<Complex> ComplexCollection { get; set; }
            public Navigation NavigationProperty { get; set; }
            public ICollection<Navigation> CollectionNavigationProperty { get; set; }
        }

        public class Navigation
        {
            public int Id { get; set; }
        }

        public class SpecialNavigation : Navigation
        {
        }

        public class Complex
        {
            public int ComplexTypeProperty { get; set; }
        }

        private class TestMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public TestMessageHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }
    }
}
