// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OwinHost
{
    /// <summary>
    /// Standard keys and values for use within the OWIN interfaces.
    /// </summary>
    internal static class OwinConstants
    {
        public const string CallCancelledKey = "owin.CallCancelled";

        // Request keys
        public const string RequestMethodKey = "owin.RequestMethod";
        public const string RequestSchemeKey = "owin.RequestScheme";
        public const string RequestPathBaseKey = "owin.RequestPathBase";
        public const string RequestPathKey = "owin.RequestPath";
        public const string RequestQueryStringKey = "owin.RequestQueryString";
        public const string RequestHeadersKey = "owin.RequestHeaders";
        public const string RequestBodyKey = "owin.RequestBody";
        public const string ClientCertifiateKey = "ssl.ClientCertificate";
        public const string IsLocalKey = "server.IsLocal";
        public const string UserKey = "server.User";

        // Response keys
        public const string ResponseStatusCodeKey = "owin.ResponseStatusCode";
        public const string ResponseReasonPhraseKey = "owin.ResponseReasonPhrase";
        public const string ResponseHeadersKey = "owin.ResponseHeaders";
        public const string ResponseBodyKey = "owin.ResponseBody";
    }
}