// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        private const string PropertiesKey = "Microsoft.AspNet.OData.Properties";
        private const string RequestScopeKey = "Microsoft.AspNet.OData.RequestScope";
        internal const string RequestContainerKey = "Microsoft.AspNet.OData.RequestContainer";

        /// <summary>
        /// Gets the <see cref="HttpRequestMessageProperties"/> instance containing OData methods and properties
        /// for given <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="request">The request of interest.</param>
        /// <returns>
        /// An object through which OData methods and properties for given <paramref name="request"/> are available.
        /// </returns>
        public static HttpRequestMessageProperties ODataProperties(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            // Cache HttpRequestMessageProperties value to avoid lots of identical objects with no instance data.
            HttpRequestMessageProperties properties;
            object value;
            if (request.Properties.TryGetValue(PropertiesKey, out value))
            {
                properties = value as HttpRequestMessageProperties;
                Contract.Assert(properties != null);
            }
            else
            {
                properties = new HttpRequestMessageProperties(request);

                // Avoid race conditions: Do not use Add().  Worst case here is an extra HttpRequestMessageProperties
                // instance which will soon go out of scope.
                request.Properties[PropertiesKey] = properties;
            }

            return properties;
        }

        /// <summary>
        /// <para>Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/>
        /// representing an error with an instance of <see cref="ObjectContent{T}"/> wrapping
        /// <paramref name="oDataError"/> as the content. If no formatter is found, this method returns a response with
        /// status 406 NotAcceptable.</para>
        ///
        /// <para>This method requires that <paramref name="request"/> has been associated with an instance of
        /// <see cref="HttpConfiguration"/>.</para>
        /// </summary>
        /// <param name="request">The request of interest.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="oDataError">The OData error to wrap.</param>
        /// <returns>
        /// An error response wrapping <paramref name="oDataError"/> with status code <paramref name="statusCode"/>.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "o",
            Justification = "oDataError is spelled correctly.")]
        public static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request,
            HttpStatusCode statusCode, ODataError oDataError)
        {
            if (request.ShouldIncludeErrorDetail())
            {
                return request.CreateResponse(statusCode, oDataError);
            }
            else
            {
                return request.CreateResponse(
                    statusCode,
                    new ODataError
                    {
                        ErrorCode = oDataError.ErrorCode,
                        Message = oDataError.Message,
                    });
            }
        }

        /// <summary>
        /// Gets the OData <see cref="ETag"/> from the given request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entityTagHeaderValue">The entity tag header value.</param>
        /// <returns>The parsed <see cref="ETag"/>.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
        public static ETag GetETag(this HttpRequestMessage request, EntityTagHeaderValue entityTagHeaderValue)
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

                HttpConfiguration configuration = request.GetConfiguration();
                if (configuration == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
                }

                // get the etag handler, and parse the etag
                IDictionary<string, object> properties =
                    configuration.GetETagHandler().ParseETag(entityTagHeaderValue) ?? new Dictionary<string, object>();
                IList<object> parsedETagValues = properties.Select(property => property.Value).AsList();

                // get property names from request
                ODataPath odataPath = request.ODataProperties().Path;
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
        public static ETag<TEntity> GetETag<TEntity>(this HttpRequestMessage request, EntityTagHeaderValue entityTagHeaderValue)
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
        public static Uri GetNextPageLink(this HttpRequestMessage request, int pageSize)
        {
            return request.GetNextPageLink(pageSize, null, null);
        }

        /// <summary>
        /// Creates a link for the next page of results; To be used as the value of @odata.nextLink.
        /// </summary>
        /// <param name="request">The request on which to base the next page link.</param>
        /// <param name="pageSize">The number of results allowed per page.</param>
        /// <param name="instance">The instance based on which the skiptoken value is generated. </param>
        /// <param name="objToSkipTokenValue">Function that extracts out the skiptoken value from the instance.</param>
        /// <returns>A next page link.</returns>
        public static Uri GetNextPageLink(this HttpRequestMessage request, int pageSize, object instance, Func<object, string> objToSkipTokenValue)
        {
            if (request == null || request.RequestUri == null)
            {
                throw Error.ArgumentNull("request");
            }

            CompatibilityOptions options = request.GetCompatibilityOptions();

            return GetNextPageHelper.GetNextPageLink(request.RequestUri, request.GetQueryNameValuePairs(), pageSize, instance, objToSkipTokenValue, options);
        }

        /// <summary>
        /// Gets the dependency injection container for the OData request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The dependency injection container.</returns>
        public static IServiceProvider GetRequestContainer(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object value;
            if (request.Properties.TryGetValue(RequestContainerKey, out value))
            {
                return (IServiceProvider)value;
            }

            // HTTP routes will not have chance to call CreateRequestContainer.
            // We have to call it.
            return request.CreateRequestContainer(null);
        }

        /// <summary>
        /// Creates a request container that associates with the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The name of the route.</param>
        /// <returns>The request container created.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "requestScope will be disposed when the request ends.")]
        public static IServiceProvider CreateRequestContainer(this HttpRequestMessage request, string routeName)
        {
            if (request.Properties.ContainsKey(RequestContainerKey))
            {
                throw Error.InvalidOperation(SRResources.RequestContainerAlreadyExists);
            }

            IServiceScope requestScope = request.CreateRequestScope(routeName);
            IServiceProvider requestContainer = requestScope.ServiceProvider;

            request.Properties[RequestScopeKey] = requestScope;
            request.Properties[RequestContainerKey] = requestContainer;

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
        public static void DeleteRequestContainer(this HttpRequestMessage request, bool dispose)
        {
            object value;
            if (request.Properties.TryGetValue(RequestScopeKey, out value))
            {
                IServiceScope requestScope = (IServiceScope)value;
                Contract.Assert(requestScope != null);

                request.Properties.Remove(RequestScopeKey);
                request.Properties.Remove(RequestContainerKey);

                if (dispose)
                {
                    requestScope.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IEdmModel"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IEdmModel"/> from the request container.</returns>
        public static IEdmModel GetModel(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<IEdmModel>();
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageWriterSettings"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="ODataMessageWriterSettings"/> from the request container.</returns>
        public static ODataMessageWriterSettings GetWriterSettings(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<ODataMessageWriterSettings>();
        }

        /// <summary>
        /// Gets the <see cref="ODataMessageReaderSettings"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="ODataMessageReaderSettings"/> from the request container.</returns>
        public static ODataMessageReaderSettings GetReaderSettings(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<ODataMessageReaderSettings>();
        }

        /// <summary>
        /// Gets the <see cref="IODataPathHandler"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataPathHandler"/> from the request container.</returns>
        public static IODataPathHandler GetPathHandler(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<IODataPathHandler>();
        }

        /// <summary>
        /// Gets the <see cref="ODataSerializerProvider"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="ODataSerializerProvider"/> from the request container.</returns>
        public static ODataSerializerProvider GetSerializerProvider(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<ODataSerializerProvider>();
        }

        /// <summary>
        /// Gets the <see cref="ODataDeserializerProvider"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="ODataDeserializerProvider"/> from the request container.</returns>
        public static ODataDeserializerProvider GetDeserializerProvider(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetRequiredService<ODataDeserializerProvider>();
        }

        /// <summary>
        /// Gets the set of <see cref="IODataRoutingConvention"/> from the request container.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The set of <see cref="IODataRoutingConvention"/> from the request container.</returns>
        public static IEnumerable<IODataRoutingConvention> GetRoutingConventions(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.GetRequestContainer().GetServices<IODataRoutingConvention>();
        }
        /// <summary>
        /// Checks whether the request is a POST targeted at a resource path ending in /$query.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="oDataPath">The OData path.</param>
        /// <returns>true if the request path has $query segment.</returns>
        internal static bool IsQueryRequest(this HttpRequestMessage request, string oDataPath)
        {
            return request.Method.Equals(HttpMethod.Post) && 
                oDataPath?.TrimEnd('/').EndsWith('/' + ODataRouteConstants.QuerySegment, StringComparison.OrdinalIgnoreCase) == true;
        }

        /// <summary>
        /// Transforms a POST request targeted at a resource path ending in $query into a GET request. 
        /// The query options are parsed from the request body and appended to the request URL.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="requestContainerFactory">Request container factory.</param>
        internal static void TransformQueryRequest(this HttpRequestMessage request, Func<IServiceProvider> requestContainerFactory)
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

            string mediaType = request.Content.Headers.ContentType?.MediaType ?? string.Empty;

            if (queryOptionsParser == null)
            {
                throw new ODataException(string.Format(
                    CultureInfo.InvariantCulture,
                    SRResources.CannotFindParserForRequestMediaType,
                    mediaType));
            }

            string queryString = queryOptionsParser.Parse(request.Content.ReadAsStreamAsync().Result);

            string requestPath = request.RequestUri.LocalPath;
            requestPath = requestPath.Substring(0, requestPath.LastIndexOf('/' + ODataRouteConstants.QuerySegment, StringComparison.OrdinalIgnoreCase));

            Uri requestUri = request.RequestUri;
            request.RequestUri = new UriBuilder(requestUri.Scheme, requestUri.Host, requestUri.Port, requestPath, queryString).Uri;
            request.Method = HttpMethod.Get;
        }

        /// <summary>
        /// Gets the set of flags for <see cref="CompatibilityOptions"/> from the http configuration. 
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Set of flags for <see cref="CompatibilityOptions"/> from the http configuration.</returns>
        internal static CompatibilityOptions GetCompatibilityOptions(this HttpRequestMessage request)
        {
            HttpConfiguration configuration = request.GetConfiguration();

            if (configuration == null)
            {
                return CompatibilityOptions.None;
            }

            return configuration.GetCompatibilityOptions();
        }

        private static IServiceScope CreateRequestScope(this HttpRequestMessage request, string routeName)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.Argument("request", SRResources.RequestMustContainConfiguration);
            }

            IServiceProvider rootContainer = configuration.GetODataRootContainer(routeName);
            IServiceScope scope = rootContainer.GetRequiredService<IServiceScopeFactory>().CreateScope();

            // Bind scoping request into the OData container.
            if (routeName != null)
            {
                scope.ServiceProvider.GetRequiredService<HttpRequestScope>().HttpRequest = request;
            }

            return scope;
        }
    }
}
