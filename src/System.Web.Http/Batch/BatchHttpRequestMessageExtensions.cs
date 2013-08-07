// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace System.Web.Http.Batch
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BatchHttpRequestMessageExtensions
    {
        private const string HttpBatchContextKey = "MS_HttpBatchContext";

        private static readonly string[] BatchRequestPropertyExclusions =
        {
            HttpPropertyKeys.HttpRouteDataKey,
            HttpPropertyKeys.DisposableRequestResourcesKey,
            HttpPropertyKeys.SynchronizationContextKey,
            HttpPropertyKeys.HttpConfigurationKey,
            HttpBatchContextKey
        };

        /// <summary>
        /// Copies the properties from another <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="subRequest">The sub-request.</param>
        /// <param name="batchRequest">The batch request that contains the properties to copy.</param>
        public static void CopyBatchRequestProperties(this HttpRequestMessage subRequest, HttpRequestMessage batchRequest)
        {
            if (subRequest == null)
            {
                throw new ArgumentNullException("subRequest");
            }
            if (batchRequest == null)
            {
                throw new ArgumentNullException("batchRequest");
            }

            foreach (KeyValuePair<string, object> property in batchRequest.Properties)
            {
                if (!BatchRequestPropertyExclusions.Contains(property.Key))
                {
                    subRequest.Properties.Add(property);
                }
            }

            HttpRequestContext originalContext = subRequest.GetRequestContext();

            if (originalContext != null)
            {
                subRequest.SetRequestContext(new BatchHttpRequestContext(originalContext));
            }
        }

        private class BatchHttpRequestContext : HttpRequestContext
        {
            private readonly HttpRequestContext _batchContext;

            public BatchHttpRequestContext(HttpRequestContext batchContext)
            {
                if (batchContext == null)
                {
                    throw new ArgumentNullException("batchContext");
                }

                _batchContext = batchContext;
            }

            public override X509Certificate2 ClientCertificate
            {
                get
                {
                    return _batchContext.ClientCertificate;
                }
                set
                {
                    _batchContext.ClientCertificate = value;
                }
            }

            public override HttpConfiguration Configuration
            {
                get
                {
                    // Use separate route data for configuration (base, not _batchContext).
                    return base.Configuration;
                }
                set
                {
                    // Use separate route data for configuration (base, not _batchContext).
                    base.Configuration = value;
                }
            }

            public override bool IncludeErrorDetail
            {
                get
                {
                    return _batchContext.IncludeErrorDetail;
                }
                set
                {
                    _batchContext.IncludeErrorDetail = value;
                }
            }

            public override bool IsLocal
            {
                get
                {
                    return _batchContext.IsLocal;
                }
                set
                {
                    _batchContext.IsLocal = value;
                }
            }

            public override IPrincipal Principal
            {
                get
                {
                    return _batchContext.Principal;
                }
                set
                {
                    _batchContext.Principal = value;
                }
            }

            public override IHttpRouteData RouteData
            {
                get
                {
                    // Use separate route data for batching (base, not _batchContext).
                    return base.RouteData;
                }
                set
                {
                    // Use separate route data for batching (base, not _batchContext).
                    base.RouteData = value;
                }
            }

            public override UrlHelper Url
            {
                get
                {
                    // Use a separate URL factory for batching (base, not _batchContext).
                    return base.Url;
                }
                set
                {
                    // Use a separate URL factory for batching (base, not _batchContext).
                    base.Url = value;
                }
            }

            public override string VirtualPathRoot
            {
                get
                {
                    return _batchContext.VirtualPathRoot;
                }
                set
                {
                    _batchContext.VirtualPathRoot = value;
                }
            }
        }
    }
}