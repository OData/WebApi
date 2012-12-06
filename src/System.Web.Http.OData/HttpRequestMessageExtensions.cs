// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.OData;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpRequestMessageExtensions
    {
        private const string ODataPathKey = "MS_ODataPath";
        private const string InlineCountPropertyKey = "MS_InlineCount";
        private const string NextPageLinkPropertyKey = "MS_NextPageLink";
        private const string MessageDetailKey = "MessageDetail";

        /// <summary>
        /// Helper method that performs content negotiation and creates a <see cref="HttpResponseMessage"/> representing an error 
        /// with an instance of <see cref="ObjectContent{T}"/> wrapping <paramref name="oDataError"/> as the content. If no formatter 
        /// is found, this method returns a response with status 406 NotAcceptable.
        /// </summary>
        /// <remarks>
        /// This method requires that <paramref name="request"/> has been associated with an instance of
        /// <see cref="HttpConfiguration"/>.
        /// </remarks>
        /// <param name="request">The request.</param>
        /// <param name="statusCode">The status code of the created response.</param>
        /// <param name="oDataError">The OData error to wrap.</param>
        /// <returns>An error response wrapping <paramref name="oDataError"/> with status code <paramref name="statusCode"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "o", Justification = "oDataError is spelled correctly.")]
        public static HttpResponseMessage CreateODataErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, ODataError oDataError)
        {
            HttpConfiguration config = request.GetConfiguration();
            if (config != null && ShouldIncludeErrorDetail(config, request))
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

        internal static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, string message, string messageDetail)
        {
            HttpError error = new HttpError(message);
            HttpConfiguration config = request.GetConfiguration();
            if (config != null && ShouldIncludeErrorDetail(config, request))
            {
                error.Add(MessageDetailKey, messageDetail);
            }
            return request.CreateErrorResponse(statusCode, error);
        }

        // IMPORTANT: This is a slightly modified version of HttpConfiguration.ShouldIncludeErrorDetail
        // That method is internal, so as a workaround the logic is copied here; Work Item #361 tracks making the method public
        // When the work item is fixed, we should be able to remove this copy and use the public method instead
        internal static bool ShouldIncludeErrorDetail(HttpConfiguration config, HttpRequestMessage request)
        {
            switch (config.IncludeErrorDetailPolicy)
            {
                case IncludeErrorDetailPolicy.Default:
                    object includeErrorDetail;
                    if (request.Properties.TryGetValue(HttpPropertyKeys.IncludeErrorDetailKey, out includeErrorDetail))
                    {
                        Lazy<bool> includeErrorDetailLazy = includeErrorDetail as Lazy<bool>;
                        if (includeErrorDetailLazy != null)
                        {
                            return includeErrorDetailLazy.Value;
                        }
                    }

                    goto case IncludeErrorDetailPolicy.LocalOnly;

                case IncludeErrorDetailPolicy.LocalOnly:
                    if (request == null)
                    {
                        return false;
                    }

                    object isLocal;
                    if (request.Properties.TryGetValue(HttpPropertyKeys.IsLocalKey, out isLocal))
                    {
                        Lazy<bool> isLocalLazy = isLocal as Lazy<bool>;
                        if (isLocalLazy != null)
                        {
                            return isLocalLazy.Value;
                        }
                    }
                    return false;

                case IncludeErrorDetailPolicy.Always:
                    return true;

                case IncludeErrorDetailPolicy.Never:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Gets the OData path of the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The OData path of the request</returns>
        public static ODataPath GetODataPath(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object path;
            if (request.Properties.TryGetValue(ODataPathKey, out path))
            {
                return path as ODataPath;
            }
            return null;
        }

        /// <summary>
        /// Sets the OData path for the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="odataPath">The OData path of the request.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "odata", Justification = "odata is spelled correctly")]
        public static void SetODataPath(this HttpRequestMessage request, ODataPath odataPath)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[ODataPathKey] = odataPath;
        }

        /// <summary>
        /// Gets the inline count to use in the OData response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The inline count to send back, or <c>null</c> if one isn't set.</returns>
        public static long? GetInlineCount(this HttpRequestMessage request)
        {
            object inlineCount;
            if (request.Properties.TryGetValue(InlineCountPropertyKey, out inlineCount))
            {
                return inlineCount as long?;
            }
            return null;
        }

        /// <summary>
        /// Gets the next page link to use in the OData response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The next page link to send back, or <c>null</c> if one isn't set.</returns>
        public static Uri GetNextPageLink(this HttpRequestMessage request)
        {
            object nextPageLink;
            if (request.Properties.TryGetValue(NextPageLinkPropertyKey, out nextPageLink))
            {
                return nextPageLink as Uri;
            }
            return null;
        }

        internal static void SetInlineCount(this HttpRequestMessage request, long inlineCount)
        {
            request.Properties[InlineCountPropertyKey] = inlineCount;
        }

        internal static void SetNextPageLink(this HttpRequestMessage request, Uri nextPageLink)
        {
            request.Properties[NextPageLinkPropertyKey] = nextPageLink;
        }
    }
}