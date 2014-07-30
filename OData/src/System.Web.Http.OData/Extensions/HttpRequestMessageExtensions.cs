// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        // Maintain the System.Web.Http.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v4 assembly.
        private const string PropertiesKey = "System.Web.Http.OData.Properties";

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
                    new ODataError()
                    {
                        ErrorCode = oDataError.ErrorCode,
                        Message = oDataError.Message,
                        MessageLanguage = oDataError.MessageLanguage
                    });
            }
        }
    }
}
