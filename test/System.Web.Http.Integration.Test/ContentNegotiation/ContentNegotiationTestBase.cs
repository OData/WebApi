// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.SelfHost;

namespace System.Web.Http.ContentNegotiation
{
    public class ContentNegotiationTestBase : IDisposable
    {
        protected readonly string baseUri = "http://localhost:8080/Conneg";
        protected HttpSelfHostServer server = null;
        protected HttpSelfHostConfiguration configuration = null;
        protected HttpClient httpClient = null;

        public ContentNegotiationTestBase()
        {
            this.SetupHost();
        }

        public void Dispose()
        {
            this.CleanupHost();
        }

        public void SetupHost()
        {
            configuration = new HttpSelfHostConfiguration(baseUri);
            configuration.Routes.MapHttpRoute("Default", "{controller}", new { controller = "Conneg" });
            server = new HttpSelfHostServer(configuration);
            server.OpenAsync().Wait();

            httpClient = new HttpClient();
        }

        public void CleanupHost()
        {
            if (server != null)
            {
                server.CloseAsync().Wait();
            }

            if (httpClient != null)
            {
                httpClient.Dispose();
            }
        }
    }
}
