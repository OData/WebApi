using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace Nuwa.Client
{
    /// <summary>
    /// IClientStrategy represents a HttpClient creation strategy.
    /// 
    /// Client strategy is rather a component relatively indepdent from 
    /// host strategy is because not like host strategy, the client
    /// strategy can be different on test method basis. For example the
    /// security credential could be different. So a HttpClient is created
    /// on basis of eacy test method (in the domain of xunit, is every 
    /// TestCommand).
    /// </summary>
    public interface IClientStrategy
    {
        bool? MessageLog { get; set; }
        bool? UseProxy { get; set; }
        bool? UseCookies { get; set; }
        ICredentials Credentials { get; set; }
        X509Certificate2 ClientCertificate { get; set; }

        HttpClient CreateClient();
    }
}
