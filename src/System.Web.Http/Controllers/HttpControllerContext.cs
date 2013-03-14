// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Routing;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Contains information for a single HTTP operation.
    /// </summary>
    public class HttpControllerContext
    {
        private HttpConfiguration _configuration;
        private IHttpRouteData _routeData;
        private HttpRequestMessage _request;

        private HttpControllerDescriptor _controllerDescriptor;
        private IHttpController _controller;

        public HttpControllerContext(HttpConfiguration configuration, IHttpRouteData routeData, HttpRequestMessage request)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (routeData == null)
            {
                throw Error.ArgumentNull("routeData");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            _configuration = configuration;
            _routeData = routeData;
            _request = request;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpControllerContext"/> class.
        /// </summary>
        /// <remarks>The default constructor is intended for use by unit testing only.</remarks>
        public HttpControllerContext()
        {
        }

        public HttpConfiguration Configuration
        {
            get
            {
                return _configuration ?? (_request == null ? null : _request.GetConfiguration());
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _configuration = value;
                SyncWithRequest();
            }
        }

        public HttpRequestMessage Request
        {
            get { return _request; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _request = value;
                SyncWithRequest();
            }
        }

        public IHttpRouteData RouteData
        {
            get
            {
                return _routeData ?? (_request == null ? null : _request.GetRouteData());
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _routeData = value;
                SyncWithRequest();
            }
        }

        /// <summary>
        /// Gets or sets the controller descriptor.
        /// </summary>
        /// <value>
        /// The controller descriptor.
        /// </value>
        public HttpControllerDescriptor ControllerDescriptor
        {
            get { return _controllerDescriptor; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _controllerDescriptor = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTTP controller.
        /// </summary>
        /// <value>
        /// The HTTP controller.
        /// </value>
        public IHttpController Controller
        {
            get { return _controller; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _controller = value;
            }
        }

        // unit test only. syncs data on the controller context with the request.
        private void SyncWithRequest()
        {
            if (_request != null)
            {
                if (_configuration != null)
                {
                    _request.SetConfiguration(_configuration);
                }

                if (_routeData != null)
                {
                    _request.SetRouteData(_routeData);
                }

                // fall back to data from request.
                _routeData = null;
                _configuration = null;
            }
        }
    }
}
