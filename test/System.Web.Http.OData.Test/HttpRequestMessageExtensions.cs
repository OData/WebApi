// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Hosting;

namespace System.Web.Http
{
    internal static class HttpRequestMessageExtensions
    {
        public static void SetConfiguration(this HttpRequestMessage request, HttpConfiguration configuration)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
        }
    }
}
