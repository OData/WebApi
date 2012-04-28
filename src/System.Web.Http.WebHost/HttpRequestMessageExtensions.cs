// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Http;
using System.Web.Http.WebHost;

namespace System.Net.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        private const string ClientCertificateKey = "MS_ClientCertificate";

        /// <summary>
        /// Gets the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2"/> 
        /// created from <see cref="T:System.Web.HttpContextBase"/> for the given request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="System.Security.Cryptography.X509Certificates.X509Certificate2"/>.</returns>
        public static X509Certificate2 GetClientCertificate(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            X509Certificate2 result = null;

            if (!request.Properties.TryGetValue(ClientCertificateKey, out result))
            {
                HttpContextBase httpContextBase;
                if (request.Properties.TryGetValue(HttpControllerHandler.HttpContextBaseKey, out httpContextBase))
                {
                    result = new X509Certificate2(httpContextBase.Request.ClientCertificate.Certificate);
                    request.Properties.Add(ClientCertificateKey, result);
                }
            }

            return result;
        }
    }
}
