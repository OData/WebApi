// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading;
using System.Web.Http.Routing;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// Represents a request context that does not change the behavior of legacy properties.
    /// Once a request context is set, things like route data and client certificate start
    /// coming from it instead of from the legacy request properties. This class is a request
    /// context that will do the same thing as the legacy request properties would have done
    /// while still allowing GetRequestContext to return non-null.
    /// </summary>
    internal sealed class HttpLegacyRequestContext : HttpRequestContext
    {
        private readonly HttpRequestMessage _request;

        private X509Certificate2 _certificate;
        private bool _certificateSet;

        private HttpConfiguration _configuration;
        private bool _configurationSet;

        private bool? _includeErrorDetail;
        private bool? _isLocal;

        private IHttpRouteData _routeData;
        private bool _routeDataSet;

        private string _virtualPathRoot;
        private bool _virtualPathRootSet;

        public HttpLegacyRequestContext(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            _request = request;

            // We didn't have to override Url and Principal the base class provides the simple property, and that's sufficient.
            Url = new UrlHelper(request);
            Principal = Thread.CurrentPrincipal;
        }

        public override X509Certificate2 ClientCertificate
        {
            get
            {
                if (_certificateSet)
                {
                    return _certificate;
                }
                else
                {
                    return _request.LegacyGetClientCertificate();
                }
            }
            set
            {
                _certificate = value;
                _certificateSet = true;
            }
        }

        public override HttpConfiguration Configuration
        {
            get
            {
                if (_configurationSet)
                {
                    return _configuration;
                }

                return _request.LegacyGetConfiguration();
            }
            set
            {
                _configuration = value;
                _configurationSet = true;
            }
        }

        public override bool IncludeErrorDetail
        {
            get
            {
                if (_includeErrorDetail.HasValue)
                {
                    return _includeErrorDetail.Value;
                }

                return _request.LegacyShouldIncludeErrorDetail();
            }
            set
            {
                _includeErrorDetail = value;
            }
        }

        public override bool IsLocal
        {
            get
            {
                if (_isLocal.HasValue)
                {
                    return _isLocal.Value;
                }

                return _request.LegacyIsLocal();
            }
            set
            {
                _isLocal = value;
            }
        }

        public override IHttpRouteData RouteData
        {
            get
            {
                if (_routeDataSet)
                {
                    return _routeData;
                }

                return _request.LegacyGetRouteData();
            }
            set
            {
                _routeData = value;
                _routeDataSet = true;
            }
        }

        public override string VirtualPathRoot
        {
            get
            {
                if (_virtualPathRootSet)
                {
                    return _virtualPathRoot;
                }

                HttpConfiguration configuration = Configuration;

                if (configuration != null)
                {
                    return configuration.VirtualPathRoot;
                }

                return null;
            }
            set
            {
                _virtualPathRoot = value;
                _virtualPathRootSet = true;
            }
        }
    }
}