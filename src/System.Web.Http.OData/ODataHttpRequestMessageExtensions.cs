// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using SelectExpandClause = Microsoft.Data.OData.Query.SemanticAst.SelectExpandClause;

namespace System.Net.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataHttpRequestMessageExtensions
    {
        private const string EdmModelKey = "MS_EdmModel";
        private const string ODataRouteNameKey = "MS_ODataRouteName";
        private const string ODataRoutingConventionsKey = "MS_ODataRoutingConventions";
        private const string ODataPathKey = "MS_ODataPath";
        private const string ODataPathHandlerKey = "MS_ODataPathHandler";
        private const string InlineCountPropertyKey = "MS_InlineCount";
        private const string NextPageLinkPropertyKey = "MS_NextPageLink";
        private const string MessageDetailKey = "MessageDetail";
        private const string SelectExpandClauseKey = "MS_SelectExpandClause";
        private const string RoutingConventionDataStoreKey = "MS_RoutingConventionDataStore";

        private const string ODataMaxServiceVersion = "MaxDataServiceVersion";

        /// <summary>
        /// Retrieves the EDM model associated with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The EDM model associated with this request, or <c>null</c> if there isn't one.</returns>
        public static IEdmModel GetEdmModel(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object model;
            if (request.Properties.TryGetValue(EdmModelKey, out model))
            {
                return model as IEdmModel;
            }

            return null;
        }

        /// <summary>
        /// Associates the given EDM model with the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="model">The EDM model to associate with the request.</param>
        public static void SetEdmModel(this HttpRequestMessage request, IEdmModel model)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[EdmModelKey] = model;
        }

        /// <summary>
        /// Retrieves the route name to use for generating OData links.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The route name to use for generating OData links associated with this request, or <c>null</c> if there isn't one.</returns>
        public static string GetODataRouteName(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object routeName;
            if (request.Properties.TryGetValue(ODataRouteNameKey, out routeName))
            {
                return routeName as string;
            }

            return null;
        }

        /// <summary>
        /// Sets the given route name to be used for generating OData links.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routeName">The route name to use for generating OData links.</param>
        public static void SetODataRouteName(this HttpRequestMessage request, string routeName)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[ODataRouteNameKey] = routeName;
        }

        /// <summary>
        /// Gets the OData routing conventions to use for controller and action selection.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The OData routing conventions to use for controller and action selection associated with this request,
        /// or <c>null</c> if there aren't any.</returns>
        public static IEnumerable<IODataRoutingConvention> GetODataRoutingConventions(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object routingConventions;
            if (request.Properties.TryGetValue(ODataRoutingConventionsKey, out routingConventions))
            {
                return routingConventions as IEnumerable<IODataRoutingConvention>;
            }

            return null;
        }

        /// <summary>
        /// Sets the OData routing conventions to use for controller and action selection.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="routingConventions">The OData routing conventions to use for controller and action selection.</param>
        public static void SetODataRoutingConventions(this HttpRequestMessage request, IEnumerable<IODataRoutingConvention> routingConventions)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[ODataRoutingConventionsKey] = routingConventions;
        }

        /// <summary>
        /// Gets the <see cref="IODataPathHandler"/> to use for generating links.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The <see cref="IODataPathHandler"/> to use for generating links.</returns>
        public static IODataPathHandler GetODataPathHandler(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object pathHandler;
            if (!request.Properties.TryGetValue(ODataPathHandlerKey, out pathHandler))
            {
                IODataPathHandler defaultPathHandler = new DefaultODataPathHandler();
                request.SetODataPathHandler(defaultPathHandler);
                return defaultPathHandler;
            }
            return pathHandler as IODataPathHandler;
        }

        /// <summary>
        /// Sets the <see cref="IODataPathHandler"/> to use for generating links.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="pathHandler">The <see cref="IODataPathHandler"/> to use for generating links.</param>
        public static void SetODataPathHandler(this HttpRequestMessage request, IODataPathHandler pathHandler)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[ODataPathHandlerKey] = pathHandler;
        }

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

        internal static HttpResponseMessage CreateErrorResponse(this HttpRequestMessage request, HttpStatusCode statusCode, string message, string messageDetail)
        {
            HttpError error = new HttpError(message);
            if (request.ShouldIncludeErrorDetail())
            {
                error.Add(MessageDetailKey, messageDetail);
            }
            return request.CreateErrorResponse(statusCode, error);
        }

        internal static ODataVersion GetODataVersion(this HttpRequestMessage request)
        {
            // OData protocol requires that you send the minimum version that the client needs to know to understand the response.
            // There is no easy way we can figure out the minimum version that the client needs to understand our response. We send response headers much ahead
            // generating the response. So if the requestMessage has a MaxDataServiceVersion, tell the client that our response is of the same version; Else use
            // the DataServiceVersionHeader. Our response might require a higher version of the client and it might fail.
            // If the client doesn't send these headers respond with the default version (V3).
            return GetODataVersionFromHeaders(request.Headers, ODataMaxServiceVersion, ODataMediaTypeFormatter.ODataServiceVersion) ?? ODataMediaTypeFormatter.DefaultODataVersion;
        }

        private static ODataVersion? GetODataVersionFromHeaders(HttpHeaders headers, params string[] headerNames)
        {
            foreach (string headerName in headerNames)
            {
                IEnumerable<string> values;
                if (headers.TryGetValues(headerName, out values))
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
                            // Parsing ODataVersion failed, try next header
                        }
                    }
                }
            }

            return null;
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
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object inlineCount;
            if (request.Properties.TryGetValue(InlineCountPropertyKey, out inlineCount))
            {
                return inlineCount as long?;
            }
            return null;
        }

        /// <summary>
        /// Sets the inline count to use in the OData response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="inlineCount">The inline count to send back to the client.</param>
        public static void SetInlineCount(this HttpRequestMessage request, long inlineCount)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[InlineCountPropertyKey] = inlineCount;
        }

        /// <summary>
        /// Gets the next page link to use in the OData response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The next page link to send back, or <c>null</c> if one isn't set.</returns>
        public static Uri GetNextPageLink(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object nextPageLink;
            if (request.Properties.TryGetValue(NextPageLinkPropertyKey, out nextPageLink))
            {
                return nextPageLink as Uri;
            }
            return null;
        }

        /// <summary>
        /// Sets the next page link to use in the OData response.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="nextPageLink">The next page link to send back to the client.</param>
        public static void SetNextPageLink(this HttpRequestMessage request, Uri nextPageLink)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            request.Properties[NextPageLinkPropertyKey] = nextPageLink;
        }

        /// <summary>
        /// Gets the parsed <see cref="SelectExpandClause"/> of the given request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The parsed <see cref="SelectExpandClause"/> of the given request.</returns>
        public static SelectExpandClause GetSelectExpandClause(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object selectExpandClause;
            if (request.Properties.TryGetValue(SelectExpandClauseKey, out selectExpandClause))
            {
                return selectExpandClause as SelectExpandClause;
            }
            return null;
        }

        /// <summary>
        /// Sets the parsed <see cref="SelectExpandClause"/> for the <see cref="ODataMediaTypeFormatter"/> to use
        /// while writing response for this request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="selectExpandClause">The parsed <see cref="SelectExpandClause"/> of the given request.</param>
        public static void SetSelectExpandClause(this HttpRequestMessage request, SelectExpandClause selectExpandClause)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }
            if (selectExpandClause == null)
            {
                throw Error.ArgumentNull("selectExpandClause");
            }

            request.Properties[SelectExpandClauseKey] = selectExpandClause;
        }

        /// <summary>
        /// Gets the data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>The data store used by <see cref="IODataRoutingConvention"/>s to store any custom route data.</returns>
        public static IDictionary<string, object> GetRoutingConventionsDataStore(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            object dataStore;
            if (request.Properties.TryGetValue(RoutingConventionDataStoreKey, out dataStore))
            {
                return (IDictionary<string, object>)dataStore;
            }
            else
            {
                IDictionary<string, object> dataStoreDictionary = new Dictionary<string, object>();
                request.Properties[RoutingConventionDataStoreKey] = dataStoreDictionary;
                return dataStoreDictionary;
            }
        }
    }
}