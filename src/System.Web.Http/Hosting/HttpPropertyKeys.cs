// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Web.Http.Routing;

namespace System.Web.Http.Hosting
{
    /// <summary>
    /// Provides common keys for properties stored in the <see cref="HttpRequestMessage.Properties"/>
    /// </summary>
    public static class HttpPropertyKeys
    {
        /// <summary>
        /// Provides a key for the <see cref="HttpConfiguration"/> associated with this request.
        /// </summary>
        public static readonly string HttpConfigurationKey = "MS_HttpConfiguration";

        /// <summary>
        /// Provides a key for the <see cref="IHttpRouteData"/> associated with this request.
        /// </summary>
        public static readonly string HttpRouteDataKey = "MS_HttpRouteData";

        /// <summary>
        /// Provides a key for the current <see cref="SynchronizationContext"/> stored in <see cref="HttpRequestMessage.Properties"/>.
        /// If <see cref="SynchronizationContext.Current"/> is <c>null</c> then no context is stored.
        /// </summary>
        public static readonly string SynchronizationContextKey = "MS_SynchronizationContext";

        /// <summary>
        /// Provides a key for the collection of resources that should be disposed when a request is disposed.
        /// </summary>
        public static readonly string DisposableRequestResourcesKey = "MS_DisposableRequestResources";

        /// <summary>
        /// Provides a key for the dependency scope for this request.
        /// </summary>
        public static readonly string DependencyScope = "MS_DependencyScope";

        /// <summary>
        /// Provides a key for the <see cref="Guid"/> stored in <see cref="HttpRequestMessage.Properties"/>.
        /// This is the correlation id for that request.
        /// </summary>
        public static readonly string RequestCorrelationKey = "MS_RequestId";
    }
}
