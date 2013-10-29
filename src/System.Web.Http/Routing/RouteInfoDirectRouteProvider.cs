// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;

namespace System.Web.Http.Routing
{
    /// <remarks>
    /// This class is an adapter that turns an IHttpRouteInfoProvider into an IDirectRouteProvider. We need it because
    /// we already shipped IHttpRouteInfoProvider but want to standardize the internal implementation around the more
    /// general IDirectRouteProvider interface.
    /// We can remove this class if we ever stop supporting custom attributes that implement IHttpRouteInfoProvider.
    /// </remarks>
    internal class RouteInfoDirectRouteProvider : IDirectRouteProvider
    {
        private readonly IHttpRouteInfoProvider _infoProvider;

        public RouteInfoDirectRouteProvider(IHttpRouteInfoProvider infoProvider)
        {
            if (infoProvider == null)
            {
                throw new ArgumentNullException("infoProvider");
            }

            _infoProvider = infoProvider;
        }

        public HttpRouteEntry CreateRoute(DirectRouteProviderContext context)
        {
            Contract.Assert(context != null);

            DirectRouteBuilder builder = context.CreateBuilder(_infoProvider.Template);
            Contract.Assert(builder != null);

            builder.Name = _infoProvider.Name;
            builder.Order = _infoProvider.Order;

            return builder.Build();
        }
    }
}
