// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Extensions
{
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
