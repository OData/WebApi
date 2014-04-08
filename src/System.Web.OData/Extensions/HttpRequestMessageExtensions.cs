// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Properties;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
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
        public static ETag GetETag(this HttpRequestMessage request, EntityTagHeaderValue entityTagHeaderValue)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (entityTagHeaderValue != null)
            {
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
                IEdmEntityType type = odataPath.EdmType as IEdmEntityType;
                if (type != null)
                {
                    IList<string> concurrencyPropertyNames =
                        type.GetConcurrencyProperties().OrderBy(c => c.Name).Select(c => c.Name).AsList();
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
                        etag[nameValue.Key] = nameValue.Value;
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
                }
                : null;
        }
    }
}
