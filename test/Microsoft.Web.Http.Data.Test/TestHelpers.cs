// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data.EntityClient;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Microsoft.Web.Http.Data.Test
{
    internal static class TestHelpers
    {
        internal static HttpRequestMessage CreateTestMessage(string url, HttpMethod httpMethod, HttpConfiguration config)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(httpMethod, url);
            IHttpRouteData rd = config.Routes[0].GetRouteData("/", requestMessage);
            requestMessage.Properties.Add(HttpPropertyKeys.HttpRouteDataKey, rd);
            return requestMessage;
        }

        // Return a non-functional connection string for an EF context. This will
        // allow a context to be instantiated, but not used.
        internal static string GetTestEFConnectionString()
        {
            string connectionString = new EntityConnectionStringBuilder
            {
                Metadata = "res://*",
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = new System.Data.SqlClient.SqlConnectionStringBuilder
                {
                    InitialCatalog = "Northwind",
                    DataSource = "xyz",
                    IntegratedSecurity = false,
                    UserID = "xyz",
                    Password = "xyz",
                }.ConnectionString
            }.ConnectionString;

            return connectionString;
        }
    }

    internal static class TestConstants
    {
        public static string BaseUrl = "http://testhost/";
        public static string CatalogUrl = "http://testhost/Catalog/";
        public static string CitiesUrl = "http://testhost/Cities/";
    }

    internal class HttpContextStub : HttpContextBase
    {
        private HttpRequestStub request;

        public HttpContextStub(Uri baseAddress, HttpRequestMessage request)
        {
            this.request = new HttpRequestStub(baseAddress, request);
        }

        public override HttpRequestBase Request
        {
            get
            {
                return this.request;
            }
        }
    }

    internal class HttpRequestStub : HttpRequestBase
    {
        private const string AppRelativePrefix = "~/";
        private string appRelativeCurrentExecutionFilePath;

        public HttpRequestStub(Uri baseAddress, HttpRequestMessage request)
        {
            this.appRelativeCurrentExecutionFilePath = GetAppRelativeCurrentExecutionFilePath(baseAddress.AbsoluteUri, request.RequestUri.AbsoluteUri);
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get
            {
                return this.appRelativeCurrentExecutionFilePath;
            }
        }

        public override string PathInfo
        {
            get
            {
                return String.Empty;
            }
        }

        private static string GetAppRelativeCurrentExecutionFilePath(string baseAddress, string requestUri)
        {
            int queryPos = requestUri.IndexOf('?');
            string requestUriNoQuery = queryPos < 0 ? requestUri : requestUri.Substring(0, queryPos);

            if (baseAddress.Length >= requestUriNoQuery.Length)
            {
                return AppRelativePrefix;
            }
            else
            {
                return AppRelativePrefix + requestUriNoQuery.Substring(baseAddress.Length);
            }
        }
    }
}
