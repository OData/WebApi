// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

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
        private UrlHelper _urlHelper;

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
            get { return _configuration; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _configuration = value;
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
            }
        }

        public IHttpRouteData RouteData
        {
            get { return _routeData; }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _routeData = value;
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

        /// <summary>
        /// Returns an instance of a UrlHelper, which is used to generate URLs to other APIs.
        /// </summary>
        public UrlHelper Url
        {
            get
            {
                if (_urlHelper == null)
                {
                    _urlHelper = new UrlHelper(this);
                }
                return _urlHelper;
            }
            set { _urlHelper = value; }
        }
    }
}
