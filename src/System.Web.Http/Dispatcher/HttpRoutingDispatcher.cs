// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using System.Web.Http.Properties;
using System.Web.Http.Routing;

namespace System.Web.Http.Dispatcher
{
    /// <summary>
    /// This class is the default endpoint message handler which examines the <see cref="IHttpRoute"/>
    /// of the matched route, and chooses which message handler to call. If <see cref="IHttpRoute.Handler"/>
    /// is <c>null</c>, then it delegates to <see cref="HttpControllerDispatcher"/>.
    /// </summary>
    public class HttpRoutingDispatcher : HttpMessageHandler
    {
        private readonly HttpConfiguration _configuration;
        private readonly HttpMessageInvoker _defaultInvoker;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRoutingDispatcher"/> class,
        /// using the provided <see cref="HttpConfiguration"/> and <see cref="HttpControllerDispatcher"/>
        /// as the default handler.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "HttpControllerDispatcher does not require disposal")]
        public HttpRoutingDispatcher(HttpConfiguration configuration)
            : this(configuration, new HttpControllerDispatcher(configuration))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRoutingDispatcher"/> class,
        /// using the provided <see cref="HttpConfiguration"/> and <see cref="HttpMessageHandler"/>.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="defaultHandler">The default handler to use when the <see cref="IHttpRoute"/> has no <see cref="IHttpRoute.Handler"/>.</param>
        public HttpRoutingDispatcher(HttpConfiguration configuration, HttpMessageHandler defaultHandler)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (defaultHandler == null)
            {
                throw Error.ArgumentNull("defaultHandler");
            }

            _configuration = configuration;
            _defaultInvoker = new HttpMessageInvoker(defaultHandler);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The Web API framework will dispose of the response after sending it")]
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Lookup route data, or if not found as a request property then we look it up in the route table
            IHttpRouteData routeData;
            if (!request.Properties.TryGetValue(HttpPropertyKeys.HttpRouteDataKey, out routeData))
            {
                routeData = _configuration.Routes.GetRouteData(request);
                if (routeData != null)
                {
                    request.Properties.Add(HttpPropertyKeys.HttpRouteDataKey, routeData);
                }
                else
                {
                    return TaskHelpers.FromResult(request.CreateErrorResponse(
                        HttpStatusCode.NotFound,
                        Error.Format(SRResources.ResourceNotFound, request.RequestUri),
                        SRResources.NoRouteData));
                }
            }

            RemoveOptionalRoutingParameters(routeData.Values);

            var invoker = routeData.Route.Handler == null ? _defaultInvoker : new HttpMessageInvoker(routeData.Route.Handler, disposeHandler: false);
            return invoker.SendAsync(request, cancellationToken);
        }

        private static void RemoveOptionalRoutingParameters(IDictionary<string, object> routeValueDictionary)
        {
            Contract.Assert(routeValueDictionary != null);

            // Get all keys for which the corresponding value is 'Optional'.
            // Having a separate array is necessary so that we don't manipulate the dictionary while enumerating.
            // This is on a hot-path and linq expressions are showing up on the profile, so do array manipulation.
            int max = routeValueDictionary.Count;
            int i = 0;
            string[] matching = new string[max];
            foreach (KeyValuePair<string, object> kv in routeValueDictionary)
            {
                if (kv.Value == RouteParameter.Optional)
                {
                    matching[i] = kv.Key;
                    i++;
                }
            }
            for (int j = 0; j < i; j++)
            {
                string key = matching[j];
                routeValueDictionary.Remove(key);
            }
        }
    }
}
