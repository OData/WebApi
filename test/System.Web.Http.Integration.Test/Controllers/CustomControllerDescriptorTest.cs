using System.Net;
using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.SelfHost;
using Xunit;

namespace System.Web.Http.Controllers
{
    public class CustomControllerDescriptorTest
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
            configuration.Services.Replace(typeof(IHttpControllerSelector), new MySingletonControllerSelector(configuration));
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

        private class MySingletonControllerSelector : DefaultHttpControllerSelector
        {
            public MySingletonControllerSelector(HttpConfiguration configuration)
                : base(configuration)
            {
            }

            public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
            {
                return new MySingletonControllerDescriptor();
            }
        }

        private class MySingletonControllerDescriptor : HttpControllerDescriptor
        {
            private static TestController singleton = new TestController();

            public override IHttpController CreateController(HttpRequestMessage request)
            {
                return singleton;
            }

            public override void ReleaseController(IHttpController controller, HttpControllerContext controllerContext)
            {
                // do nothing
            }
        }

    }
}
