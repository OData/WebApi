// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.IdentityModel.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using System.Web.Http;

namespace System.Net.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        private const string SecurityKey = "Security";
        
        /// <summary>
        /// Gets the current <see cref="T:System.Security.Cryptography.X509Certificates.X509Certificate2"/> 
        /// stored in <see cref="T:System.ServiceModel.Security.SecurityMessageProperty"/> for the given request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="System.Security.Cryptography.X509Certificates.X509Certificate2"/>.</returns>
        public static X509Certificate2 GetClientCertificate(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            SecurityMessageProperty property = request.GetProperty<SecurityMessageProperty>(SecurityKey);
            X509Certificate2 result = null;

            if (property != null && property.ServiceSecurityContext != null && property.ServiceSecurityContext.AuthorizationContext != null)
            {
                X509CertificateClaimSet certClaimSet = null;
                foreach (ClaimSet claimSet in property.ServiceSecurityContext.AuthorizationContext.ClaimSets)
                {
                    certClaimSet = claimSet as X509CertificateClaimSet;

                    if (certClaimSet != null)
                    {
                        result = certClaimSet.X509Certificate;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the current <see cref="T:System.ServiceModel.Security.SecurityMessageProperty"/> 
        /// stored in <see cref="M:HttpRequestMessage.Properties"/> for the given request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="SecurityMessageProperty"/>.</returns>
        public static SecurityMessageProperty GetSecurityMessageProperty(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetProperty<SecurityMessageProperty>(SecurityKey);
        }

        private static T GetProperty<T>(this HttpRequestMessage request, string key)
        {
            T value;
            request.Properties.TryGetValue(key, out value);
            return value;
        }
    }
}
