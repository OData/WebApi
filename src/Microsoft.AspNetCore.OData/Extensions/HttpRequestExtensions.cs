// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class HttpRequestExtensions
    {
        internal const string ODataServiceVersionHeader = "OData-Version";
        internal const string ODataMaxServiceVersionHeader = "OData-MaxVersion";
        internal const ODataVersion DefaultODataVersion = ODataVersion.V4;

        /// <summary>
        /// Gets the <see cref="IODataFeature"/> from the services container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataFeature"/> from the services container.</returns>
        public static IODataFeature ODataFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.HttpContext.ODataFeature();
        }

        /// <summary>
        /// Gets the <see cref="IETagHandler"/> from the services container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IETagHandler"/> from the services container.</returns>
        public static IETagHandler GetETagHandler(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.HttpContext.GetETagHandler();
        }

        /// <summary>
        /// Gets the <see cref="IODataPathHandler"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataPathHandler"/> from the request container.</returns>
        public static IODataPathHandler GetPathHandler(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<IODataPathHandler>();
        }

        /// <summary>
        /// Gets the <see cref="IEdmModel"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IEdmModel"/> from the request container.</returns>
        public static IEdmModel GetModel(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<IEdmModel>();
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageReaderSettings"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="ODataMessageReaderSettings"/> from the request container.</returns>
        public static ODataMessageReaderSettings GetReaderSettings(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<ODataMessageReaderSettings>();
        }

        /// <summary>
        /// Creates a link for the next page of results; To be used as the value of @odata.nextLink.
        /// </summary>
        /// <param name="request">The request on which to base the next page link.</param>
        /// <param name="pageSize">The number of results allowed per page.</param>
        /// <returns>A next page link.</returns>
        public static Uri GetNextPageLink(this HttpRequest request, int pageSize)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the dependency injection container for the OData request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The dependency injection container.</returns>
        public static IServiceProvider GetRequestContainer(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IServiceProvider requestContainer = request.HttpContext.ODataFeature().RequestContainer;
            if (requestContainer != null)
            {
                return requestContainer;
            }

            // HTTP routes will not have chance to call CreateRequestContainer. We have to call it.
            return request.CreateRequestContainer(request.HttpContext.ODataFeature().RouteName);
        }

        /// <summary>
        /// Creates a request container that associates with the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <returns>The request container created.</returns>
        public static IServiceProvider CreateRequestContainer(this HttpRequest request, string routeName)
        {
            if (request.HttpContext.ODataFeature().RequestContainer != null)
            {
                throw Error.InvalidOperation(SRResources.RequestContainerAlreadyExists);
            }

            IServiceScope requestScope = request.CreateRequestScope(routeName);
            IServiceProvider requestContainer = requestScope.ServiceProvider;

            request.HttpContext.ODataFeature().RequestScope = requestScope;
            request.HttpContext.ODataFeature().RequestContainer = requestContainer;

            return requestContainer;
        }

        /// <summary>
        /// Deletes the request container from the <paramref name="request"/> and disposes
        /// the container if <paramref name="dispose"/> is <c>true</c>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="dispose">
        /// Returns <c>true</c> to dispose the request container after deletion; <c>false</c> otherwise.
        /// </param>
        public static void DeleteRequestContainer(this HttpRequest request, bool dispose)
        {
            if (request.HttpContext.ODataFeature().RequestScope != null)
            {
                IServiceScope requestScope = request.HttpContext.ODataFeature().RequestScope;
                request.HttpContext.ODataFeature().RequestScope = null;
                request.HttpContext.ODataFeature().RequestContainer = null;

                if (dispose)
                {
                    requestScope.Dispose();
                }
            }
        }

        /// <summary>
        /// Create a scoped request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The route name.</param>
        /// <returns></returns>
        private static IServiceScope CreateRequestScope(this HttpRequest request, string routeName)
        {
            IServiceProvider rootContainer;
            if (string.IsNullOrEmpty(routeName))
            {
                // For HTTP routes, use the default request services.
                rootContainer = request.HttpContext.RequestServices;
            }
            else
            {
                // For OData routes, create a scoped requested from the per-route container.
                IPerRouteContainer perRouteContainer = request.HttpContext.RequestServices.GetRequiredService<IPerRouteContainer>();
                if (perRouteContainer == null)
                {
                    throw Error.ArgumentNull("routeName");
                }

                rootContainer = perRouteContainer.GetODataRootContainer(routeName);
                if (perRouteContainer == null)
                {
                    throw Error.ArgumentNull("routeName");
                }
            }

            return rootContainer.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}