// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Util;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// End to end functional tests for model binding
    /// </summary>
    public abstract class ModelBindingTests
    {
        protected HttpServer server = null;
        protected HttpConfiguration configuration = null;
        protected string baseAddress = null;
        protected HttpClient httpClient = null;

        protected ModelBindingTests()
        {
            this.SetupHost();
        }

        public void SetupHost()
        {
            baseAddress = "http://localhost/";

            configuration = new HttpConfiguration();
            configuration.Routes.MapHttpRoute("Default", "{controller}/{action}", new { controller = "ModelBinding" });
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.MessageHandlers.Add(new ConvertToStreamMessageHandler());

            server = new HttpServer(configuration);
            httpClient = new HttpClient(server);
        }
    }
}