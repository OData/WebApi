using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Common;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.SelfHost;
using Xunit;

namespace System.Web.Http.Controllers
{
    public class CustomControllerFactoryTest
    {
        [Fact]
        public void Body_WithSingletonControllerInstance_Fails()
        {
            // Arrange
            HttpClient httpClient = new HttpClient();
            string baseAddress = "http://localhost";
            string requestUri = baseAddress + "/Test";
            HttpSelfHostConfiguration configuration = new HttpSelfHostConfiguration(baseAddress);
            configuration.Routes.MapHttpRoute("Default", "{controller}", new { controller = "Test" });
            configuration.ServiceResolver.SetService(typeof(IHttpControllerFactory), new MySingletonControllerFactory());
            HttpSelfHostServer host = new HttpSelfHostServer(configuration);
            host.OpenAsync().Wait();
            HttpResponseMessage response = null;

            try
            {
                // Act
                response = httpClient.GetAsync(requestUri).Result;
                response = httpClient.GetAsync(requestUri).Result;
                response = httpClient.GetAsync(requestUri).Result;

                // Assert
                Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            }
            finally
            {
                if (response != null)
                {
                    response.Dispose();
                }
            }

            host.CloseAsync().Wait();
        }

        private class MySingletonControllerFactory : IHttpControllerFactory
        {
            private static TestController singleton = new TestController();

            public IHttpController CreateController(HttpControllerContext controllerContext, string controllerName)
            {
                return singleton;
            }

            public void ReleaseController(HttpControllerContext controllerContext, IHttpController controller)
            {
                throw new NotImplementedException();
            }

            public IDictionary<string, HttpControllerDescriptor> GetControllerMapping()
            {
                throw new NotImplementedException();
            }
        }

    }
}
