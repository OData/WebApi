// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestExtensions"/>.
    /// </summary>
    public static class HttpRequestExtensions
    {
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
        /// Extension method to return the <see cref="IUrlHelper"/> from the <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="request">The Http request.</param>
        /// <returns>The <see cref="IUrlHelper"/>.</returns>
        public static IUrlHelper GetUrlHelper(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            IODataFeature feature = request.ODataFeature();
            if (feature.UrlHelper == null)
            {
                // if not set, get it from global.
                feature.UrlHelper = request.HttpContext.GetUrlHelper();
            }

            return feature.UrlHelper;
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

            return request.GetRequestContainer().GetRequiredService<IETagHandler>();
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
        /// Gets the <see cref="ODataMessageWriterSettings"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="ODataMessageWriterSettings"/> from the request container.</returns>
        public static ODataMessageWriterSettings GetWriterSettings(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<ODataMessageWriterSettings>();
        }

        internal static bool IsCountRequest(this HttpRequest request)
        {
            ODataPath path = request.ODataFeature().Path;
            return path != null && path.Segments.LastOrDefault() is CountSegment;
        }

        internal static bool IsRawValueRequest(this HttpRequest request)
        {
            ODataPath path = request.ODataFeature().Path;
            return path != null && path.Segments.LastOrDefault() is ValueSegment;
        }

        /// <summary>
        /// Gets the set of <see cref="IODataRoutingConvention"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The set of <see cref="IODataRoutingConvention"/> from the request container.</returns>
        public static IEnumerable<IODataRoutingConvention> GetRoutingConventions(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetServices<IODataRoutingConvention>();
        }

        /// <summary>
        /// Gets the OData <see cref="ETag"/> from the given request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entityTagHeaderValue">The entity tag header value.</param>
        /// <returns>The parsed <see cref="ETag"/>.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
        public static ETag GetETag(this HttpRequest request, EntityTagHeaderValue entityTagHeaderValue)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (entityTagHeaderValue != null)
            {
                if (entityTagHeaderValue.Equals(EntityTagHeaderValue.Any))
                {
                    return new ETag { IsAny = true };
                }

                // get the etag handler, and parse the etag
                IETagHandler etagHandler = request.GetRequestContainer().GetRequiredService<IETagHandler>();
                IDictionary<string, object> properties = etagHandler.ParseETag(entityTagHeaderValue) ?? new Dictionary<string, object>();
                IList<object> parsedETagValues = properties.Select(property => property.Value).AsList();

                // get property names from request
                ODataPath odataPath = request.ODataFeature().Path;
                IEdmModel model = request.GetModel();
                IEdmNavigationSource source = odataPath.NavigationSource;
                if (model != null && source != null)
                {
                    IList<IEdmStructuralProperty> concurrencyProperties = model.GetConcurrencyProperties(source).ToList();
                    IList<string> concurrencyPropertyNames = concurrencyProperties.OrderBy(c => c.Name).Select(c => c.Name).AsList();
                    ETag etag = new ETag();

                    if (parsedETagValues.Count != concurrencyPropertyNames.Count)
                    {
                        etag.IsWellFormed = false;
                    }

                    IEnumerable<KeyValuePair<string, object>> nameValues = concurrencyPropertyNames.Zip(
                        parsedETagValues,
                        (name, value) => new KeyValuePair<string, object>(name, value));
                    foreach (var nameValue in nameValues)
                    {
                        IEdmStructuralProperty property = concurrencyProperties.SingleOrDefault(e => e.Name == nameValue.Key);
                        Contract.Assert(property != null);

                        Type clrType = EdmLibHelpers.GetClrType(property.Type, model);
                        Contract.Assert(clrType != null);

                        if (nameValue.Value != null)
                        {
                            Type valueType = nameValue.Value.GetType();
                            etag[nameValue.Key] = valueType != clrType
                                ? Convert.ChangeType(nameValue.Value, clrType, CultureInfo.InvariantCulture)
                                : nameValue.Value;
                        }
                        else
                        {
                            etag[nameValue.Key] = nameValue.Value;
                        }
                    }

                    return etag;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="ETag{TEntity}"/> from the given request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entityTagHeaderValue">The entity tag header value.</param>
        /// <returns>The parsed <see cref="ETag{TEntity}"/>.</returns>
        public static ETag<TEntity> GetETag<TEntity>(this HttpRequest request, EntityTagHeaderValue entityTagHeaderValue)
        {
            ETag etag = request.GetETag(entityTagHeaderValue);
            return etag != null
                ? new ETag<TEntity>
                {
                    ConcurrencyProperties = etag.ConcurrencyProperties,
                    IsWellFormed = etag.IsWellFormed,
                    IsAny = etag.IsAny,
                }
                : null;
        }
        /// <summary>
        /// Creates a link for the next page of results; To be used as the value of @odata.nextLink.
        /// </summary>
        /// <param name="request">The request on which to base the next page link.</param>
        /// <param name="pageSize">The number of results allowed per page.</param>
        /// <returns>A next page link.</returns>
        public static Uri GetNextPageLink(this HttpRequest request, int pageSize)
        {
            return GetNextPageLink(request, pageSize, null, null);
        }

        /// <summary>
        /// Creates a link for the next page of results; To be used as the value of @odata.nextLink.
        /// </summary>
        /// <param name="request">The request on which to base the next page link.</param>
        /// <param name="pageSize">The number of results allowed per page.</param>
        /// <param name="instance">Object which can be used to generate the skiptoken value.</param>
        /// <param name="objectToSkipTokenValue">Function that takes in the last object and returns the skiptoken value string.</param>
        /// <returns>A next page link.</returns>
        public static Uri GetNextPageLink(this HttpRequest request, int pageSize, object instance, Func<object,string> objectToSkipTokenValue)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            UriBuilder uriBuilder = new UriBuilder(request.Scheme, request.Host.Host) 
            { 
                Path = (request.PathBase + request.Path).ToUriComponent() 
            };
            if (request.Host.Port.HasValue)
            {
                uriBuilder.Port = request.Host.Port.Value;
            }

            CompatibilityOptions compatibilityOptions = request.GetCompatibilityOptions();

            IEnumerable<KeyValuePair<string, string>> queryParameters = request.Query.SelectMany(kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value));
            return GetNextPageHelper.GetNextPageLink(uriBuilder.Uri, queryParameters, pageSize, instance, objectToSkipTokenValue, compatibilityOptions);
        }

        /// <summary>
        /// Gets the set of flags for <see cref="CompatibilityOptions"/> from ODataOptions. 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Set of flags for <see cref="CompatibilityOptions"/> from ODataOptions.</returns>
        internal static CompatibilityOptions GetCompatibilityOptions(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (request.HttpContext == null)
            {
                return CompatibilityOptions.None;
            }

            ODataOptions options = request.HttpContext.RequestServices.GetRequiredService<ODataOptions>();

            if (options == null)
            {
                return CompatibilityOptions.None;
            }

            return options.CompatibilityOptions;
        }

        internal static ODataVersion? ODataServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return GetODataVersionFromHeader(request.Headers, ODataVersionConstraint.ODataServiceVersionHeader);
        }

        internal static ODataVersion? ODataMaxServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return GetODataVersionFromHeader(request.Headers, ODataVersionConstraint.ODataMaxServiceVersionHeader);
        }

        internal static ODataVersion? ODataMinServiceVersion(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return GetODataVersionFromHeader(request.Headers, ODataVersionConstraint.ODataMinServiceVersionHeader);
        }

        private static ODataVersion? GetODataVersionFromHeader(IHeaderDictionary headers, string headerName)
        {
            StringValues values;
            if (headers.TryGetValue(headerName, out values))
            {
                string value = values.FirstOrDefault();
                if (value != null)
                {
                    string trimmedValue = value.Trim(' ', ';');
                    try
                    {
                        return ODataUtils.StringToODataVersion(trimmedValue);
                    }
                    catch (ODataException)
                    {
                        // Parsing the odata version failed.
                    }
                }
            }

            return null;
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

            IServiceProvider requestContainer = request.ODataFeature().RequestContainer;
            if (requestContainer != null)
            {
                return requestContainer;
            }

            // HTTP routes will not have chance to call CreateRequestContainer. We have to call it.
            return request.CreateRequestContainer(request.ODataFeature().RouteName);
        }

        /// <summary>
        /// Creates a request container that associates with the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <returns>The request container created.</returns>
        public static IServiceProvider CreateRequestContainer(this HttpRequest request, string routeName)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (request.ODataFeature().RequestContainer != null)
            {
                throw Error.InvalidOperation(SRResources.RequestContainerAlreadyExists);
            }

            IServiceScope requestScope = request.CreateRequestScope(routeName);
            IServiceProvider requestContainer = requestScope.ServiceProvider;

            request.ODataFeature().RequestScope = requestScope;
            request.ODataFeature().RequestContainer = requestContainer;

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
            if (request.ODataFeature().RequestScope != null)
            {
                IServiceScope requestScope = request.ODataFeature().RequestScope;
                request.ODataFeature().RequestScope = null;
                request.ODataFeature().RequestContainer = null;

                if (dispose)
                {
                    requestScope.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks whether the request is a POST targeted at a resource path ending in /$query.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="oDataPath">The OData path.</param>
        /// <returns>true if the request path has $query segment.</returns>
        internal static bool IsQueryRequest(this HttpRequest request, string oDataPath)
        {
            return request.Method.Equals(HttpMethods.Post) && 
                oDataPath?.TrimEnd('/').EndsWith('/' + ODataRouteConstants.QuerySegment, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Transforms a POST request targeted at a resource path ending in $query into a GET request. 
        /// The query options are parsed from the request body and appended to the request URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestContainerFactory">Request container factory.</param>
        internal static void TransformQueryRequest(this HttpRequest request, Func<IServiceProvider> requestContainerFactory)
        {
            if (requestContainerFactory == null)
            {
                throw Error.ArgumentNull("requestContainerFactory");
            }

            IServiceProvider requestContainer = requestContainerFactory();

            // Fetch parsers available in the request container for parsing the query options in the request body
            IEnumerable<IODataQueryOptionsParser> queryOptionsParsers = requestContainer.GetRequiredService<IEnumerable<IODataQueryOptionsParser>>();
            IODataQueryOptionsParser queryOptionsParser = queryOptionsParsers.FirstOrDefault(
                d => d.MediaTypeMapping.TryMatchMediaType(request) > 0);

            if (queryOptionsParser == null)
            {
                throw new ODataException(string.Format(
                    CultureInfo.InvariantCulture, 
                    SRResources.CannotFindParserForRequestMediaType,
                    request.ContentType));
            }

            string queryString = queryOptionsParser.ParseAsync(request.Body).Result;

            // Request path starts with a /
            string requestPath = request.Path.Value;
            requestPath = requestPath.Substring(0, requestPath.LastIndexOf('/' + ODataRouteConstants.QuerySegment, StringComparison.OrdinalIgnoreCase));

            request.Path = new PathString(requestPath);
            request.QueryString = new QueryString(queryString);
            request.Method = HttpMethods.Get;
        }

        /// <summary>
        /// Create a scoped request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The route name.</param>
        /// <returns></returns>
        private static IServiceScope CreateRequestScope(this HttpRequest request, string routeName)
        {
            IPerRouteContainer perRouteContainer = request.HttpContext.RequestServices.GetRequiredService<IPerRouteContainer>();
            if (perRouteContainer == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
            }

            IServiceProvider rootContainer = perRouteContainer.GetODataRootContainer(routeName);
            IServiceScope scope = rootContainer.GetRequiredService<IServiceScopeFactory>().CreateScope();

            // Bind scoping request into the OData container.
            if (!string.IsNullOrEmpty(routeName))
            {
                scope.ServiceProvider.GetRequiredService<HttpRequestScope>().HttpRequest = request;
            }

            return scope;
        }
    }
}