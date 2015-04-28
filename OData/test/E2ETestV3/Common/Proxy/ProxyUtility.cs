using System.Configuration;
using System.Net;
using System.Net.Http;
using Xunit;

namespace WebStack.QA.Common.Proxy
{
    public static class ProxyUtility
    {
        private const string RelativePathToCheckRunning = "/Status/IsRunning";

        /// <summary>
        /// The proxy Url configured in app settings
        /// </summary>
        public static string ProxyUrl 
        {
            get
            {
                return ConfigurationManager.AppSettings["proxy"];
            }
        }

        /// <summary>
        /// Create the proxy client and check its running status before return
        /// </summary>
        /// <returns>HttpClient with proxy configured</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Object is returned (inside the handler) to the caller")]
        public static HttpClient CreateProxyClient()
        {
            CheckProxyStatus();           

            var clientHandler = new HttpClientHandler();
            clientHandler.Proxy = new WebProxy(ProxyUtility.ProxyUrl);
            return new HttpClient(clientHandler);
        }

        private static void CheckProxyStatus()
        {
            using (HttpClient hc = new HttpClient())
            {
                using (var testReq = new HttpRequestMessage(
                    HttpMethod.Get,
                    ProxyUrl + RelativePathToCheckRunning))
                {
                    var resp = hc.SendAsync(testReq).Result;
                    Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
                }
            }
        }
    }
}
