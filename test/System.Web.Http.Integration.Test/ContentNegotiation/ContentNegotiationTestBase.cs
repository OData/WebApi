// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Util;

namespace System.Web.Http.ContentNegotiation
{
    public class ContentNegotiationTestBase
    {
        protected readonly string baseUri = "http://localhost/Conneg";
        protected HttpServer server = null;
        protected HttpConfiguration configuration = null;
        protected HttpClient httpClient = null;

        public ContentNegotiationTestBase()
        {
            this.SetupHost();
        }

        public void SetupHost()
        {
            configuration = new HttpConfiguration();
            configuration.Routes.MapHttpRoute("Default", "{controller}", new { controller = "Conneg" });
            configuration.MessageHandlers.Add(new ConvertToStreamMessageHandler());

            server = new HttpServer(configuration);

            httpClient = new HttpClient(server);
        }
    }
}
