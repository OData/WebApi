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
using System.Web.OData.Properties;
using System.Web.OData.Routing;
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
                IEdmModel model = request.ODataProperties().Model;
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
    }
}
