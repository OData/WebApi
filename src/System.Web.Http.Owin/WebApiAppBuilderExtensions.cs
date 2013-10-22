// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Owin;

namespace Owin
{
    /// <summary>
    /// Provides extension methods for the <see cref="IAppBuilder"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WebApiAppBuilderExtensions
    {
        private static readonly IHostBufferPolicySelector _defaultBufferPolicySelector = new OwinBufferPolicySelector();

        /// <summary>
        /// Adds a component to the OWIN pipeline for running a Web API endpoint.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> used to configure the endpoint.</param>
        /// <returns>The application builder.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Disposed by HttpMessageHandlerAdapter in the success path.")]
        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpConfiguration configuration)
        {
            HttpServer server = new HttpServer(configuration);

            try
            {
                HttpMessageHandlerOptions options = CreateOptions(server, configuration);
                return UseMessageHandler(builder, options);
            }
            catch
            {
                server.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Adds a component to the OWIN pipeline for running a Web API endpoint.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="httpServer">The http server.</param>
        /// <returns>The application builder.</returns>
        public static IAppBuilder UseWebApi(this IAppBuilder builder, HttpServer httpServer)
        {
            HttpMessageHandlerOptions options = CreateOptions(httpServer, httpServer.Configuration);
            return UseMessageHandler(builder, options);
        }

        private static IAppBuilder UseMessageHandler(this IAppBuilder builder, HttpMessageHandlerOptions options)
        {
            return builder.Use(typeof(HttpMessageHandlerAdapter), options);
        }

        private static HttpMessageHandlerOptions CreateOptions(HttpServer server, HttpConfiguration configuration)
        {
            IHostBufferPolicySelector bufferPolicySelector = configuration.Services.GetHostBufferPolicySelector()
                ?? _defaultBufferPolicySelector;

            return new HttpMessageHandlerOptions
            {
                MessageHandler = server,
                BufferPolicySelector = bufferPolicySelector
            };
        }
    }
}
