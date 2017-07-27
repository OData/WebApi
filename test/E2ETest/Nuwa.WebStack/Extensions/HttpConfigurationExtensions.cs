
namespace System.Web.Http
{
    public static class HttpConfigurationExtensions
    {
        private const string HttpServerKey = "Nuwa.HttpServerKey";

        public static HttpServer GetHttpServer(this HttpConfiguration configuration)
        {
            if (configuration.Properties.ContainsKey(HttpServerKey))
            {
                return configuration.Properties[HttpServerKey] as HttpServer;
            }

            return null;
        }

        public static void SetHttpServer(this HttpConfiguration configuration, HttpServer server)
        {
            configuration.Properties[HttpServerKey] = server;
        }
    }
}
