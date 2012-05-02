// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.WebHost;
using System.Web.Http.WebHost.Routing;
using System.Web.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Provides a global <see cref="T:System.Web.Http.HttpConfiguration"/> for ASP.NET applications.
    /// </summary>
    public static class GlobalConfiguration
    {
        private static Lazy<HttpConfiguration> _configuration = new Lazy<HttpConfiguration>(
            () =>
            {
                HttpConfiguration config = new HttpConfiguration(new HostedHttpRouteCollection(RouteTable.Routes));
                config.Services.Replace(typeof(IAssembliesResolver), new WebHostAssembliesResolver());
                config.Services.Replace(typeof(IHttpControllerTypeResolver), new WebHostHttpControllerTypeResolver());
                config.Services.Replace(typeof(IHostBufferPolicySelector), new WebHostBufferPolicySelector());
                return config;
            });

        private static Lazy<HttpMessageHandler> _defaultHandler = new Lazy<HttpMessageHandler>(
            () => new HttpRoutingDispatcher(_configuration.Value));

        /// <summary>
        /// Gets the global <see cref="T:System.Web.Http.HttpConfiguration"/>.
        /// </summary>
        public static HttpConfiguration Configuration
        {
            get { return _configuration.Value; }
        }

        /// <summary>
        /// Gets the default message handler that will be called for all requests.
        /// </summary>
        public static HttpMessageHandler DefaultHandler
        {
            get { return _defaultHandler.Value; }
        }
    }
}
