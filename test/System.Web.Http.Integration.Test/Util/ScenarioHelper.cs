using System.Net.Http;
using System.Threading;
using System.Web.Http.Common;

namespace System.Web.Http
{
    public static class ScenarioHelper
    {
        public static string BaseAddress = "http://localhost";
        public static void RunTest(string controllerName, string routeSuffix, HttpRequestMessage request,
            Action<HttpResponseMessage> assert, Action<HttpConfiguration> configurer = null)
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute("Default", "{controller}" + routeSuffix, new { controller = controllerName });
            if (configurer != null)
            {
                configurer(config);
            }
            HttpServer server = new HttpServer(config);
            HttpResponseMessage response = null;
            try
            {
                // Act
                response = server.SubmitRequestAsync(request, CancellationToken.None).Result;

                // Assert
                assert(response);
            }
            finally
            {
                request.Dispose();
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }
    }
}
