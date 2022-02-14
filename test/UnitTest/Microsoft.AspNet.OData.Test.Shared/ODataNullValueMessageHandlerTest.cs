//-----------------------------------------------------------------------------
// <copyright file="ODataNullValueMessageHandlerTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class ODataNullValueMessageHandlerTest
    {
        private IEdmEntitySet _entitySet;
        public ODataNullValueMessageHandlerTest()
        {
            EdmEntityType entityType = new EdmEntityType("NS", "entity");
            EdmEntityContainer container = new EdmEntityContainer("NS", "default");
            _entitySet = new EdmEntitySet(container, "entities", entityType);
        }

#if NETCORE
        [Fact]
        public void SendAsync_ThrowsIfContextIsNull()
        {
            // Arrange
            ODataNullValueMessageHandler handler = CreateHandler();

            // Act & Assert
            ExceptionAssert.ThrowsArgumentNull(() => handler.OnResultExecuting(null), "context");
        }
#else
        [Fact]
        public async Task SendAsync_ThrowsIfRequestIsNull()
        {
            // Arrange
            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler();

            // Act & Assert
            await ExceptionAssert.ThrowsArgumentNullAsync(() => handler.SendAsync(null), "request");
        }
#endif

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
        /// <remarks>
        /// This test specifically test the AspNet behavior or ODataNullValueMessageHandler and whether it creates
        /// a new response or returns the original response. AspNetCore always returns the original response.
        /// </remarks>
        [Fact]
        public async Task SendAsync_ReturnsNullIfResponseIsNull()
        {
            // Arrange
            ODataNullValueMessageHandler handler = CreateHandler(null);
            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/any");
            request.ODataContext().Path = new ODataPath(new EntitySetSegment(_entitySet));

            // Act
            var response = await SendToHandler(handler, request);

            // Assert
            Assert.Null(response);
        }

        /// <remarks>
        /// This test specifically test the AspNet behavior or ODataNullValueMessageHandler and whether it creates
        /// a new response or returns the original response. AspNetCore always returns the original response.
        /// </remarks>
        [Fact]
        public async Task SendAsync_ReturnsOriginalResponseIfContentIsNull()
        {
            // Arrange
            var originalResponse = ResponseFactory.Create(HttpStatusCode.OK);
            ODataNullValueMessageHandler handler = CreateHandler(originalResponse);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/any");
            request.ODataContext().Path = new ODataPath(new EntitySetSegment(_entitySet));

            // Act
            var response = await SendToHandler(handler, request);

            // Assert
            Assert.Same(originalResponse, response);
        }

        /// <remarks>
        /// This test specifically test the AspNet behavior or ODataNullValueMessageHandler and whether it creates
        /// a new response or returns the original response. AspNetCore always returns the original response.
        /// </remarks>
        [Fact]
        public async Task SendAsync_ReturnsOriginalResponseIfNoObjectContent()
        {
            // Arrange
            var originalResponse = ResponseFactory.Create(HttpStatusCode.OK, "test");
            ODataNullValueMessageHandler handler = CreateHandler(originalResponse);

            var request = RequestFactory.Create(HttpMethod.Get, "http://localhost/any");
            request.ODataContext().Path = new ODataPath(new EntitySetSegment(_entitySet));

            // Act
            var response = await SendToHandler(handler, request);

            // Assert
            Assert.Same(originalResponse, response);
        }

        /// <remarks>
        /// This test specifically test the AspNet behavior or ODataNullValueMessageHandler and whether it creates
        /// a new response or returns the original response. AspNetCore always returns the original response.
        /// </remarks>
        [Fact]
        public async Task SendAsync_ReturnsOriginalResponseIfObjectContentHasValue()
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
            request.ODataProperties().Path = new ODataPath(new EntitySetSegment(_entitySet));

            // Act 
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Same(originalResponse, response);
        }

        /// <remarks>
        /// This test specifically test the AspNet behavior or ODataNullValueMessageHandler and whether it creates
        /// a new response or returns the original response. AspNetCore always returns the original response.
        /// </remarks>
        [Theory]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Redirect)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.InternalServerError)]
        public async Task SendAsync_ReturnsOriginalResponseIfStatusCodeIsNotOk(HttpStatusCode status)
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
            request.ODataProperties().Path = new ODataPath(new EntitySetSegment(_entitySet));

            // Act
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Same(originalResponse, response);
        }

        /// <remarks>
        /// This test specifically test the AspNet behavior or ODataNullValueMessageHandler and whether it creates
        /// a new response or returns the original response. AspNetCore always returns the original response.
        /// </remarks>
        [Theory]
        [InlineData("Delete")]
        [InlineData("Post")]
        [InlineData("Put")]
        [InlineData("Patch")]
        public async Task SendAsync_ReturnsOriginalResponseIfRequestIsNotGet(string method)
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
            request.ODataProperties().Path = new ODataPath(new EntitySetSegment(_entitySet));

            // Act 
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Same(originalResponse, response);
        }

        /// <remarks>
        /// This test specifically test the AspNet behavior or ODataNullValueMessageHandler and whether it creates
        /// a new response or returns the original response. AspNetCore always returns the original response.
        /// </remarks>
        [Fact]
        public async Task SendAsync_ReturnsOriginalResponseIfRequestDoesNotHaveODataPath()
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
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Same(originalResponse, response);
        }

        /// <remarks>
        /// This test specifically test the AspNet behavior or ODataNullValueMessageHandler and whether it creates
        /// a new response or returns the original response. AspNetCore always returns the original response.
        /// </remarks>
        [Fact]
        public async Task SendAsync_ReturnsNotFoundForNullEntityResponse()
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

            var configuration = RoutingConfigurationFactory.Create();
            ODataPath path = new DefaultODataPathHandler().Parse(BuildModel(), "http://localhost/any", "Customers(3)");
            HttpRequestMessage request = RequestFactory.Create(HttpMethod.Get, "http://localhost/any", configuration);
            request.ODataContext().Path = path;

            // Act 
            HttpResponseMessage response = await handler.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
#endif

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

#if NETCORE
        private ODataNullValueMessageHandler CreateHandler(AspNetCore.Http.HttpResponse originalResponse = null)
        {
            return new ODataNullValueMessageHandler();
        }

        private Task<HttpResponse> SendToHandler(
            ODataNullValueMessageHandler handler,
            AspNetCore.Http.HttpRequest request)
        {
            var pageContext = new PageContext(new ActionContext(
                request.HttpContext,
                new RouteData(),
                new PageActionDescriptor(),
                new ModelStateDictionary()));

            var model = new Mock<PageModel>();

            var modelAsFilter = model.As<IAsyncResultFilter>();
            modelAsFilter
                .Setup(f => f.OnResultExecutionAsync(It.IsAny<ResultExecutingContext>(), It.IsAny<ResultExecutionDelegate>()))
                .Returns(Task.CompletedTask);

            var resultExecutingContext = new ResultExecutingContext(
               pageContext,
               Array.Empty<IFilterMetadata>(),
               new AspNetCore.Mvc.RazorPages.PageResult(),
               model.Object);

            handler.OnResultExecuting(resultExecutingContext);

            return Task.FromResult(request.HttpContext.Response);
        }
#else
        private ODataNullValueMessageHandler CreateHandler(HttpResponseMessage originalResponse)
        {
            ODataNullValueMessageHandler handler = new ODataNullValueMessageHandler();
            handler.InnerHandler = new TestMessageHandler(originalResponse);

            return handler;
        }

        private Task<HttpResponseMessage> SendToHandler(
            ODataNullValueMessageHandler handler,
            HttpRequestMessage request)
        {
            return handler.SendAsync(request);
        }
#endif

        private static IEdmModel BuildModel()
        {
            var mb = ODataConventionModelBuilderFactory.Create();
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

#if NETFX
        /// <remarks>
        /// This class is used to return a specific response for AspNet tests.
        /// </remarks>
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
#endif
    }
}
