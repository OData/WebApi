using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace Nuwa.Client
{
    /// <summary>
    /// default implementation if IClientStrategy
    /// </summary>
    internal class DefaultClientStrategy : IClientStrategy
    {
        public DefaultClientStrategy()
        {
            MessageLog = null;
            UseCookies = false;  // TODO: temporary compromise for DependencyResolver test cases
            Credentials = null;
        }

        public bool? MessageLog { get; set; }
        public bool? UseProxy { get; set; }
        public bool? UseCookies { get; set; }
        public ICredentials Credentials { get; set; }

        // TODO: unused
        public X509Certificate2 ClientCertificate { get; set; }

        /// <summary>
        /// Create an HttpClient based on the configuration information collected.
        /// </summary>
        /// <returns>a HttpClient instance</returns>
        public HttpClient CreateClient()
        {
            HttpClientHandler clientHandler = NegotiateSecurity();

            if (UseCookies != null)
            {
                clientHandler = clientHandler ?? new HttpClientHandler();
                clientHandler.UseCookies = (bool)this.UseCookies;
            }

            if (UseProxy ?? false)
            {
                clientHandler.UseProxy = true;
                clientHandler.Proxy = new WebProxy(ConfigurationManager.AppSettings["proxy"]);
            }

            if (MessageLog ?? false)
            {
                clientHandler = clientHandler ?? new HttpClientHandler();
                return new HttpClient(new ClientLogHandler(clientHandler));
            }
            else if (clientHandler != null)
            {
                return new HttpClient(clientHandler);
            }
            else
            {
                return new HttpClient();
            }
        }

        private HttpClientHandler NegotiateSecurity()
        {
            if (this.Credentials != null)
            {
                var retval = new HttpClientHandler();
                retval.Credentials = this.Credentials;

                return retval;
            }

            return null;
        }
    }
}