// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.Controllers
{
    public class ApiControllerTestabilityTest
    {
        [Fact]
        public void Post_With_EmptyRequest_And_MockUrlHelper()
        {
            // Arrange
            CustomersController controller = new CustomersController();
            controller.Configuration = new HttpConfiguration();
            controller.Request = new HttpRequestMessage();

            Mock<UrlHelper> url = new Mock<UrlHelper>();
            url.Setup(u => u.Link("default", new { id = 42 })).Returns("http://location_header/").Verifiable();
            controller.Url = url.Object;

            // Act
            var result = controller.Post(new Customer { ID = 42 });

            // Assert
            Customer customer;
            Assert.Equal("http://location_header/", result.Headers.Location.AbsoluteUri);
            Assert.True(result.TryGetContentValue<Customer>(out customer));
            Assert.Equal(42, customer.ID);
        }

        [Fact]
        public void Post_With_EmptyConfiguration_ThrowsNoRoute()
        {
            // Arrange
            CustomersController controller = new CustomersController();
            controller.Configuration = new HttpConfiguration();
            controller.Request = new HttpRequestMessage();

            // Act
            Assert.ThrowsArgument(
                () => controller.Post(new Customer { ID = 42 }),
                "name",
                "A route named 'default' could not be found in the route collection.");
        }

        [Fact]
        public void Post_With_InitializeConfigurationAndRequestAndRouteData()
        {
            // Arrange
            CustomersController controller = new CustomersController();
            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute("default", "{controller}/{id}", new { id = RouteParameter.Optional });
            controller.RequestContext.RouteData = new HttpRouteData(new HttpRoute(), new HttpRouteValueDictionary { { "controller", "Customers" } });
            controller.Request = new HttpRequestMessage { RequestUri = new Uri("http://locahost/Customers") };

            // Act
            var result = controller.Post(new Customer { ID = 42 });

            // Assert
            Customer customer;
            Assert.Equal("http://locahost/Customers/42", result.Headers.Location.AbsoluteUri);
            Assert.True(result.TryGetContentValue<Customer>(out customer));
            Assert.Equal(42, customer.ID);
        }

        [Fact]
        public void SettingRequestWithoutContextAppliesADefaultContextToTheRequest()
        {
            // Arrange
            CustomersController controller = new CustomersController();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            controller.Request = request;

            // Arrange
            HttpRequestContext context = request.GetRequestContext(); // cache the result before accessing the controller

            // Assert
            Assert.Equal(controller.RequestContext, context);
        }

        [Fact]
        public void ControllerHasDefaultControllerContext()
        {
            // Arrange
            CustomersController controller = new CustomersController();

            // Assert
            Assert.NotNull(controller.ControllerContext);
            Assert.NotNull(controller.RequestContext);
        }

        [Fact]
        public void SettingRequestWithConflictingContextThrows()
        {
            // Arrange
            CustomersController controller = new CustomersController();

            var request = new HttpRequestMessage();
            var context = new HttpRequestContext();

            request.SetRequestContext(context);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => { controller.Request = request; },
                "The request context property on the request must be null or match ApiController.RequestContext.");
        }

        [Fact]
        public void SettingRequestContextAppliesToRequest()
        {
            // This test is to make sure the ordering of the calls request and requestcontext setters does't matter

            // Arrange
            CustomersController controller = new CustomersController();

            var request = new HttpRequestMessage();
            var context = new HttpRequestContext();

            Assert.Null(request.GetRequestContext());

            // Act
            controller.Request = request;
            controller.RequestContext = context;

            // Assert
            Assert.Equal(request.GetRequestContext(), context);
        }

        [Fact]
        public void SetRequestWithLegacyConfigurationProperty_ProvidesControllerConfiguration()
        {
            // Arrange
            CustomersController controller = new CustomersController();

            using (HttpRequestMessage request = new HttpRequestMessage())
            using (HttpConfiguration expectedConfiguration = new HttpConfiguration())
            {
                request.SetConfiguration(expectedConfiguration);
                controller.Request = request;

                // Act
                HttpConfiguration configuration = controller.Configuration;

                // Assert
                Assert.Same(expectedConfiguration, configuration);
            }
        }

        private class CustomersController : ApiController
        {
            public HttpResponseMessage Post(Customer c)
            {
                var response = Request.CreateResponse(HttpStatusCode.Created, c);
                response.Headers.Location = new Uri(Url.Link("default", new { id = c.ID }));
                return response;
            }
        }

        private class Customer
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }
    }
}
