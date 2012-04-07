// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using System.Web.Http.Services;

namespace System.Web.Http
{
    /// <summary>
    /// Configuration of <see cref="HttpServer"/> instances.
    /// </summary>
    public class HttpConfiguration : IDisposable
    {
        private readonly HttpRouteCollection _routes;
        private readonly ConcurrentDictionary<object, object> _properties = new ConcurrentDictionary<object, object>();
        private readonly MediaTypeFormatterCollection _formatters = DefaultFormatters();
        private readonly Collection<DelegatingHandler> _messageHandlers = new Collection<DelegatingHandler>();
        private readonly HttpFilterCollection _filters = new HttpFilterCollection();

        private IDependencyResolver _dependencyResolver = EmptyResolver.Instance;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpConfiguration"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "The route collection is disposed as part of this class.")]
        public HttpConfiguration()
            : this(new HttpRouteCollection(String.Empty))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpConfiguration"/> class.
        /// </summary>
        /// <param name="routes">The <see cref="HttpRouteCollection"/> to associate with this instance.</param>
        public HttpConfiguration(HttpRouteCollection routes)
        {
            if (routes == null)
            {
                throw Error.ArgumentNull("routes");
            }

            _routes = routes;
            Services = new DefaultServices(this);
        }

        /// <summary>
        /// Gets the list of filters that apply to all requests served using this HttpConfiguration instance.
        /// </summary>
        public HttpFilterCollection Filters
        {
            get { return _filters; }
        }

        /// <summary>
        /// Gets an ordered list of <see cref="DelegatingHandler"/> instances to be invoked as an
        /// <see cref="HttpRequestMessage"/> travels up the stack and an <see cref="HttpResponseMessage"/> travels down in
        /// stack in return. The handlers are invoked in a bottom-up fashion in the incoming path and top-down in the outgoing 
        /// path. That is, the last entry is called first for an incoming request message but invoked last for an outgoing 
        /// response message.
        /// </summary>
        /// <value>
        /// The message handler collection.
        /// </value>
        public Collection<DelegatingHandler> MessageHandlers
        {
            get { return _messageHandlers; }
        }

        /// <summary>
        /// Gets the <see cref="HttpRouteCollection"/> associated with this <see cref="HttpServer"/> instance.
        /// </summary>
        /// <value>
        /// The <see cref="HttpRouteCollection"/>.
        /// </value>
        public HttpRouteCollection Routes
        {
            get { return _routes; }
        }

        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        public ConcurrentDictionary<object, object> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Gets the root virtual path. The <see cref="VirtualPathRoot"/> property always returns 
        /// "/" as the first character of the returned value.
        /// </summary>
        public string VirtualPathRoot
        {
            get { return _routes.VirtualPathRoot; }
        }

        /// <summary>
        /// Gets or sets the dependency resolver associated with this <see cref="HttpConfiguration"/>.
        /// </summary>
        public IDependencyResolver DependencyResolver
        {
            get { return _dependencyResolver; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _dependencyResolver = value;
            }
        }

        /// <summary>
        /// Gets the container of default services associated with this <see cref="HttpConfiguration"/>.
        /// Only supports the list of service types documented on <see cref="DefaultServices"/>. For general
        /// purpose types, please use <see cref="DependencyResolver"/>.
        /// </summary>
        public DefaultServices Services { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether error details should be included in error messages.
        /// </summary>
        public IncludeErrorDetailPolicy IncludeErrorDetailPolicy { get; set; }

        /// <summary>
        /// Gets the media type formatters.
        /// </summary>
        public MediaTypeFormatterCollection Formatters
        {
            get { return _formatters; }
        }

        private static MediaTypeFormatterCollection DefaultFormatters()
        {
            var formatters = new MediaTypeFormatterCollection();

            // Basic FormUrlFormatter does not support binding to a T. 
            // Use our JQuery formatter instead.
            formatters.Add(new JQueryMvcFormUrlEncodedFormatter());

            return formatters;
        }

        internal bool ShouldIncludeErrorDetail(HttpRequestMessage request)
        {
            switch (IncludeErrorDetailPolicy)
            {
                case IncludeErrorDetailPolicy.LocalOnly:
                    Uri requestUri = request.RequestUri;
                    return requestUri.IsAbsoluteUri && requestUri.IsLoopback;

                case IncludeErrorDetailPolicy.Always:
                    return true;

                case IncludeErrorDetailPolicy.Never:
                default:
                    return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                if (disposing)
                {
                    _routes.Dispose();
                    Services.Dispose();
                    DependencyResolver.Dispose();
                }
            }
        }
    }
}
