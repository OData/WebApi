// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Properties;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace System.Web.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        private const string PropertiesKey = "System.Web.OData.Properties";
        private const string RequestContainerKey = "System.Web.OData.RequestContainer";
        private const string RequestScopeKey = "System.Web.OData.RequestScope";

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
                IEdmEntitySet entitySet = odataPath.NavigationSource as IEdmEntitySet;
                if (model != null && entitySet != null)
                {
                    IList<IEdmStructuralProperty> concurrencyProperties = model.GetConcurrencyProperties(entitySet).ToList();
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
            if (request == null || request.RequestUri == null)
            {
                throw Error.ArgumentNull("request");
            }

            Uri requestUri = request.RequestUri;

            if (!requestUri.IsAbsoluteUri)
            {
                throw Error.ArgumentUriNotAbsolute("request", requestUri);
            }

            return GetNextPageLink(requestUri, request.GetQueryNameValuePairs(), pageSize);
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
        /// <exception cref="ODataException">
        /// <paramref name="request"/> already has requestContainer property.
        /// </exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "requestScope will be disposed when the request ends.")]
        public static IServiceProvider CreateRequestContainer(this HttpRequestMessage request, string routeName)
        {
            if (request.Properties.ContainsKey(RequestContainerKey))
            {
                throw new ODataException(SRResources.RequestContainerAlreadyExists);
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

        internal static Uri GetNextPageLink(Uri requestUri, int pageSize)
        {
            Contract.Assert(requestUri != null);
            Contract.Assert(requestUri.IsAbsoluteUri);

            return GetNextPageLink(requestUri, new FormDataCollection(requestUri), pageSize);
        }

        internal static Uri GetNextPageLink(Uri requestUri, IEnumerable<KeyValuePair<string, string>> queryParameters, int pageSize)
        {
            Contract.Assert(requestUri != null);
            Contract.Assert(queryParameters != null);
            Contract.Assert(requestUri.IsAbsoluteUri);

            StringBuilder queryBuilder = new StringBuilder();

            int nextPageSkip = pageSize;

            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                string key = kvp.Key;
                string value = kvp.Value;
                switch (key)
                {
                    case "$top":
                        int top;
                        if (Int32.TryParse(value, out top))
                        {
                            // There is no next page if the $top query option's value is less than or equal to the page size.
                            Contract.Assert(top > pageSize);
                            // We decrease top by the pageSize because that's the number of results we're returning in the current page
                            value = (top - pageSize).ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    case "$skip":
                        int skip;
                        if (Int32.TryParse(value, out skip))
                        {
                            // We increase skip by the pageSize because that's the number of results we're returning in the current page
                            nextPageSkip += skip;
                        }
                        continue;
                    default:
                        break;
                }

                if (key.Length > 0 && key[0] == '$')
                {
                    // $ is a legal first character in query keys
                    key = '$' + Uri.EscapeDataString(key.Substring(1));
                }
                else
                {
                    key = Uri.EscapeDataString(key);
                }
                value = Uri.EscapeDataString(value);

                queryBuilder.Append(key);
                queryBuilder.Append('=');
                queryBuilder.Append(value);
                queryBuilder.Append('&');
            }

            queryBuilder.AppendFormat("$skip={0}", nextPageSkip);

            UriBuilder uriBuilder = new UriBuilder(requestUri)
            {
                Query = queryBuilder.ToString()
            };
            return uriBuilder.Uri;
        }

        private static IServiceProvider GetRootContainer(this HttpRequestMessage request, string routeName)
        {
            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.Argument("request", SRResources.RequestMustContainConfiguration);
            }

            // Requests from OData routes will have RouteName set.
            return routeName != null
                ? configuration.GetODataRootContainer(routeName)
                : configuration.GetNonODataRootContainer();
        }

        private static IServiceScope CreateRequestScope(this HttpRequestMessage request, string routeName)
        {
            return request.GetRootContainer(routeName).GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}
