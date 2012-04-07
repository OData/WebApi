// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web.Http.SelfHost.Properties;

namespace System.Web.Http.SelfHost.ServiceModel
{
    internal static class HttpTransportSecurityExtensionMethods
    {
        internal static void ConfigureTransportProtectionAndAuthentication(this HttpTransportSecurity httpTransportSecurity, HttpsTransportBindingElement httpsTransportBindingElement)
        {
            Debug.Assert(httpTransportSecurity != null, "httpTransportSecurity cannot be null");
            Debug.Assert(httpsTransportBindingElement != null, "httpsTransportBindingElement cannot be null");

            httpTransportSecurity.ConfigureAuthentication(httpsTransportBindingElement);
            httpsTransportBindingElement.RequireClientCertificate = httpTransportSecurity.ClientCredentialType == HttpClientCredentialType.Certificate;
        }

        internal static void ConfigureTransportAuthentication(this HttpTransportSecurity httpTransportSecurity, HttpTransportBindingElement httpTransportBindingElement)
        {
            Debug.Assert(httpTransportSecurity != null, "httpTransportSecurity cannot be null");
            Debug.Assert(httpTransportBindingElement != null, "httpTransportBindingElement cannot be null");

            if (httpTransportSecurity.ClientCredentialType == HttpClientCredentialType.Certificate)
            {
                throw Error.InvalidOperation(SRResources.CertificateUnsupportedForHttpTransportCredentialOnly);
            }

            httpTransportSecurity.ConfigureAuthentication(httpTransportBindingElement);
        }

        internal static void DisableTransportAuthentication(this HttpTransportSecurity httpTransportSecurity, HttpTransportBindingElement httpTransportBindingElement)
        {
            Debug.Assert(httpTransportSecurity != null, "httpTransportSecurity cannot be null");
            Debug.Assert(httpTransportBindingElement != null, "httpTransportBindingElement cannot be null");

            httpTransportBindingElement.AuthenticationScheme = AuthenticationSchemes.Anonymous;
            httpTransportBindingElement.ProxyAuthenticationScheme = AuthenticationSchemes.Anonymous;
            httpTransportBindingElement.Realm = String.Empty;
            httpTransportBindingElement.ExtendedProtectionPolicy = httpTransportSecurity.ExtendedProtectionPolicy;
        }

        private static void ConfigureAuthentication(this HttpTransportSecurity httpTransportSecurity, HttpTransportBindingElement httpTransportBindingElement)
        {
            Debug.Assert(httpTransportSecurity != null, "httpTransportSecurity cannot be null");
            Debug.Assert(httpTransportBindingElement != null, "httpTransportBindingElement cannot be null");

            httpTransportBindingElement.AuthenticationScheme = HttpClientCredentialTypeHelper.MapToAuthenticationScheme(httpTransportSecurity.ClientCredentialType);
            httpTransportBindingElement.ProxyAuthenticationScheme = HttpProxyCredentialTypeHelper.MapToAuthenticationScheme(httpTransportSecurity.ProxyCredentialType);
            httpTransportBindingElement.Realm = httpTransportSecurity.Realm;
            httpTransportBindingElement.ExtendedProtectionPolicy = httpTransportSecurity.ExtendedProtectionPolicy;
        }
    }
}
