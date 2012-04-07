// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.SelfHost;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding
    /// </summary>
    public abstract class ModelBindingTests : IDisposable
    {
        protected HttpSelfHostServer server = null;
        protected HttpSelfHostConfiguration configuration = null;
        protected string baseAddress = null;
        protected HttpClient httpClient = null;

        protected ModelBindingTests()
        {
            this.SetupHost();
        }

        public void Dispose()
        {
            this.CleanupHost();
        }

        public void SetupHost()
        {
            httpClient = new HttpClient();

            baseAddress = String.Format("http://{0}/", Environment.MachineName);

            configuration = new HttpSelfHostConfiguration(baseAddress);
            configuration.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "ModelBinding" });

            server = new HttpSelfHostServer(configuration);
            server.OpenAsync().Wait();
        }

        public void CleanupHost()
        {
            if (server != null)
            {
                server.CloseAsync().Wait();
            }
        }
    }
}