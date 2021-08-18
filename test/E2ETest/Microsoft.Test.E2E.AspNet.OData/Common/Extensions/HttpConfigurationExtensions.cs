//-----------------------------------------------------------------------------
// <copyright file="HttpConfigurationExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETFX // This class is only used in the AspNet version.
using System.Web.Http;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Extensions
{
    /// <summary>
    /// This extension is used to create batch handlers for AspNet.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        private const string HttpServerKey = "HttpServerKey";

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
#endif
